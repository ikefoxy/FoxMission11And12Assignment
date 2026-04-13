using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
// Thin API layer: receives HTTP requests and delegates database work to the repository.
public sealed class BooksController(IBookRepository bookRepository) : ControllerBase
{
    [HttpGet("categories")]
    // Returns unique book categories for the catalog filter dropdown.
    public IActionResult GetCategories()
    {
        try
        {
            return Ok(bookRepository.GetCategories());
        }
        catch (FileNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("books")]
    // Returns paged/sorted books and pagination metadata for the UI.
    public IActionResult GetBooks([FromQuery] int? pageSize, [FromQuery] int? pageNum, [FromQuery] string? sortOrder, [FromQuery] string? category)
    {
        try
        {
            return Ok(bookRepository.GetBooks(pageSize, pageNum, sortOrder, category));
        }
        catch (FileNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("books/{id:int}")]
    // Looks up a single book by numeric ID.
    public IActionResult GetBookById(int id)
    {
        try
        {
            var book = bookRepository.GetBookById(id);
            return book is null ? NotFound() : Ok(book);
        }
        catch (FileNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("books")]
    // Creates a new book and returns 201 with a location header.
    public IActionResult AddBook([FromBody] BookInput bookInput)
    {
        try
        {
            var createdBook = bookRepository.AddBook(bookInput);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.BookID }, createdBook);
        }
        catch (FileNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("books/{id:int}")]
    // Updates an existing book; returns 404 if that ID does not exist.
    public IActionResult UpdateBook(int id, [FromBody] BookInput bookInput)
    {
        try
        {
            var updated = bookRepository.UpdateBook(id, bookInput);
            if (!updated)
            {
                return NotFound();
            }

            var updatedBook = bookRepository.GetBookById(id);
            return Ok(updatedBook);
        }
        catch (FileNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("books/{id:int}")]
    // Deletes a book by ID; returns 204 when successful.
    public IActionResult DeleteBook(int id)
    {
        try
        {
            var deleted = bookRepository.DeleteBook(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (FileNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
