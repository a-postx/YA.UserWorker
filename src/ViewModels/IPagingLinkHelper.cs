namespace YA.TenantWorker.ViewModels
{
    public interface IPagingLinkHelper
    {
        string GetLinkValue<T>(PageResult<T> page, string routeNames) where T : class;
    }
}
