namespace SmartPark.Models;

/// <summary>
/// Lightweight container for a single page of results returned from a
/// server-side paginated query. <see cref="TotalCount"/> reflects the total
/// matching rows in the database, not just the page size, so the UI can
/// render a complete pager (page X of Y).
/// </summary>
/// <typeparam name="T">Row type for the page.</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Empty(int page, int pageSize) => new()
    {
        Items = Array.Empty<T>(),
        TotalCount = 0,
        Page = page,
        PageSize = pageSize
    };
}
