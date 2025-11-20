using Microsoft.EntityFrameworkCore;

public class BookDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        if (!string.IsNullOrEmpty(connectionString))
        {
            options.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Isbn).HasMaxLength(20);
            entity.Property(e => e.Publisher).HasMaxLength(255);
            entity.Property(e => e.Year);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Archived).HasDefaultValue(false);
            entity.Property(e => e.ValidatedOn);
        });
    }
}