using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;

namespace YA.TenantWorker.Application.Models.ValueObjects
{
    /// <summary>
    /// HTTP API error message for UI
    /// </summary>
    public class ApiError : ValueObject
    {
        public ApiError(ApiErrorTypes type, ApiErrorCodes code, string message, string correlationId, object errorData = default)
        {
            if (type == ApiErrorTypes.Unknown)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
            if (code == ApiErrorCodes.DEFAULT_ERROR)
            {
                throw new ArgumentOutOfRangeException(nameof(code));
            }
            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentOutOfRangeException(nameof(correlationId));
            }

            Type = type;
            Code = code;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            CorrelationID = correlationId;
            ErrorData = errorData;
        }

        public ApiErrorTypes Type { get; private set; }
        public ApiErrorCodes Code { get; private set; }
        public string Message { get; private set; }
        public string CorrelationID { get; private set; }
        public object ErrorData { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Type;
            yield return Code;
            yield return Message;
            yield return CorrelationID;
            yield return ErrorData;
        }
    }
}
