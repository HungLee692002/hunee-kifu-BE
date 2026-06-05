using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Enums;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Options;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnglishCenter.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly IDateTimeProvider _clock;

    public AuthService(AppDbContext db, IOptions<JwtOptions> jwt, IDateTimeProvider clock)
    {
        _db = db;
        _jwt = jwt.Value;
        _clock = clock;
    }

    public Task<TokenResponse> LoginAsync(LoginRequest request, string? clientIp, CancellationToken cancellationToken = default) =>
        IssueTokensForUserAsync(request.Username, request.Password, clientIp, cancellationToken);

    public async Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, string? clientIp, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(request.RefreshToken);
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, cancellationToken)
            ?? throw new NotFoundException("Refresh token không hợp lệ.");

        if (stored.ExpiresAt <= _clock.UtcNow)
            throw new NotFoundException("Refresh token đã hết hạn.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken)
            ?? throw new NotFoundException("Người dùng không tồn tại.");

        if (user.Status != UserStatus.Active)
            throw new BusinessException("Tài khoản không hoạt động.", 401);

        stored.RevokedAt = _clock.UtcNow;
        return await CreateTokenResponseAsync(user, clientIp, cancellationToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(request.RefreshToken);
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, cancellationToken);

        if (stored is null)
            return;

        stored.RevokedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<TokenResponse> IssueTokensForUserAsync(
        string username,
        string password,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken)
            ?? throw new NotFoundException("Tên đăng nhập hoặc mật khẩu không đúng.");

        if (user.Status != UserStatus.Active || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new NotFoundException("Tên đăng nhập hoặc mật khẩu không đúng.");

        user.LastLoginAt = _clock.UtcNow;
        return await CreateTokenResponseAsync(user, clientIp, cancellationToken);
    }

    private async Task<TokenResponse> CreateTokenResponseAsync(User user, string? clientIp, CancellationToken cancellationToken)
    {
        var roles = await GetRoleCodesAsync(user.Id, cancellationToken);
        var accessToken = CreateAccessToken(user, roles);
        var refreshToken = GenerateRefreshToken();
        StoreRefreshToken(user.Id, refreshToken, clientIp);
        await _db.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            _jwt.AccessMinutes * 60,
            refreshToken,
            "Bearer",
            new AuthUserDto(user.Id, user.Username, user.FullName, roles));
    }

    private void StoreRefreshToken(Guid userId, string refreshToken, string? clientIp)
    {
        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = _clock.UtcNow.AddDays(_jwt.RefreshDays),
            CreatedAt = _clock.UtcNow,
            CreatedByIp = clientIp,
        });
    }

    private string CreateAccessToken(User user, IReadOnlyList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("fullName", user.FullName),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: _clock.UtcNow.AddMinutes(_jwt.AccessMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<IReadOnlyList<string>> GetRoleCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.Code
        ).Distinct().ToListAsync(cancellationToken);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
