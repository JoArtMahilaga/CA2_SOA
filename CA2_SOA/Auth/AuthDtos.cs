namespace CA2SOA.Auth;

public sealed record RegisterRequest(string UserName, string Email, string Password);

public sealed record LoginRequest(string UserNameOrEmail, string Password);

public sealed record AuthUserDto(int Id, string UserName, string Email);

public sealed record AuthResponse(string Token, AuthUserDto User);