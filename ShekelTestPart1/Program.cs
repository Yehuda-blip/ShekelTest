using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Data.Common;

namespace ShekelTestPart1
{
    internal class Program
    {
        // run options
        const string POPULATE = "populate";
        const string BENCHMARK = "benchmark";
        const string USAGE = $"""
usage: to populate DB with data - pass '{POPULATE}' and then '<customerCount> <productCount> <orderDetailsCount> <orderCount>'
       to run benchmark - pass '{BENCHMARK}' and then <cycles: the number of times each timed query is called>
""";
        const string APPROACH_1_STD_JOIN_DOUBLE_GROUP_BY = """
SELECT 
    P.ProductID,
    P.ProductDesc,
    ISNULL(SUM(OD.Quantity), 0) AS TotalQuantityBought
FROM 
    dbo.Products P
LEFT JOIN 
    dbo.OrderDetails OD ON P.ProductID = OD.ProductID
GROUP BY 
    P.ProductID, P.ProductDesc;
""";
        const string APPROACH_2_SUBQUERY = """
SELECT 
    P.ProductID,
    P.ProductDesc,
    ISNULL(Q.TotalQuantityBought, 0) AS TotalQuantityBought
FROM 
    dbo.Products P
LEFT JOIN 
(
    SELECT 
        ProductID,
        SUM(Quantity) AS TotalQuantityBought
    FROM 
        dbo.OrderDetails
    GROUP BY 
        ProductID
) Q ON P.ProductID = Q.ProductID;
""";


        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(USAGE);
                return;
            }
            switch (args[0])
            {
                case POPULATE:
                    Populator pop;
                    try
                    {
                        pop = new Populator(
                            int.Parse(args[1]),
                            int.Parse(args[2]),
                            int.Parse(args[3]),
                            int.Parse(args[4])
                            );
                    }
                    catch (Exception ex) when (ex is FormatException || ex is IndexOutOfRangeException)
                    {
                        Console.WriteLine(USAGE);
                        return;
                    }
                    pop.Populate();
                    break;

                case BENCHMARK:
                    int cycles;
                    try
                    {
                        cycles = int.Parse(args[1]);
                    }
                    catch (Exception ex) when (ex is FormatException || ex is IndexOutOfRangeException)
                    {
                        Console.WriteLine(USAGE);
                        return;
                    }
                    Console.WriteLine(BenchmarkProcedure(cycles));
                    break;

                default:
                    Console.WriteLine($"unidentefied run option {args[1]}, try {POPULATE} or {BENCHMARK}");
                    break;
            }
        }

        static string BenchmarkProcedure(int cycles)
        {
            List<List<TimeSpan>> timings;
            using (ShekelTestEntityModel ctx = new ShekelTestEntityModel())
            {
                ctx.Database.OpenConnection();
                DbCommand[] queries = new DbCommand[]
                {
                    CreateQuery(ctx, APPROACH_1_STD_JOIN_DOUBLE_GROUP_BY),
                    CreateQuery(ctx, APPROACH_2_SUBQUERY)
                };
                timings = queries.Select(
                    q => Enumerable.Range(0, cycles)
                        .Select(_ =>
                            MonitorRunningTime(() => ReadAllAndLose(q))
                         )
                        .ToList()
                    ).ToList();
                ctx.Database.CloseConnection();
            }

            string output = "";
            for (int i = 0; i < timings.Count; i++)
            {
                TimeSpan max = timings[i].Max();
                TimeSpan min = timings[i].Min();
                double averageTicks = timings[i].Average(time => time.Ticks);
                TimeSpan mean = TimeSpan.FromTicks((long)averageTicks);
                output += $"results on approach {i + 1}:\n    avg: {mean}    max: {max}    min: {min}\n";
            }
            return output;
        }

        static DbCommand CreateQuery(DbContext ctx, string query)
        {
            DbCommand command = ctx.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            return command;
        }

        static void ReadAllAndLose(DbCommand query)
        {
            using (var result = query.ExecuteReader())
            {
                while (result.Read()) ;
            }
        }

        static TimeSpan MonitorRunningTime(Action action)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            action();
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }
    }
}
