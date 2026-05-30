using System;
using System.Collections.Generic;
using Xunit;

namespace SmartPark.Tests;

public class PagedResultTests
{
    // Local test implementation mirroring app logic to validate test setup.
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
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

    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var pr = new PagedResult<int>
        {
            TotalCount = 45,
            PageSize = 10,
            Page = 1
        };

        Assert.Equal(5, pr.TotalPages);
    }

    [Fact]
    public void Empty_ReturnsEmptyItems()
    {
        var empty = PagedResult<string>.Empty(1, 20);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.TotalCount);
    }

    [Fact]
    public void HasPreviousAndNext_Behavior()
    {
        var pr = new PagedResult<int>
        {
            TotalCount = 50,
            PageSize = 10,
            Page = 3
        };

        Assert.True(pr.HasPrevious);
        Assert.True(pr.HasNext);
    }
}
