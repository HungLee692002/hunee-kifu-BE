using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int? year, int? month, CancellationToken cancellationToken = default);
}
