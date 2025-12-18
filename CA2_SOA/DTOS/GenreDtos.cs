namespace CA2SOA.DTOS;

public sealed record GenreDto(int Id, string Name);
public sealed record CreateGenreRequest(string Name);
public sealed record UpdateGenreRequest(string Name);