namespace CA2SOA.Entities;

public sealed class Game
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Platform { get; set; } = "";

    public int GenreId { get; set; }
    public Genre? Genre { get; set; }

    public List<LibraryEntry> LibraryEntries { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}