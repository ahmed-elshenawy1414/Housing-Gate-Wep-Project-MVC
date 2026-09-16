namespace StudentHousing.ViewModels
{
    public class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public static PaginatedResult<T> Empty(int page = 1, int pageSize = 12) => new() { Items = Array.Empty<T>(), TotalCount = 0, Page = page, PageSize = pageSize };
    }
}
