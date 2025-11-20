using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class BookDbContextFactory : IDesignTimeDbContextFactory<BookDbContext>
{
    public BookDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
            ?? throw new InvalidOperationException("SqlConnectionString environment variable is not set.");

        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            })
            .Options;

        return new BookDbContext(options);
    }
}
