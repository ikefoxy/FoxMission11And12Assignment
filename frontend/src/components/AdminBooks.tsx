import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import type { Book, BookInput } from '../types/book'

type BooksResponse = {
  books: Book[]
  totalBooks: number
}

// Use same-origin by default so backend can serve frontend and API together.
const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''

// Blank form state used for both initial load and "Clear" action.
const emptyForm: BookInput = {
  title: '',
  author: '',
  publisher: '',
  isbn: '',
  classification: '',
  category: '',
  pageCount: 1,
  price: 0,
}

export default function AdminBooks() {
  const [books, setBooks] = useState<Book[]>([])
  const [form, setForm] = useState<BookInput>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [statusMessage, setStatusMessage] = useState('')

  // Loads all books for the admin table.
  const loadBooks = async () => {
    setIsLoading(true)
    setError('')

    try {
      const response = await fetch(
        `${API_BASE}/api/books?pageSize=500&pageNum=1&sortOrder=asc&category=all`,
      )

      if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`)
      }

      const data: BooksResponse = await response.json()
      setBooks(data.books)
    } catch {
      setError('Unable to load books from API.')
      setBooks([])
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    // Initial table load when the page opens.
    void loadBooks()
  }, [])

  // Reset form fields and exit edit mode.
  const resetForm = () => {
    setForm(emptyForm)
    setEditingId(null)
  }

  const isEditing = editingId !== null

  const formTitle = useMemo(
    () => (isEditing ? `Edit Book #${editingId}` : 'Add New Book'),
    [isEditing, editingId],
  )

  // Handles both create and update based on current edit mode.
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSubmitting(true)
    setError('')
    setStatusMessage('')

    try {
      const method = isEditing ? 'PUT' : 'POST'
      const url = isEditing
        ? `${API_BASE}/api/books/${editingId}`
        : `${API_BASE}/api/books`

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(form),
      })

      if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`)
      }

      setStatusMessage(isEditing ? 'Book updated.' : 'Book added.')
      resetForm()
      await loadBooks()
    } catch {
      setError('Unable to save book. Check all fields and try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async (bookId: number) => {
    // Small confirmation guard to prevent accidental deletes.
    const confirmed = window.confirm('Delete this book?')
    if (!confirmed) {
      return
    }

    setError('')
    setStatusMessage('')

    try {
      const response = await fetch(`${API_BASE}/api/books/${bookId}`, {
        method: 'DELETE',
      })

      if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`)
      }

      setStatusMessage('Book deleted.')
      if (editingId === bookId) {
        resetForm()
      }
      await loadBooks()
    } catch {
      setError('Unable to delete book.')
    }
  }

  const startEdit = (book: Book) => {
    // Prefill form with selected row so user can edit inline.
    setEditingId(book.bookID)
    setForm({
      title: book.title,
      author: book.author,
      publisher: book.publisher,
      isbn: book.isbn,
      classification: book.classification,
      category: book.category,
      pageCount: book.pageCount,
      price: book.price,
    })
    setStatusMessage('')
    setError('')
  }

  return (
    <div className="container py-4">
      <div className="d-flex align-items-center justify-content-between mb-3">
        <h1 className="mb-0">Admin Books</h1>
        <Link className="btn btn-outline-primary" to="/">
          Back to Catalog
        </Link>
      </div>

      <p className="text-muted">
        Manage books in the SQLite database (add, update, delete).
      </p>

      {error && <div className="alert alert-danger">{error}</div>}
      {statusMessage && <div className="alert alert-success">{statusMessage}</div>}

      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <h2 className="h5">{formTitle}</h2>
          <form className="row g-3" onSubmit={handleSubmit}>
            <div className="col-md-6">
              <label className="form-label">Title</label>
              <input
                className="form-control"
                value={form.title}
                onChange={(e) => setForm((prev) => ({ ...prev, title: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Author</label>
              <input
                className="form-control"
                value={form.author}
                onChange={(e) => setForm((prev) => ({ ...prev, author: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Publisher</label>
              <input
                className="form-control"
                value={form.publisher}
                onChange={(e) => setForm((prev) => ({ ...prev, publisher: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">ISBN</label>
              <input
                className="form-control"
                value={form.isbn}
                onChange={(e) => setForm((prev) => ({ ...prev, isbn: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Classification</label>
              <input
                className="form-control"
                value={form.classification}
                onChange={(e) => setForm((prev) => ({ ...prev, classification: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Category</label>
              <input
                className="form-control"
                value={form.category}
                onChange={(e) => setForm((prev) => ({ ...prev, category: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-3">
              <label className="form-label">Page Count</label>
              <input
                className="form-control"
                type="number"
                min={1}
                value={form.pageCount}
                onChange={(e) =>
                  setForm((prev) => ({ ...prev, pageCount: Number.parseInt(e.target.value, 10) || 1 }))
                }
                required
              />
            </div>
            <div className="col-md-3">
              <label className="form-label">Price</label>
              <input
                className="form-control"
                type="number"
                min={0}
                step="0.01"
                value={form.price}
                onChange={(e) =>
                  setForm((prev) => ({ ...prev, price: Number.parseFloat(e.target.value) || 0 }))
                }
                required
              />
            </div>
            <div className="col-12 d-flex gap-2">
              <button className="btn btn-primary" disabled={isSubmitting} type="submit">
                {isSubmitting ? 'Saving...' : isEditing ? 'Update Book' : 'Add Book'}
              </button>
              <button className="btn btn-outline-secondary" disabled={isSubmitting} onClick={resetForm} type="button">
                Clear
              </button>
            </div>
          </form>
        </div>
      </div>

      <div className="card shadow-sm">
        <div className="card-body">
          <h2 className="h5">Current Books ({books.length})</h2>
          <div className="table-responsive">
            <table className="table table-striped table-hover table-bordered align-middle">
              <thead className="table-dark">
                <tr>
                  <th>ID</th>
                  <th>Title</th>
                  <th>Author</th>
                  <th>Category</th>
                  <th>Price</th>
                  <th className="text-end">Actions</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan={6} className="text-center py-3">
                      Loading books...
                    </td>
                  </tr>
                ) : books.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="text-center py-3">
                      No books found.
                    </td>
                  </tr>
                ) : (
                  books.map((book) => (
                    <tr key={book.bookID}>
                      <td>{book.bookID}</td>
                      <td>{book.title}</td>
                      <td>{book.author}</td>
                      <td>{book.category}</td>
                      <td>${book.price.toFixed(2)}</td>
                      <td className="text-end">
                        <div className="btn-group">
                          <button className="btn btn-sm btn-outline-primary" onClick={() => startEdit(book)}>
                            Edit
                          </button>
                          <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(book.bookID)}>
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  )
}
