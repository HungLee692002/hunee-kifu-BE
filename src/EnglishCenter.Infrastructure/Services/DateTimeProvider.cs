using EnglishCenter.Application.Abstractions;

namespace EnglishCenter.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
