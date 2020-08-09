namespace YA.TenantWorker.Constants
{
    public static class RouteNames
    {
        public const string GetToken = ControllerName.Token + nameof(GetToken);

        public const string GetTenant = ControllerName.Tenants + nameof(GetTenant);
        public const string GetTenantById = ControllerName.Tenants + nameof(GetTenantById);
        public const string GetTenantPage = ControllerName.Tenants + nameof(GetTenantPage);
        public const string HeadTenant = ControllerName.Tenants + nameof(HeadTenant);
        public const string HeadTenantById = ControllerName.Tenants + nameof(HeadTenantById);
        public const string HeadTenantPage = ControllerName.Tenants + nameof(HeadTenantPage);
        public const string OptionsTenant = ControllerName.Tenants + nameof(OptionsTenant);
        public const string OptionsTenantAll = ControllerName.Tenants + nameof(OptionsTenantAll);
        public const string OptionsTenantById = ControllerName.Tenants + nameof(OptionsTenantById);
        public const string PostTenant = ControllerName.Tenants + nameof(PostTenant);
        public const string PatchTenantById = ControllerName.Tenants + nameof(PatchTenantById);
        public const string PatchTenant = ControllerName.Tenants + nameof(PatchTenant);
        public const string DeleteTenantById = ControllerName.Tenants + nameof(DeleteTenantById);
        public const string DeleteTenant = ControllerName.Tenants + nameof(DeleteTenant);

        public const string OptionsClientInfo = ControllerName.ClientInfo + nameof(OptionsClientInfo);
        public const string PostClientInfo = ControllerName.ClientInfo + nameof(PostClientInfo);
    }
}
