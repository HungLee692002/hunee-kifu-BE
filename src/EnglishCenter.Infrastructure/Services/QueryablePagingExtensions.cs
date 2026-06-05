using EnglishCenter.Application.Common;
using EnglishCenter.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public static class QueryablePagingExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, PagedQuery paging)
        where T : AuditableEntity
    {
        var sortBy = paging.SortBy?.ToLowerInvariant() ?? "updatedat";
        var desc = paging.SortDescending;

        return sortBy switch
        {
            "createdat" => desc
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            _ => desc
                ? query.OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                : query.OrderBy(e => e.UpdatedAt ?? e.CreatedAt),
        };
    }

    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        PagedQuery paging,
        CancellationToken cancellationToken = default)
        where T : AuditableEntity
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .ApplySort(paging)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, paging.Page, paging.PageSize, totalCount);
    }
}
