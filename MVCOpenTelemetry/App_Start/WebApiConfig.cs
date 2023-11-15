using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using MVCOpenTelemetry.Handlers;

namespace MVCOpenTelemetry.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new CustomTraceHandler());
            config.SuppressHostPrincipal();

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            config.Formatters.Add(new XmlMediaTypeFormatter());

            config.Formatters.XmlFormatter.UseXmlSerializer = true;
        }
    }
}