using CorrelationId.Abstractions;
using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using YA.Common.Constants;
using YA.TenantWorker.Application.Exceptions;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Infrastructure.Messaging.Filters;

namespace YA.TenantWorker.Infrastructure.Services
{
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

            if (_correlationCtx.CorrelationContext != null && mbMessageContext != null)
            {
                throw new CorrelationIdNotFoundException("Cannot obtain CorrelationID: both contexts are presented.");
            }

            if (_correlationCtx.CorrelationContext == null && mbMessageContext == null)
            {
                return Guid.Empty;
            }

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

            throw new CorrelationIdNotFoundException("Cannot obtain CorrelationID");
        }

        public Guid GetTenantId()
        {
            MbMessageContext mbMessageContext = MbMessageContextProvider.Current;

            if (_httpCtx.HttpContext != null && mbMessageContext != null)
            {
                throw new TenantIdNotFoundException("Cannot obtain TenantID: both contexts are presented.");
            }

            //засев БД при старте приложения
            if (_httpCtx.HttpContext == null && mbMessageContext == null)
            {
                return Guid.Parse(SeedData.SystemTenantId);
            }

            //веб-запрос
            if (_httpCtx.HttpContext != null && mbMessageContext == null)
            {
                Guid httpTenantId = _httpCtx.HttpContext.User.GetClaimValue<Guid>(YaClaimNames.tid);

                if (httpTenantId == Guid.Empty)
                {
                    throw new TenantIdNotFoundException("Cannot obtain TenantID from tid claim.");
                }

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

            throw new TenantIdNotFoundException("Cannot obtain TenantID");
        }

        public string GetTraceId()
        {
            string traceId = Activity.Current?.Id ?? _httpCtx.HttpContext.TraceIdentifier;
            return traceId;
        }
    }
}
