using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Constants
{
    public static class RouteNames
    {
        public const string GetToken = ControllerName.Token + nameof(GetToken);
        public const string GetTenant = ControllerName.Tenant + nameof(GetTenant);
        public const string GetTenantPage = ControllerName.Tenant + nameof(GetTenantPage);
        public const string HeadTenant = ControllerName.Tenant + nameof(HeadTenant);
        public const string HeadTenantPage = ControllerName.Tenant + nameof(HeadTenantPage);
        public const string OptionsTenant = ControllerName.Tenant + nameof(OptionsTenant);
        public const string OptionsTenants = ControllerName.Tenant + nameof(OptionsTenants);
        public const string PostTenant = ControllerName.Tenant + nameof(PostTenant);
        public const string PatchTenant = ControllerName.Tenant + nameof(PatchTenant);
        public const string DeleteTenant = ControllerName.Tenant + nameof(DeleteTenant);
    }
}
