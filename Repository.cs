using Microsoft.EntityFrameworkCore;

public interface IBookRepository
{
    Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default);
    Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(string id, Book updatedBook, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task PurgeAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> ValidateAndArchiveOldBooksAsync(int archiveThresholdYears = 10, CancellationToken cancellationToken = default);
}

public class Repository : IBookRepository
{
    private readonly BookDbContext _db;

    public Repository(BookDbContext db) => _db = db;

    public async Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.Id = Guid.NewGuid().ToString();
        await _db.Books.AddAsync(book, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return book;
    }

    public async Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Books.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Book?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<bool> UpdateAsync(string id, Book updatedBook, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existing is null)
            return false;

        existing.Title = updatedBook.Title;
        existing.Author = updatedBook.Author;
        existing.Isbn = updatedBook.Isbn;
        existing.Publisher = updatedBook.Publisher;
        existing.Year = updatedBook.Year;
        existing.Description = updatedBook.Description;
        existing.Archived = updatedBook.Archived;
        existing.ValidatedOn = updatedBook.ValidatedOn;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existing is null)
            return false;

        _db.Books.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        _db.Books.RemoveRange(_db.Books);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        await _db.Books.CountAsync(cancellationToken);

    public async Task<int> ValidateAndArchiveOldBooksAsync(
        int archiveThresholdYears = 10,
        CancellationToken cancellationToken = default)
    {
        var cutoffYear = DateTime.UtcNow.Year - archiveThresholdYears;
        var now = DateTime.UtcNow;

        var booksToUpdate = await _db.Books
            .Where(b => !b.Archived && b.Year <= cutoffYear)
            .ToListAsync(cancellationToken);

        foreach (var book in booksToUpdate)
        {
            book.Archived = true;
            book.ValidatedOn = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return booksToUpdate.Count;
    }
}
