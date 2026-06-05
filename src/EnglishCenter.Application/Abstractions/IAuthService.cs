using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, string? clientIp, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, string? clientIp, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
