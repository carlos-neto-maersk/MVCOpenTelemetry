# MVC OpenTelemetry MRE

When working on legacy code in a profissional environment, my team faced a bug using on development code 
of the opentelemetry-dotnet-contrib OpenTelemetry.Instrumentation.AspNet package in a .NET Framework ASP.NET MVC application.  
  
The bug was identified by seeing a saw tooth pattern in the metrics of the application, on the response time time series.  
  
It was raised to the community in the OpenTelemetry dotnet contrib issue tracker, [here](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/1431).  
  
This repository aims to be a Minimal, Reproducible Example (MRE) of the bug, so that the community can help us to solve it.  
  
On the `MetricsController` it should be provided a dependency URL so that the application makes use of the `HttpClient` to make a request to the dependency.  
This is to follow a similar pattern to the one we have in our application.  
This application can be a minimum application with a random `Task.Delay` between 30ms and 400ms. 