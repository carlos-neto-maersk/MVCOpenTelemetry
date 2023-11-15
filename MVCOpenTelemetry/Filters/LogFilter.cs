using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;

namespace MVCOpenTelemetry.Filters
{
    public sealed class LogFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        private string _methodName;
        private string _correlationId;
        private Stopwatch _sw;
        private static readonly ILogger _logger = ApiGlobals.LoggerFactory.CreateLogger<LogFilter>();
        private IDisposable _scope;

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            _sw = Stopwatch.StartNew();
            _methodName = actionContext.GetMethodName();
            _correlationId = actionContext.GetCorrelationId();

            var controller = actionContext.GetControllerName();
            var path = actionContext.GetRelativePath();

            _scope = _logger.BeginScope("CorrelationId={CorrelationId}, Controller={Controller}, Path={Path}, MethodName={MethodName}",
                _correlationId, controller, path, _methodName);

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _scope.Dispose();
            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }

}