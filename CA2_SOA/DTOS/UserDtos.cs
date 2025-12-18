namespace CA2SOA.DTOS;

public sealed record UserDto(int Id, string UserName, string Email, DateTime CreatedUtc);