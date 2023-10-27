using EFCore.BulkExtensions;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ShekelTestPart1
{
    internal class Populator
    {
        // some population methods here use a random generator
        // to try and ensure uniform distributions over
        // some of the population parameters. While we try
        // to avoid randomness in general for the population,
        // in some cases it seems like the only correct choice.
        const int ORDER_DETAILS_RANDOM_SEED = 0;
        Random rand = new Random(ORDER_DETAILS_RANDOM_SEED);

        const string CUSTOMER_ID_PREFIX = "c_"; // Customer.CustomerID has some max length, consider if changing
        static readonly DateTime FIRST_INSERT_TIME = DateTime.UnixEpoch.AddYears(30);
        const int ADD_HOURS_TIME_INCREMENTOR_FOR_ORDERS = 10;
        const int CITY_COUNT = 15;
        const short PRODUCT_QUANTITY_RANGE = 5;

        // parameters
        readonly int CUSTOMER_COUNT; 
        readonly int PRODUCT_COUNT;
        readonly int ORDER_DETAILS_COUNT;
        readonly int ORDER_COUNT;

        internal Populator(int customerCount, int productCount, int orderDetailsCount, int orderCount)
        {
            if (customerCount == 0 || productCount == 0 || orderDetailsCount == 0 || orderCount == 0)
                throw new ArgumentException("All count parameters must be strictly positive");
            if (orderCount > orderDetailsCount)
                throw new ArgumentException("must have orderDetails count larger or equal to order count");

            Type customerType = typeof(Customer);
            PropertyInfo property = customerType.GetProperty("CustomerID");
            ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null || 
                columnAttribute.TypeName.IsNullOrEmpty() ||
                !Regex.IsMatch(columnAttribute.TypeName, @"nchar\(\d+\)"))
            {
                throw new InvalidOperationException("The type of customer ID is incorrectly configured," +
                    " this is a software bug and not a user input issue.");
            }

            string idLengthStr = Regex.Match(columnAttribute.TypeName, @"\d+").Value;
            int idLength = int.Parse(idLengthStr);
            if (String.Format(CUSTOMER_ID_PREFIX, customerCount).Length > idLength)
            {
                throw new ArgumentException($"Customer count parameter must be less than {idLength - CUSTOMER_ID_PREFIX.Length}");
            }

            CUSTOMER_COUNT = customerCount;
            PRODUCT_COUNT = productCount;
            ORDER_DETAILS_COUNT = orderDetailsCount;
            ORDER_COUNT = orderCount;
        }

        public void Populate()
        {
            List<Customer> customers = GenerateCustomers(CUSTOMER_COUNT, CUSTOMER_ID_PREFIX);
            List<Product> products = GenerateProducts(PRODUCT_COUNT, FIRST_INSERT_TIME);
            List<OrderDetails> orderDetailsCollection = GenerateOrderDetails(ORDER_DETAILS_COUNT, PRODUCT_QUANTITY_RANGE, ORDER_COUNT, customers, products);
            List<Order> orders = AggregateOrders(orderDetailsCollection, customers, products);

            using (ShekelTestEntityModel ctx = new ShekelTestEntityModel())
            {
                ctx.BulkInsert(customers);
                Console.WriteLine("Inserted customer data to DB");
                ctx.BulkInsert(products);
                Console.WriteLine("Inserted product data to DB");
                ctx.BulkInsert(orderDetailsCollection);
                Console.WriteLine("Inserted orderDetails data to DB");
                ctx.BulkInsert(orders);
                Console.WriteLine("Inserted orders data to DB");
            };
            Console.WriteLine("Finished inserting data to DB");
        }

        private List<Customer> GenerateCustomers(int count, string idPrefix)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Customer
                {
                    CustomerID = idPrefix + i,
                    FirstName = $"FirstName_{i}",
                    LastName = $"LastName_{i}",
                    City = $"City_{i % CITY_COUNT + 1}"
                })
                .ToList();
        }

        private List<Product> GenerateProducts(int count, DateTime firstInsert)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Product
                {
                    ProductID = i,
                    ProductDesc = $"Description_{i}",
                    InsertDate = firstInsert.AddMinutes(i),
                    Price = decimal.One + i
                })
                .ToList();
        }

        private List<OrderDetails> GenerateOrderDetails(int count, short quantity_range, int orderCount, List<Customer> customers, List<Product> products)
        {
            return Enumerable.Range(1, count)
                .Select(i => new OrderDetails
                {
                    OrderDetailsID = i,
                    OrderID = i % orderCount + 1,
                    ProductID = products[rand.Next(products.Count)].ProductID,
                    Quantity = (short)rand.Next(1, quantity_range),
                })
                .ToList();
        }

        private List<Order> AggregateOrders(List<OrderDetails> orderDetails, List<Customer> customers, List<Product> products)
        {
            // here we create a joined collection of orderDetails and the product they reference, 
            // group them by order, and sort them by orderID. This is because we want to ensure
            // that the OrderDate fields in the returned Order collection match the id order,
            // as we assume OrderID is a primary key and should be incrementing (although this is not
            // enforced by the DB as far as I know). An argument could be made that if we add this
            // complexity here, then some constraints should be placed on the OrderDetailsID in the 
            // DB to match the Orders, but this is not the current implementation (because it's not
            // very interesting to implement - this method is not simply generating data by some pattern
            // because it servers as an example of a bit more complex software, and I would not add it if I
            // wasn't trying to impress someone).
            var groupedJoins = orderDetails.Join(products, od => od.ProductID, p => p.ProductID,
                (od, p) => new
                {
                    od = od,
                    p = p
                })
                .GroupBy(joinedValue => joinedValue.od.OrderID)
                .OrderBy(group => group.Key);

            if (groupedJoins.IsNullOrEmpty())
            {
                throw new Exception("After joining and grouping orderDetails and products, result was null or empty." +
                    "This could indicate that orderDetails collection contains OrderDetails objects with ProductIDs" +
                    "which are not in products, or that one of the collections is empty.");
            }

            List<Order> orders = new List<Order>();

            // Must insure that every order happens after all the products in it were added
            DateTime timeIncrementor = groupedJoins.First().MaxBy(joinedValue => joinedValue.p.InsertDate).p.InsertDate;
            foreach (var gj in groupedJoins)
            {
                DateTime lastProductInsertInOrder = gj.MaxBy(joinedValue => joinedValue.p.InsertDate).p.InsertDate;
                timeIncrementor = lastProductInsertInOrder > timeIncrementor ? lastProductInsertInOrder : timeIncrementor;
                timeIncrementor = timeIncrementor.AddHours(ADD_HOURS_TIME_INCREMENTOR_FOR_ORDERS);
                orders.Add(new Order
                {
                    OrderID = gj.First().od.OrderID,
                    CustomerID = customers[rand.Next(customers.Count)].CustomerID,
                    OrderDate = new DateTime(timeIncrementor.Ticks),
                    PriceSum = gj.Sum(joinedValue => joinedValue.p.Price * joinedValue.od.Quantity)
                });
            }

            return orders;
        }
    }
}
