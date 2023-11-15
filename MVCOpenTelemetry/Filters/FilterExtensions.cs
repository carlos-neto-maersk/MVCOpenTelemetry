using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;

namespace MVCOpenTelemetry.Filters
{
    public static class FilterExtensions
    {
        public static string GetControllerName(this HttpActionContext ctx)
            => ctx.ActionDescriptor.ControllerDescriptor.ControllerType.FullName;

        public static string GetMethodName(this HttpActionContext ctx)
            => ctx.ActionDescriptor.ActionName;

        public static string GetRelativePath(this HttpActionContext ctx)
            => ctx.Request.RequestUri.LocalPath;


        public static string GetCorrelationId(this HttpActionContext ctx)
        {
            if (ctx.ActionArguments.ContainsKey("correlationId"))
            {
                return ctx.ActionArguments["correlationId"].ToString();
            }
            else
            {
                var guid = Guid.NewGuid().ToString();
                ctx.ActionArguments.Add("correlationId", guid);
                return guid;
            }
        }

        public static string GetXTraceHeaderValue(this HttpActionContext ctx)
        {
            foreach (var header in ctx.Request.Headers)
            {
                if (string.Equals(header.Key, "X-Trace-RID", StringComparison.OrdinalIgnoreCase))
                {
                    return header.Value.First();
                }
            }
            return string.Empty;
        }
    }

}