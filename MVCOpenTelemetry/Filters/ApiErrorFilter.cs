using Microsoft.Extensions.Logging;
using MVCOpenTelemetry.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MVCOpenTelemetry.Filters
{
    public sealed class ApiErrorFilter : ExceptionFilterAttribute
    {
        private string _methodName;
        private static readonly ILogger _logger = ApiGlobals.LoggerFactory.CreateLogger<ApiErrorFilter>();

        public override Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _methodName = actionExecutedContext.ActionContext.GetMethodName();

            HandleException(actionExecutedContext, GetErrorMessage(actionExecutedContext.Exception));
            LogException(actionExecutedContext);

            return Task.CompletedTask;
        }

        private void LogException(HttpActionExecutedContext actionExecutedContext)
        {
            var controller = actionExecutedContext.ActionContext.GetControllerName();
            var path = actionExecutedContext.ActionContext.GetRelativePath();
            var correlationId = actionExecutedContext.ActionContext.GetCorrelationId();

            using var scope = _logger.BeginScope("CorrelationId={CorrelationId}, Controller={Controller}, Path={Path}, MethodName={MethodName}",
                correlationId, controller, path, _methodName);

            _logger.LogInformation(actionExecutedContext.Exception, "Caught exception, Reason={Reason}", actionExecutedContext.Exception.Message);
        }

        private void HandleException(HttpActionExecutedContext actionExecutedContext, string reasonPhrase)
        {
            var exception = actionExecutedContext.Exception;
            var exType = exception.GetType();

            if (exType == typeof(ApplicationException))
            {
                HandleApplicationException(actionExecutedContext, exception, reasonPhrase);
                return;
            }

            if (exType.Name == "BadRequestException")
            {
                HandleBadRequestException(actionExecutedContext, exception as BadRequestException);
                return;
            }

            if (exception.Message.ToLower() == "not found")
            {
                HandleNotFoundException(actionExecutedContext, exception, reasonPhrase);
                return;
            }

            HandleGenericException(actionExecutedContext, exception, reasonPhrase);
        }

        private void HandleGenericException(HttpActionExecutedContext actionExecutedContext, Exception ex, string reasonPhrase)
        {
            actionExecutedContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(ex.Message),
                ReasonPhrase = reasonPhrase
            };
        }

        private void HandleNotFoundException(HttpActionExecutedContext actionExecutedContext, Exception ex, string reasonPhrase)
        {
            actionExecutedContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(ex.Message),
                ReasonPhrase = reasonPhrase
            };
        }

        private void HandleApplicationException(HttpActionExecutedContext actionExecutedContext, Exception ex, string reasonPhrase)
        {
            actionExecutedContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(ex.Message),
                ReasonPhrase = reasonPhrase
            };
        }

        private void HandleBadRequestException(HttpActionExecutedContext actionExecutedContext, BadRequestException exception)
        {

            actionExecutedContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(exception.Content),
                ReasonPhrase = exception.Reason
            };
        }

        private string GetErrorMessage(Exception exception)
        {
            return Regex.Replace(exception.GetBaseException().Message, @"\t|\n|\r", "");
        }
    }
}