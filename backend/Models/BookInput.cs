using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public sealed class BookInput
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string Publisher { get; set; } = string.Empty;

    [Required]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    public string Classification { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int PageCount { get; set; }

    [Range(0, double.MaxValue)]
    public double Price { get; set; }
}
