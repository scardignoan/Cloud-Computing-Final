using System.Text.Json.Serialization;




public class Book
{
    // Maps the C# property `Id` to the JSON field named "id".
    // This is typically used as a unique identifier for the book.
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    // Maps to the JSON field "title". The book's human-readable title.
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    // Maps to the JSON field "author". The author name(s) of the book.
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    // Maps to the JSON field "isbn". International Standard Book Number.
    // Stored as a string because ISBNs can contain leading zeros and
    // may include dashes in some representations.
    [JsonPropertyName("isbn")]
    public string Isbn { get; set; } = string.Empty;
    
    // Maps to the JSON field "publisher". The publisher of the book.
    [JsonPropertyName("publisher")]
    public string Publisher { get; set; } = string.Empty;
    
    // Maps to the JSON field "year". Publication year as an integer.
    // If you expect unknown years, consider using "int?" (nullable int).
    [JsonPropertyName("year")]
    public int Year { get; set; }
    
    // Maps to the JSON field "description". A short description or summary.
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    // NEW: Governance fields for validation audit trail
    // Maps to the JSON field "archived". Indicates if the book should be archived.
    // Set to true by the validation endpoint if year < current year - 10.
    [JsonPropertyName("archived")]
    public bool Archived { get; set; } = false;

    // Maps to the JSON field "validatedOn". Timestamp of last validation.
    // Provides audit trail for governance and compliance tracking.
    // Null if never validated.
    [JsonPropertyName("validatedOn")]
    public DateTime? ValidatedOn { get; set; }
}