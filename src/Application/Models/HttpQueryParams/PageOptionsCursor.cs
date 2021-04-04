namespace YA.UserWorker.Application.Models.HttpQueryParams
{
    /// <summary>
    /// Параметры курсорного запроса постраничного вывода.
    /// </summary>
    public class PageOptionsCursor
    {
        public int? First { get; set; }
        public int? Last { get; set; }
        public string After { get; set; }
        public string Before { get; set; }
    }
}
