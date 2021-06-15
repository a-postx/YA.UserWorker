using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using YA.Common.Constants;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features.Users.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users
{
    public class PostUserAh : IPostUserAh
    {
        public PostUserAh(ILogger<PostUserAh> logger,
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

        private readonly ILogger<PostUserAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IAuthProviderManager _authProviderManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        public async Task<IActionResult> ExecuteAsync(AccessInfoSm accessInfo, CancellationToken cancellationToken)
        {
            (string authId, string userId) = _runtimeCtx.GetUserIdentifiers();

            JwtSecurityTokenHandler jwtHandler = new();

            SecurityToken securityToken;

            try
            {
                securityToken = jwtHandler.ReadToken(accessInfo.AccessToken);
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
                    new AuthenticationHeaderValue("Bearer", accessInfo.AccessToken);
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

            ClaimsPrincipal user = _actionCtx.ActionContext.HttpContext.User;
            string userEmail = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email)?.Value;

            ICommandResult<User> result = await _mediator
                .Send(new CreateUserCommand(authId, userId, userInfo.Name, userInfo.Given_Name,
                    userInfo.Family_Name, userEmail, userInfo.Picture, userInfo.NickName), cancellationToken);

            switch (result.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatus.UnprocessableEntity:
                    return new UnprocessableEntityResult();
                case CommandStatus.Ok:
                    Guid tenantId = result.Data.Tenants.First().TenantID;

                    await _authProviderManager
                        .SetTenantAsync(authId + "|" + userId, tenantId, YaMembershipAccessType.Owner, cancellationToken);

                    _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                    UserVm userVm = _mapper.Map<UserVm>(result.Data);

                    return new CreatedAtRouteResult(RouteNames.GetUser, new { }, userVm);
            }
        }
    }
}
