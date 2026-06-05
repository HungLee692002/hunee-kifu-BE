namespace EnglishCenter.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
