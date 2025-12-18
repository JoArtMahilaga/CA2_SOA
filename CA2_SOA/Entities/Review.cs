namespace CA2SOA.Entities;

public sealed class Review
{
    public int Id { get; set; }

    public int GameId { get; set; }
    public Game? Game { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }


    public int Rating { get; set; }

    public string Comment { get; set; } = "";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}