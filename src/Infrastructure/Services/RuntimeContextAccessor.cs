using System.Diagnostics;
using CorrelationId.Abstractions;
using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Http;
using YA.Common.Constants;
using YA.UserWorker.Application.Exceptions;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Constants;
using YA.UserWorker.Infrastructure.Messaging.Filters;

namespace YA.UserWorker.Infrastructure.Services;

public class RuntimeContextAccessor : IRuntimeContextAccessor
{
    public RuntimeContextAccessor(ILogger<RuntimeContextAccessor> logger,
        IHttpContextAccessor httpCtx,
        ICorrelationContextAccessor correlationCtx)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationCtx = correlationCtx ?? throw new ArgumentNullException(nameof(correlationCtx));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
    }

    private readonly ILogger<RuntimeContextAccessor> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly ICorrelationContextAccessor _correlationCtx;

    public Guid GetCorrelationId()
    {
        MbMessageContext mbMessageContext = MbMessageContextProvider.Current;

        //веб-запрос
        if (_correlationCtx.CorrelationContext != null && mbMessageContext == null)
        {
            if (Guid.TryParse(_correlationCtx.CorrelationContext.CorrelationId, out Guid correlationId))
            {
                return correlationId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        //запрос из шины
        if (_correlationCtx.CorrelationContext == null && mbMessageContext != null)
        {
            if (mbMessageContext.CorrelationId == Guid.Empty)
            {
                throw new CorrelationIdNotFoundException("Cannot obtain CorrelationID from message bus message.");
            }

            return mbMessageContext.CorrelationId;
        }

        if (_correlationCtx.CorrelationContext == null && mbMessageContext == null)
        {
            return Guid.Empty;
        }

        if (_correlationCtx.CorrelationContext != null && mbMessageContext != null)
        {
            throw new CorrelationIdNotFoundException("Cannot obtain CorrelationID: bad context state.");
        }

        throw new CorrelationIdNotFoundException("Cannot obtain CorrelationID");
    }

    public (string authId, string userId) GetUserIdentifiers()
    {
        string uid = _httpCtx.HttpContext.User.Claims
            .Where(e => e.Type == YaClaimNames.uid).First().Value;

        string[] userProps = uid.Split('|');

        string userProvider;
        string userExternalId;

        if (userProps.Length > 1)
        {
            userProvider = userProps[0];
            userExternalId = userProps[1];
        }
        else
        {
            userProvider = "keycloak";
            userExternalId = userProps[0];
        }

        return (userProvider, userExternalId);
    }

    public string GetUserId()
    {
        //миграция БД исполняется не от запроса, поэтому контекста нет.
        //надо сделать обязательным, если делать миграцию через скрипты
        string result = _httpCtx.HttpContext?.User?.GetClaimValue<string>(YaClaimNames.uid);

        return result;
    }

    public Guid GetTenantId()
    {
        MbMessageContext mbMessageContext = MbMessageContextProvider.Current;

        //веб-запрос
        if (_httpCtx.HttpContext != null && mbMessageContext == null)
        {
            Guid httpTenantId = _httpCtx.HttpContext.User.GetClaimValue<Guid>(YaClaimNames.tid);

            //идентификатор может быть пустым для первый раз вошедшего пользователя
            return httpTenantId;
        }

        //запрос из шины
        if (_httpCtx.HttpContext == null && mbMessageContext != null)
        {
            if (mbMessageContext.TenantId == Guid.Empty)
            {
                throw new TenantIdNotFoundException("Cannot obtain TenantID from message bus message.");
            }

            return mbMessageContext.TenantId;
        }

        //засев БД при старте приложения
        if (_httpCtx.HttpContext == null && mbMessageContext == null)
        {
            return Guid.Parse(SeedData.SystemTenantId);
        }

        if (_httpCtx.HttpContext != null && mbMessageContext != null)
        {
            throw new TenantIdNotFoundException("Cannot obtain TenantID: bad context state.");
        }

        throw new TenantIdNotFoundException("Cannot obtain TenantID");
    }

    public string GetTraceId()
    {
        string traceId = Activity.Current?.Id ?? _httpCtx.HttpContext.TraceIdentifier;
        return traceId;
    }
}
