using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

  /// <summary>POST /auth/tokens — đăng nhập hoặc refresh (REST).</summary>
    [HttpPost("tokens")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<TokenResponse>> CreateToken([FromBody] TokenRequestBody body, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        TokenResponse result = body.GrantType?.ToLowerInvariant() switch
        {
            "refresh_token" => await _authService.RefreshAsync(
                new RefreshTokenRequest(body.RefreshToken ?? string.Empty), ip, ct),
            _ => await _authService.LoginAsync(
                new LoginRequest(body.Username ?? string.Empty, body.Password ?? string.Empty), ip, ct),
        };
        return Created("/api/v1/auth/tokens", result);
    }

    [HttpDelete("tokens")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeToken([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request, ct);
        return NoContent();
    }
}

public class TokenRequestBody
{
    public string? GrantType { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? RefreshToken { get; set; }
}
