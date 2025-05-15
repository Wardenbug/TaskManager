namespace Core.Domain
{
    public class PaginationParams
    {
        const int maxPageSize = 50;
        private int _pageSize = 10; 

        public int PageNumber { get; set; } = 1;
        public string SearchTerm { get; set; } = "";

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
