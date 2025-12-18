namespace CA2SOA.DTOS;

public sealed record GameDto(int Id, string Title, string Platform, int GenreId, string GenreName);
public sealed record CreateGameRequest(string Title, string Platform, int GenreId);
public sealed record UpdateGameRequest(string Title, string Platform, int GenreId);