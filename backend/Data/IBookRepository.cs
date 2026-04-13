using backend.Models;

namespace backend.Data;

// Contract used by controllers so data access can be swapped without changing API logic.
public interface IBookRepository
{
    IReadOnlyList<string> GetCategories();
    PagedBooksResponse GetBooks(int? pageSize, int? pageNum, string? sortOrder, string? category);
    Book? GetBookById(int id);
    Book AddBook(BookInput bookInput);
    bool UpdateBook(int id, BookInput bookInput);
    bool DeleteBook(int id);
}
