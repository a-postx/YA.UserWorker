namespace YA.TenantWorker.Application.Dto.ViewModels
{
    public interface IPagingLinkHelper
    {
        string GetLinkValue<T>(PageResult<T> page, string routeNames) where T : class;
    }
}
