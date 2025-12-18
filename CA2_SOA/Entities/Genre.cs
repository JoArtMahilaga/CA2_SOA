namespace CA2SOA.Entities;

public sealed class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public List<Game> Games { get; set; } = new();
}