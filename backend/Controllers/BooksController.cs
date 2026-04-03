using backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public sealed class BooksController(IBookRepository bookRepository) : ControllerBase
{
    [HttpGet("categories")]
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
}
