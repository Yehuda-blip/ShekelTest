using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class MyDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.U
        //optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True");
    }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    // other properties...
}
