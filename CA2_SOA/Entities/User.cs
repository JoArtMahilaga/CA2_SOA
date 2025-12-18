namespace CA2SOA.Entities;

public sealed class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";

    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<LibraryEntry> LibraryEntries { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}