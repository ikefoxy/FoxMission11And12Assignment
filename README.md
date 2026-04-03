# FoxMission11And12Assignment

Bookstore app built with ASP.NET Core Web API + React + SQLite.

## Run (TA)

From repo root:

```bash
npm run start:all
```

Open:

`http://localhost:5039`

You should see a book list with category filter, paging, sorting, and cart actions.

## Quick API Check

```bash
curl -s "http://localhost:5039/api/categories"
```

```bash
curl -s "http://localhost:5039/api/books?pageSize=5&pageNum=1&sortOrder=asc&category=all"
```

Both commands should return JSON with data.

## Stop

```bash
kill $(lsof -ti :5039) 2>/dev/null || true
```

## Backend Structure

- `backend/Controllers/BooksController.cs`
- `backend/Data/SqliteBookRepository.cs`
- `backend/Models/Book.cs`
- `backend/Models/PagedBooksResponse.cs`
