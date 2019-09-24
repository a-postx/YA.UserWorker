namespace YA.TenantWorker.Application.Models.ViewModels
{
    public interface IPagingLinkHelper
    {
        string GetLinkValue<T>(PageResult<T> page, string routeNames) where T : class;
    }
}
