using backend.Models;

namespace backend.Data;

public interface IBookRepository
{
    IReadOnlyList<string> GetCategories();
    PagedBooksResponse GetBooks(int? pageSize, int? pageNum, string? sortOrder, string? category);
    Book? GetBookById(int id);
    Book AddBook(BookInput bookInput);
    bool UpdateBook(int id, BookInput bookInput);
    bool DeleteBook(int id);
}
