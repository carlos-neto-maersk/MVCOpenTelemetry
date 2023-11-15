using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MVCOpenTelemetry.Handlers
{
    public class CustomTraceHandler : DelegatingHandler
    {
        private ILogger _logger = ApiGlobals.LoggerFactory.CreateLogger<CustomTraceHandler>();
        private string _headerName = "X-Trace-RId";

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string customTraceHeader = string.Empty;

            if (request.Headers.Contains(_headerName))
                customTraceHeader = request.Headers.GetValues(_headerName).FirstOrDefault();

            using var scope = _logger.BeginScope("X-Trace-RID={XTraceRId}", customTraceHeader);
            var response = await base.SendAsync(request, cancellationToken);

            if (string.IsNullOrEmpty(customTraceHeader))
                customTraceHeader = Activity.Current?.TraceId.ToString();

            if ((int)response.StatusCode >= 300)
            {
                response.Headers.Add(_headerName, customTraceHeader);
            }

            return response;
        }
    }
}