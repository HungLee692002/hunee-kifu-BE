namespace EnglishCenter.Application.Common;

public class PagedQuery
{
    private int _pageSize = 20;

    public int Page { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is 10 or 20 or 50 or 100 ? value : 20;
    }
    public string SortBy { get; set; } = "updatedAt";
    public string SortDir { get; set; } = "desc";

    public bool SortDescending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);
}
