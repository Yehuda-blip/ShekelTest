using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;

namespace ShekelTestPart1
{
    internal class Program
    {
        // run options
        const string POPULATE = "populate";
        const string BENCHMARK = "benchmark";
        const string USAGE = $"""
usage: to populate DB with data - pass '{POPULATE}' and then '<customerCount> <productCount> <orderDetailsCount> <orderCount>'
       to run benchmark - pass '{BENCHMARK}'
""";

        static void Main(string[] args)
        {
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
                    catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
                    {
                        Console.WriteLine(USAGE);
                        return;
                    }
                    pop.Populate();
                    break;

                case BENCHMARK:
                    //BenchmarkProcedure();
                    break;

                default:
                    Console.WriteLine($"unidentefied run option {args[1]}, try {POPULATE} or {BENCHMARK}");
                    break;
            }
        }
    }
}
