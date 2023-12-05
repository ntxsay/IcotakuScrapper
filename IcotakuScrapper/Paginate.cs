namespace IcotakuScrapper;

public readonly struct Paginate<T> where T : notnull
{
    public uint CurrentPage { get; init; } = 1;
    public uint TotalPages { get; init; } = 1;
    public uint MaxItemsPerPage { get; init; }
    public uint TotalItems { get; init; } = 0;
    public IReadOnlyCollection<T> Items { get; init; } = [];

    public Paginate(uint currentPage, uint totalPages, uint maxItemsPerPage, uint totalItems, IReadOnlyCollection<T> items)
    {
        CurrentPage = currentPage;
        TotalPages = totalPages;
        MaxItemsPerPage = maxItemsPerPage;
        TotalItems = totalItems;
        Items = items;
    }
}