using backend.Models;

namespace backend.Data;

public interface IBookRepository
{
    IReadOnlyList<string> GetCategories();
    PagedBooksResponse GetBooks(int? pageSize, int? pageNum, string? sortOrder, string? category);
}
