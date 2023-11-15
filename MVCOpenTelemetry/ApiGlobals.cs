using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MVCOpenTelemetry
{
    public class ApiGlobals
    {
        public static string AppName { set; get; } = "MVCOtel";
        public static DateTime AppStartDate { set; get; } = DateTime.UtcNow;
        public static object m_LockObj = new();
        public static int CallCount { set; get; }
        public static DateTime StartUpDateTime { set; get; } = DateTime.UtcNow;
        public static string InstanceName { set; get; } = "Not Assigned";
        public static DateTime AppDate { get; set; } = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
        public static DateTime UpSince { get; set; } = DateTime.UtcNow;
        public static JsonSerializerSettings _jss = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static ILoggerFactory LoggerFactory { get; set; }

        internal static void Initialize()
        {
            InstanceName = Environment.MachineName + ":" + Environment.CurrentManagedThreadId;
        }

        internal static int IncCallCount()
        {
            ++CallCount;
            return CallCount;
        }
    }
}