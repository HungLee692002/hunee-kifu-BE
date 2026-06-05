namespace EnglishCenter.Application.Dtos;

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

public record AuthUserDto(Guid Id, string Username, string FullName, IReadOnlyList<string> Roles);

public record TokenResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    string TokenType,
    AuthUserDto User);
