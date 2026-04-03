import './App.css'
import { Link, Route, Routes } from 'react-router-dom'
import AdminBooks from './components/AdminBooks'
import BookList from './components/BookList'

function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <div className="container py-4">
            <div className="d-flex align-items-center justify-content-between mb-3">
              <h1 className="mb-0">Bookstore Catalog</h1>
              <Link className="btn btn-outline-primary" to="/adminbooks">
                Manage Books
              </Link>
            </div>
            <BookList />
          </div>
        }
      />
      <Route path="/adminbooks" element={<AdminBooks />} />
    </Routes>
  )
}

export default App
