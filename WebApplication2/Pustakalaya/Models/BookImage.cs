namespace Pustakalaya.Models;

public class BookImage
{
    public long Id { get; set; }
    public long BookId { get; set; }
    public string Url { get; set; }

    public Book Book { get; set; }
}