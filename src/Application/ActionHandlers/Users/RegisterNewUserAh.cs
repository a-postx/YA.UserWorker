using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using YA.Common.Constants;
using YA.UserWorker.Application.Features.Memberships.Commands;
using YA.UserWorker.Application.Features.TenantInvitations.Commands;
using YA.UserWorker.Application.Features.TenantInvitations.Queries;
using YA.UserWorker.Application.Features.Tenants.Commands;
using YA.UserWorker.Application.Features.Tenants.Queries;
using YA.UserWorker.Application.Features.Users.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users;

public class RegisterNewUserAh : IRegisterNewUserAh
{
    public RegisterNewUserAh(ILogger<RegisterNewUserAh> logger,
        IActionContextAccessor actionCtx,
        IRuntimeContextAccessor runtimeContext,
        IAuthProviderManager authProviderManager,
        IMediator mediator,
        IMapper mapper,
        IHttpClientFactory httpClientFactory)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
        _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        _authProviderManager = authProviderManager ?? throw new ArgumentNullException(nameof(authProviderManager));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    private readonly ILogger<RegisterNewUserAh> _log;
    private readonly IActionContextAccessor _actionCtx;
    private readonly IRuntimeContextAccessor _runtimeCtx;
    private readonly IAuthProviderManager _authProviderManager;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task<IActionResult> ExecuteAsync(UserRegistrationInfoSm regInfo, CancellationToken cancellationToken)
    {
        (string authId, string userId) = _runtimeCtx.GetUserIdentifiers();

        JwtSecurityTokenHandler jwtHandler = new();

        SecurityToken securityToken;

        try
        {
            securityToken = jwtHandler.ReadToken(regInfo.AccessToken);
        }
        catch (ArgumentNullException)
        {
            throw new InvalidOperationException("Security token is null.");
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("Security token is not well formed.");
        }

        if (securityToken is null)
        {
            throw new InvalidOperationException("Security token cannot be decoded.");
        }

        JwtSecurityToken jst = securityToken as JwtSecurityToken;
        string userInfoUri = jst.Audiences.FirstOrDefault(e => e.Contains("userinfo", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(userInfoUri))
        {
            throw new InvalidOperationException("Userinfo endpoint cannot be obtained.");
        }

        DetailedUserInfoTm userInfo;

        using (HttpRequestMessage detailsRequest = new(HttpMethod.Get, userInfoUri))
        using (HttpClient client = _httpClientFactory.CreateClient())
        {
            string auth0userId = authId + "|" + userId;

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", regInfo.AccessToken);
            string correlationId = _runtimeCtx.GetCorrelationId().ToString();
            client.DefaultRequestHeaders.Add("x-correlation-id", correlationId);

            HttpResponseMessage detailsResponse = await client.SendAsync(detailsRequest, cancellationToken);

            if (!detailsResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cannot get user info for {auth0userId}, status code: {detailsResponse.StatusCode}");
            }

            _log.LogInformation("{UserId} detailed info has been received", auth0userId);

            string detailedInfoContent = await detailsResponse.Content.ReadAsStringAsync(cancellationToken);

            userInfo = JsonSerializer
                .Deserialize<DetailedUserInfoTm>(detailedInfoContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        if (userInfo is null)
        {
            throw new InvalidOperationException("Detailed user info cannot be obtained.");
        }

        ClaimsPrincipal userCtx = _actionCtx.ActionContext.HttpContext.User;
        string userEmail = userCtx.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email)?.Value;

        ICommandResult<User> userResult = await _mediator
            .Send(new CreateUserCommand(authId, userId, userInfo.Name, userInfo.Given_Name,
                userInfo.Family_Name, userEmail, userInfo.Picture, userInfo.NickName), cancellationToken);

        switch (userResult.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(userResult.Status), userResult.Status, "Unexpected result on creating user");
            case CommandStatus.UnprocessableEntity:
                return new UnprocessableEntityResult();
            case CommandStatus.Ok:
                break;
        }

        YaInvitation userInvitation = null;
        Tenant userTenant = null;
        Membership userMembership = null;
        YaMembershipAccessType accessType = YaMembershipAccessType.None;

        bool inviteToExistingTenant = regInfo.JoinTeamToken.HasValue && regInfo.JoinTeamToken.Value != Guid.Empty;

        if (inviteToExistingTenant)
        {
            ICommandResult<YaInvitation> inviteResult = await _mediator
                .Send(new GetInvitationCommand(regInfo.JoinTeamToken.Value), cancellationToken);

            switch (inviteResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(inviteResult.Status), inviteResult.Status, "Unexpected result on getting invitation");
                case CommandStatus.Ok:
                    userInvitation = inviteResult.Data;
                    break;
            }

            if (userInvitation != null)
            {
                accessType = userInvitation.AccessType;
            }

            ICommandResult<Tenant> currentTenantResult = await _mediator
                .Send(new GetTenantCommand(userInvitation.TenantId), cancellationToken);

            switch (currentTenantResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentTenantResult.Status), currentTenantResult.Status, "Unexpected result on creating tenant");
                case CommandStatus.Ok:
                    userTenant = currentTenantResult.Data;
                    break;
            }
        }
        else
        {
            string tenantName = string.IsNullOrEmpty(userEmail) ? "<неизвестный>" : userEmail;

            ICommandResult<Tenant> tenantResult = await _mediator
                .Send(new CreateTenantCommand(tenantName), cancellationToken);

            switch (tenantResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(tenantResult.Status), tenantResult.Status, "Unexpected result on creating tenant");
                case CommandStatus.Ok:
                    userTenant = tenantResult.Data;
                    break;
            }

            accessType = YaMembershipAccessType.Owner;
        }

        if (accessType == YaMembershipAccessType.None)
        {
            throw new InvalidOperationException("Unknown tenant access type");
        }

        ICommandResult<Membership> membershipResult = await _mediator
                .Send(new CreateMembershipCommand(userResult.Data.UserID, userTenant.TenantID, accessType), cancellationToken);

        switch (membershipResult.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(membershipResult.Status), membershipResult.Status, "Unexpected result on creating membership");
            case CommandStatus.Ok:
                userMembership = membershipResult.Data;
                break;
        }

        if (inviteToExistingTenant)
        {
            ICommandResult invitationClaimedResult = await _mediator
                .Send(new SetInvitationClaimedCommand(userInvitation.YaInvitationID, userMembership.MembershipID), cancellationToken);

            switch (membershipResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(membershipResult.Status), membershipResult.Status, "Unexpected result on setting invitation claimed");
                case CommandStatus.Ok:
                    break;
            }
        }

        User user = userResult.Data;
        Guid tenantId = userTenant.TenantID;

        await _authProviderManager
            .SetTenantAsync(authId + "|" + userId, tenantId, accessType, cancellationToken);

        _actionCtx.ActionContext.HttpContext
            .Response.Headers.Add(HeaderNames.LastModified, userResult.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

        UserVm userVm = _mapper.Map<UserVm>(userResult.Data);

        return new CreatedAtRouteResult(RouteNames.GetUser, new { }, userVm);
    }
}
