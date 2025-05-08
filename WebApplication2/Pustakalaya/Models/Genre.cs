namespace Pustakalaya.Models
{
    public class Genre
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public List<Book> Books { get; set; } = new();
    }
}