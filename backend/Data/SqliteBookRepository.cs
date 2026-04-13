using backend.Models;
using Microsoft.Data.Sqlite;

namespace backend.Data;

// SQLite-backed implementation of the bookstore data operations.
public sealed class SqliteBookRepository(IConfiguration configuration, IWebHostEnvironment environment) : IBookRepository
{
    private readonly string _databasePath = ResolveDatabasePath(configuration, environment);

    public IReadOnlyList<string> GetCategories()
    {
        EnsureDatabaseExists();

        var categories = new List<string>();

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT Category
            FROM Books
            ORDER BY Category COLLATE NOCASE ASC
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(reader.GetString(0));
        }

        return categories;
    }

    public PagedBooksResponse GetBooks(int? pageSize, int? pageNum, string? sortOrder, string? category)
    {
        EnsureDatabaseExists();

        // Normalize query inputs so API always behaves predictably.
        var safePageSize = Math.Clamp(pageSize ?? 5, 1, 100);
        var safePageNum = Math.Max(pageNum ?? 1, 1);
        var normalizedSortOrder = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var normalizedCategory = string.IsNullOrWhiteSpace(category) || string.Equals(category, "all", StringComparison.OrdinalIgnoreCase)
            ? null
            : category.Trim();

        var books = new List<Book>();
        var totalBooks = 0;

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        using (var countCommand = connection.CreateCommand())
        {
            // Count first so UI can render accurate pagination controls.
            countCommand.CommandText = normalizedCategory is null
                ? "SELECT COUNT(*) FROM Books"
                : "SELECT COUNT(*) FROM Books WHERE Category = @category";

            if (normalizedCategory is not null)
            {
                countCommand.Parameters.AddWithValue("@category", normalizedCategory);
            }

            totalBooks = Convert.ToInt32(countCommand.ExecuteScalar());
        }

        var offset = (safePageNum - 1) * safePageSize;

        using (var command = connection.CreateCommand())
        {
            // Query only one page of books, with optional category filter.
            command.CommandText = $"""
                SELECT BookID, Title, Author, Publisher, ISBN, Classification, Category, PageCount, Price
                FROM Books
                {(normalizedCategory is null ? string.Empty : "WHERE Category = @category")}
                ORDER BY Title COLLATE NOCASE {normalizedSortOrder}
                LIMIT @pageSize OFFSET @offset
                """;

            if (normalizedCategory is not null)
            {
                command.Parameters.AddWithValue("@category", normalizedCategory);
            }

            command.Parameters.AddWithValue("@pageSize", safePageSize);
            command.Parameters.AddWithValue("@offset", offset);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                books.Add(new Book
                {
                    BookID = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    Publisher = reader.GetString(3),
                    ISBN = reader.GetString(4),
                    Classification = reader.GetString(5),
                    Category = reader.GetString(6),
                    PageCount = reader.GetInt32(7),
                    Price = reader.GetDouble(8)
                });
            }
        }

        var totalPages = totalBooks == 0 ? 1 : (int)Math.Ceiling(totalBooks / (double)safePageSize);

        return new PagedBooksResponse
        {
            Books = books,
            CurrentPage = safePageNum,
            PageSize = safePageSize,
            TotalBooks = totalBooks,
            TotalPages = totalPages,
            SortOrder = normalizedSortOrder.ToLowerInvariant(),
            SelectedCategory = normalizedCategory ?? "all"
        };
    }

    public Book? GetBookById(int id)
    {
        EnsureDatabaseExists();

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT BookID, Title, Author, Publisher, ISBN, Classification, Category, PageCount, Price
            FROM Books
            WHERE BookID = @bookId
            """;
        command.Parameters.AddWithValue("@bookId", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Book
        {
            BookID = reader.GetInt32(0),
            Title = reader.GetString(1),
            Author = reader.GetString(2),
            Publisher = reader.GetString(3),
            ISBN = reader.GetString(4),
            Classification = reader.GetString(5),
            Category = reader.GetString(6),
            PageCount = reader.GetInt32(7),
            Price = reader.GetDouble(8)
        };
    }

    public Book AddBook(BookInput bookInput)
    {
        EnsureDatabaseExists();

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        using var command = connection.CreateCommand();
        // Insert then read back the row so caller gets the saved record (including generated ID).
        command.CommandText = """
            INSERT INTO Books (Title, Author, Publisher, ISBN, Classification, Category, PageCount, Price)
            VALUES (@title, @author, @publisher, @isbn, @classification, @category, @pageCount, @price);
            SELECT last_insert_rowid();
            """;
        AddBookParameters(command, bookInput);

        var newId = Convert.ToInt32(command.ExecuteScalar());
        return GetBookById(newId) ?? throw new InvalidOperationException("Book insert succeeded but readback failed.");
    }

    public bool UpdateBook(int id, BookInput bookInput)
    {
        EnsureDatabaseExists();

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Books
            SET
                Title = @title,
                Author = @author,
                Publisher = @publisher,
                ISBN = @isbn,
                Classification = @classification,
                Category = @category,
                PageCount = @pageCount,
                Price = @price
            WHERE BookID = @bookId
            """;
        command.Parameters.AddWithValue("@bookId", id);
        AddBookParameters(command, bookInput);

        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteBook(int id)
    {
        EnsureDatabaseExists();

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Books WHERE BookID = @bookId";
        command.Parameters.AddWithValue("@bookId", id);

        return command.ExecuteNonQuery() > 0;
    }

    private static void AddBookParameters(SqliteCommand command, BookInput bookInput)
    {
        // Trim string inputs to avoid storing accidental leading/trailing spaces.
        command.Parameters.AddWithValue("@title", bookInput.Title.Trim());
        command.Parameters.AddWithValue("@author", bookInput.Author.Trim());
        command.Parameters.AddWithValue("@publisher", bookInput.Publisher.Trim());
        command.Parameters.AddWithValue("@isbn", bookInput.ISBN.Trim());
        command.Parameters.AddWithValue("@classification", bookInput.Classification.Trim());
        command.Parameters.AddWithValue("@category", bookInput.Category.Trim());
        command.Parameters.AddWithValue("@pageCount", bookInput.PageCount);
        command.Parameters.AddWithValue("@price", bookInput.Price);
    }

    private void EnsureDatabaseExists()
    {
        // Fail fast with a clear error if the expected SQLite file is missing.
        if (!File.Exists(_databasePath))
        {
            throw new FileNotFoundException($"Database file not found at {_databasePath}");
        }
    }

    private static string ResolveDatabasePath(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Prefer configured path, then common local defaults used by this repo.
        var configuredPath = configuration["BookstoreDatabasePath"];
        var candidatePaths = new List<string>();

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            candidatePaths.Add(
                Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath)));
        }

        candidatePaths.Add(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "Bookstore.sqlite")));
        candidatePaths.Add(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Bookstore.sqlite")));

        foreach (var path in candidatePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return candidatePaths[0];
    }
}
