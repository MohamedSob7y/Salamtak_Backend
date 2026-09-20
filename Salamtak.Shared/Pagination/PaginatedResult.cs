namespace Salamtak.Shared.Pagination
{
    public class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; }= new List<T>();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages =>PageSize == 0? 0: (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage =>PageNumber > 1;

        public bool HasNextPage =>PageNumber < TotalPages;

        public PaginatedResult()
        {
        }

        public PaginatedResult(
            IReadOnlyList<T> items,
            int pageNumber,
            int pageSize,
            int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}