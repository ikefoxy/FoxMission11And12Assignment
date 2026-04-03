# FoxMission11And12Assignment

Bookstore app built with ASP.NET Core Web API + React + SQLite.

## Mission 13 Status

- Phase branch created: `phase6`
- Admin page route added: `/adminbooks`
- Book CRUD implemented:
  - Add books
  - Update books
  - Delete books
- SPA deep-link file added: `frontend/public/routes.json`
- App deployed to Azure

## Azure Links

- Live site: `https://foxmission13-ikefox-21573.azurewebsites.net`
- Admin page: `https://foxmission13-ikefox-21573.azurewebsites.net/adminbooks`

## Local Run Steps (TA)

From the repo root:

```bash
npm run start:all
```

Open:

- Catalog: `http://localhost:5039/`
- Admin page: `http://localhost:5039/adminbooks`

## API Checks (TA)

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

## Stop Local App

```bash
kill $(lsof -ti :5039) 2>/dev/null || true
```

## Backend Structure

- `backend/Controllers/BooksController.cs`
- `backend/Data/IBookRepository.cs`
- `backend/Data/SqliteBookRepository.cs`
- `backend/Models/Book.cs`
- `backend/Models/BookInput.cs`
- `backend/Models/PagedBooksResponse.cs`
