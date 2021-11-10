namespace YA.UserWorker.Constants;

public static class RouteNames
{
    public const string GetUser = ControllerName.Users + nameof(GetUser);
    public const string HeadUser = ControllerName.Users + nameof(HeadUser);
    public const string OptionsUser = ControllerName.Users + nameof(OptionsUser);
    public const string PostUser = ControllerName.Users + nameof(PostUser);
    public const string PatchUser = ControllerName.Users + nameof(PatchUser);
    public const string SwitchUserTenant = ControllerName.Users + nameof(SwitchUserTenant);

    public const string GetTenantById = ControllerName.Tenants + nameof(GetTenantById);
    public const string GetTenantPage = ControllerName.Tenants + nameof(GetTenantPage);
    public const string GetTenant = ControllerName.Tenants + nameof(GetTenant);
    public const string HeadTenant = ControllerName.Tenants + nameof(HeadTenant);
    public const string HeadTenantById = ControllerName.Tenants + nameof(HeadTenantById);
    public const string HeadTenantPage = ControllerName.Tenants + nameof(HeadTenantPage);
    public const string OptionsTenant = ControllerName.Tenants + nameof(OptionsTenant);
    public const string OptionsTenantAll = ControllerName.Tenants + nameof(OptionsTenantAll);
    public const string OptionsTenantById = ControllerName.Tenants + nameof(OptionsTenantById);
    public const string PatchTenantById = ControllerName.Tenants + nameof(PatchTenantById);
    public const string PatchTenant = ControllerName.Tenants + nameof(PatchTenant);
    public const string DeleteTenantById = ControllerName.Tenants + nameof(DeleteTenantById);
    public const string DeleteTenant = ControllerName.Tenants + nameof(DeleteTenant);

    public const string GetInvitation = ControllerName.TenantInvitations + nameof(GetInvitation);
    public const string HeadInvitation = ControllerName.TenantInvitations + nameof(HeadInvitation);
    public const string OptionsInvitation = ControllerName.TenantInvitations + nameof(OptionsInvitation);
    public const string DeleteInvitation = ControllerName.TenantInvitations + nameof(DeleteInvitation);
    public const string OptionsTenantInvitations = ControllerName.TenantInvitations + nameof(OptionsTenantInvitations);
    public const string PostTenantInvitation = ControllerName.TenantInvitations + nameof(PostTenantInvitation);

    public const string OptionsMembership = ControllerName.TenantMemberships + nameof(OptionsMembership);
    public const string DeleteMembership = ControllerName.TenantMemberships + nameof(DeleteMembership);
    public const string OptionsTenantMemberships = ControllerName.TenantInvitations + nameof(OptionsTenantMemberships);
    public const string PostTenantMembership = ControllerName.TenantMemberships + nameof(PostTenantMembership);

    public const string OptionsClientInfo = ControllerName.ClientInfos + nameof(OptionsClientInfo);
    public const string PostClientInfo = ControllerName.ClientInfos + nameof(PostClientInfo);
}
