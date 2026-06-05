namespace EnglishCenter.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
