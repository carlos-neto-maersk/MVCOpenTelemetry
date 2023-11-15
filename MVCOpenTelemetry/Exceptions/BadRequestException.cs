using System;

namespace MVCOpenTelemetry.Exceptions
{
    public class BadRequestException : Exception
    {
        public string Reason { get; private set; }
        public string Content { get; private set; }

        public BadRequestException(string reason, string content) : base($"{reason} {content}")
        {
            Reason = reason;
            Content = content;
        }
    }
}