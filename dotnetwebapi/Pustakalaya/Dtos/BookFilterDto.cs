namespace Pustakalaya.Dtos;

public class BookFilterDto
{
    public string? Search { get; set; }
    public int? GenreId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Sort { get; set; }
}
