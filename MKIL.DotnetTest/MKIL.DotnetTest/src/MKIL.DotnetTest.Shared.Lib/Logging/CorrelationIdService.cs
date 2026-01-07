using Microsoft.AspNetCore.Http;
using MKIL.DotnetTest.Shared.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.Shared.Lib.Logging
{
    public interface ICorrelationIdService
    {
        string GetCorrelationId();
        void SetCorrelationId(string correlationId);
    }

    public class CorrelationIdService : ICorrelationIdService
    {
        
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
                   ?? Guid.NewGuid().ToString();
        }

        public void SetCorrelationId(string correlationId)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items[Constants.CORRELATION_HEADER] = correlationId;
            }
        }
    }
}
