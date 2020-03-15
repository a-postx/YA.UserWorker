using CorrelationId;
using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Runtime;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Infrastructure.Messaging.Filters;

namespace YA.TenantWorker.Infrastructure.Services
{
    /// <summary>
    /// Отслеживает состояние контекста исполнения
    /// </summary>
    public class RuntimeContextAccessor : IRuntimeContextAccessor
    {
        public RuntimeContextAccessor(ILogger<RuntimeContextAccessor> logger,
            IHttpContextAccessor httpContextAccessor,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private readonly ILogger<RuntimeContextAccessor> _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        /// <summary>
        /// Доступ к текущему идентификатору корелляции.
        /// </summary>
        public Guid GetCorrelationId()
        {
            MbMessageContext mbMessageContext = MbMessageContextProvider.Current;

            if (_correlationContextAccessor.CorrelationContext != null && mbMessageContext != null)
            {
                throw new Exception("Cannot obtain TenantID: both contexts are presented.");
            }

            if (_correlationContextAccessor.CorrelationContext == null && mbMessageContext == null)
            {
                return Guid.Empty;
            }

            //веб-запрос
            if (_correlationContextAccessor.CorrelationContext != null && mbMessageContext == null)
            {
                if (Guid.TryParse(_correlationContextAccessor.CorrelationContext.CorrelationId, out Guid correlationId))
                {
                    return correlationId;
                }
                else
                {
                    return Guid.Empty;
                }
            }

            //запрос из шины
            if (_correlationContextAccessor.CorrelationContext == null && mbMessageContext != null)
            {
                if (mbMessageContext.CorrelationId == Guid.Empty)
                {
                    throw new Exception("Cannot obtain CorrelationID from message bus message.");
                }

                return mbMessageContext.CorrelationId;
            }

            throw new Exception("Cannot obtain CorrelationID.");
        }

        /// <summary>
        /// Доступ к текущему идентификатору арендатора. Никогда не выводит пустой идентификатор.
        /// </summary>
        public Guid GetTenantId()
        {
            MbMessageContext mbMessageContext = MbMessageContextProvider.Current;

            if (_httpContextAccessor.HttpContext != null && mbMessageContext != null)
            {
                throw new Exception("Cannot obtain TenantID: both contexts are presented.");
            }

            //засев БД при старте приложения
            if (_httpContextAccessor.HttpContext == null && mbMessageContext == null)
            {
                return Guid.Parse(SeedData.SystemTenantId);
            }

            //веб-запрос
            if (_httpContextAccessor.HttpContext != null && mbMessageContext == null)
            {
                Guid httpTenantId = _httpContextAccessor.HttpContext.User.GetClaimValue<Guid>(CustomClaimNames.tid);

                if (httpTenantId == Guid.Empty)
                {
                    if (_httpContextAccessor.HttpContext.Request.Method == "POST" 
                        && _httpContextAccessor.HttpContext.Request.Path.HasValue
                        && _httpContextAccessor.HttpContext.Request.Path.Value == "/authentication")
                    {
                        return Guid.Parse(SeedData.SystemTenantId);
                    }
                    else
                    {
                        throw new Exception("Cannot obtain TenantID from tid claim.");
                    }
                }

                return httpTenantId;
            }

            //запрос из шины
            if (_httpContextAccessor.HttpContext == null && mbMessageContext != null)
            {
                if (mbMessageContext.TenantId == Guid.Empty)
                {
                    throw new Exception("Cannot obtain TenantID from message bus message.");
                }

                return mbMessageContext.TenantId;
            }

            throw new Exception("Cannot obtain TenantID.");
        }
    }
}
