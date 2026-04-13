namespace backend.Models;

// API response shape for paginated catalog requests.
public sealed class PagedBooksResponse
{
    public List<Book> Books { get; set; } = [];
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalBooks { get; set; }
    public int TotalPages { get; set; }
    public string SortOrder { get; set; } = "asc";
    public string SelectedCategory { get; set; } = "all";
}
