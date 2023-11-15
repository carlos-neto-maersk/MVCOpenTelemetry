using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MVCOpenTelemetry.Filters;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace MVCOpenTelemetry.Controllers
{
    [ApiErrorFilter, LogFilter]
    public class MetricsController : ApiController
    {
        private static ILogger _logger = ApiGlobals.LoggerFactory.CreateLogger<MetricsController>();
        private static string _dependencyUrl = ConfigurationManager.AppSettings["DependencyUrl"]; 
        //TODO Change this to a server with a variable response time around 30ms and 400ms

        [HttpGet, Route("api/Metrics")]
        public async Task<IHttpActionResult> Get(CancellationToken ct)
        {
            _logger.LogInformation("Entered endpoint");
            string dependencyUrl = "https://localhost:44300/api/Dependency";  // TODO Change this
            object body = new
            {
                ParameterA = "A",
                ParameterB = "I'm a parameter"
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            {
                System.Diagnostics.Debug.WriteLine(json);

                var response = await client.PostAsync(dependencyUrl, new StringContent(json, Encoding.UTF8, "application/json"), ct).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<MetricsResponse>(content);
                if (!response.IsSuccessStatusCode) throw new Exception($"{response.ReasonPhrase} {content}");
            }

            _logger.LogInformation("Leaving endpoint");
            return Ok();
        }
    }

    public class MetricsResponse
    {
        public string ParameterC { get; set; }
    }
}