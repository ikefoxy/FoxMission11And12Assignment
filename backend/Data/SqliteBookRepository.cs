using backend.Models;
using Microsoft.Data.Sqlite;

namespace backend.Data;

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

    private void EnsureDatabaseExists()
    {
        if (!File.Exists(_databasePath))
        {
            throw new FileNotFoundException($"Database file not found at {_databasePath}");
        }
    }

    private static string ResolveDatabasePath(IConfiguration configuration, IWebHostEnvironment environment)
    {
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
