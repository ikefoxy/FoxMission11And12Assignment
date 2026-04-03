# FoxMission11And12Assignment

Bookstore app built with ASP.NET Core Web API + React + SQLite.

## Mission 13 Features Added

- Admin page at `/adminbooks`
- Add books to database
- Update books in database
- Delete books from database
- SPA route support file: `frontend/public/routes.json`

## Deployed Azure Site

- Live URL: `http://foxmission13-ikefox-21573.azurewebsites.net`
- Admin page: `http://foxmission13-ikefox-21573.azurewebsites.net/adminbooks`

## Run (TA)

From repo root:

```bash
npm run start:all
```

Open:

- Catalog: `http://localhost:5039/`
- Admin page: `http://localhost:5039/adminbooks`

## Quick API Check

```bash
curl -s "http://localhost:5039/api/categories"
```

```bash
curl -s "http://localhost:5039/api/books?pageSize=5&pageNum=1&sortOrder=asc&category=all"
```

```bash
curl -i -X POST "http://localhost:5039/api/books" -H "Content-Type: application/json" -d '{"title":"TA Test","author":"Tester","publisher":"BYU","isbn":"978-0000000000","classification":"Fiction","category":"Testing","pageCount":123,"price":9.99}'
```

CRUD endpoints:

- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`

## Stop

```bash
kill $(lsof -ti :5039) 2>/dev/null || true
```

## Backend Structure

- `backend/Controllers/BooksController.cs`
- `backend/Data/SqliteBookRepository.cs`
- `backend/Models/Book.cs`
- `backend/Models/BookInput.cs`
- `backend/Models/PagedBooksResponse.cs`
