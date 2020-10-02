namespace YA.TenantWorker.Application.Models.Dto
{
    /// <summary>
    /// Page options for listing elements
    /// </summary>
    public class PageOptions
    {
        public int? First { get; set; }
        public int? Last { get; set; }
        public string After { get; set; }
        public string Before { get; set; }
    }
}
