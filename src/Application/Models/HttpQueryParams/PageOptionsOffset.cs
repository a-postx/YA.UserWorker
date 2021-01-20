namespace YA.TenantWorker.Application.Models.HttpQueryParams
{
    /// <summary>
    /// Параметры оступного запроса постраничного вывода.
    /// </summary>
    public class PageOptionsOffset
    {
        public PageOptionsOffset()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        public PageOptionsOffset(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize > 100 ? 100 : pageSize;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
