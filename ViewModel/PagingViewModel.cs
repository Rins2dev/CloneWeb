namespace ViewModel
{
    public class PagingViewModel
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasPrev => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
