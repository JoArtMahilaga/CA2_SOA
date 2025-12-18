namespace CA2SOA.Entities;

public sealed class LibraryEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int GameId { get; set; }
    public Game? Game { get; set; }

    public LibraryStatus Status { get; set; } = LibraryStatus.Backlog;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}