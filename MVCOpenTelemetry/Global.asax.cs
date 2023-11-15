using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using MVCOpenTelemetry.App_Start;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVCOpenTelemetry
{
    public class MvcApplication : HttpApplication
    {
        private TracerProvider _tracerProvider;
        private MeterProvider _meterProvider;
        private static string _otlpEndpoint = ConfigurationManager.AppSettings["OtlpEndpoint"];
        protected void Application_Start()
        {
            // Set up OpenTelemetry
            void ConfigureResource(ResourceBuilder r) => r.AddService(
                serviceName: ApiGlobals.AppName,
                serviceVersion: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                serviceInstanceId: Environment.MachineName);
            SetUpTraces(ConfigureResource);
            SetUpMetrics(ConfigureResource);
            SetUpLogging(ConfigureResource);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ApiGlobals.Initialize();
        }

        protected void Application_End()
        {
            this._tracerProvider?.Dispose();
            this._meterProvider?.Dispose();
        }


        private void SetUpLogging(Action<ResourceBuilder> configureResource)
        {
            ApiGlobals.LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder
#if DEBUG
                    .AddSimpleConsole(options =>
                    {
                        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.ffff] ";
                        options.UseUtcTimestamp = true;
                        options.SingleLine = true;
                        options.IncludeScopes = true;
                    })
#endif
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning);
                if (UseOTLP())
                {
                    builder.AddOpenTelemetry(options =>
                    {
                        var resourceBuilder = ResourceBuilder.CreateDefault();
                        configureResource(resourceBuilder);
                        options.IncludeScopes = true;
                        options.SetResourceBuilder(resourceBuilder);
                        options.AddOtlpExporter(otlpOptions => { otlpOptions.Endpoint = new Uri(ConfigurationManager.AppSettings["OtlpEndpoint"]); });
                    });
                }
            });
        }

        private void SetUpTraces(Action<ResourceBuilder> configureResource)
        {
            var builder = Sdk.CreateTracerProviderBuilder()
                .ConfigureResource(configureResource)
                .AddAspNetInstrumentation()
                .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
                .AddHttpClientInstrumentation();

            if (UseOTLP())
            {
                builder.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(_otlpEndpoint);
                });
            }

            if (UseAzureMonitor())
            {
                builder.AddAzureMonitorTraceExporter(o => o.ConnectionString = ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
            }

            if (!UseOTLP() && !UseAzureMonitor())
            {
                builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug);
            }

            this._tracerProvider = builder.Build();
        }

        private void SetUpMetrics(Action<ResourceBuilder> configureResource)
        {
            var builder = Sdk.CreateMeterProviderBuilder()
                .ConfigureResource(configureResource)
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation();

            if (UseOTLP())
            {
                builder.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(_otlpEndpoint);
                });
            }

            if (UseAzureMonitor())
            {
                builder.AddAzureMonitorMetricExporter(o => o.ConnectionString = ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
            }

            if (!UseOTLP() && !UseAzureMonitor())
            {
                builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug);
            }

            this._meterProvider = builder.Build();
        }

        private static bool UseOTLP()
        {
            return !String.IsNullOrEmpty(ConfigurationManager.AppSettings["OtlpEndpoint"]);
        }

        private static bool UseAzureMonitor()
        {
            return !String.IsNullOrEmpty(ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
        }
    }
}
