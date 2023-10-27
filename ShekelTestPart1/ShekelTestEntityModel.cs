using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShekelTestPart1
{
    internal class ShekelTestEntityModel : DbContext
    {

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Obviously NOT how we store connection strings
            optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;TrustServerCertificate=True");
        }
    }


    public class Customer
    {
        [Key]
        [Column(TypeName = "nchar(5)")]
        public string CustomerID { get; set; }

        [StringLength(40)]
        public string FirstName { get; set; }

        [StringLength(30)]
        public string LastName { get; set; }

        [StringLength(15)]
        public string City { get; set; }
    }


    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [StringLength(40)]
        public string ProductDesc { get; set; }

        public DateTime InsertDate { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }
    }


    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Column(TypeName = "nchar(5)")]
        public string CustomerID { get; set; }

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal PriceSum { get; set; }
    }


    public class OrderDetails
    {
        [Key]
        public int OrderDetailsID { get; set; }

        public int OrderID { get; set; }

        public int ProductID { get; set; }

        [Column(TypeName = "smallint")]
        public short Quantity { get; set; }
    }
}

