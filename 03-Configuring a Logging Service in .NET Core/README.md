#### Configuring Logging Service

- .NET Core has its implementation of the logging mechanism, but in all our projects we prefer to create our custom logger service with the external logger library NLog.

- We are going to do that because having an abstraction will allow us to have any logger behind our interface. This means that we can start with NLog, and at some point, we can switch to any other logger and our interface will still work because of our abstraction.

#### Creating Required Projects

- Let’s create two new  **Class Library**  projects:
	1. **Contracts**: responsible to keep our interfaces;
	2. **LoggerService**:  Where we are going to use to write our logger logic in;
- Now that we have these projects in place,  reference them from our main project. LoggerService should reference Contracts. Main project should reference LoggerService;

#### ILoggerManager Interface and NLog
- The logger service will contain four methods for logging our messages:
	- Info;
	- Debug;
	- Warning;
	- Error;
- Create an interface named ILoggerManager inside the Contracts project containing those four method definitions:

```cs
public interface ILoggerManager
{
	void LogInfo(string message);
	void LogWarn(string message);
	void LogDebug(string message);
	void LogError(string message);
}
```

-  Install the NLog library in our LoggerService project. NLog is a logging platform for .NET which will help us create and log our messages.

```bash
Install-Package NLog.Extensions.Logging -Version 1.7.4
```

#### Implementing the Interface and Nlog.Config File
- In the LoggerService project, we are going to create a new class: LoggerManager;

```cs
//implement the ILoggerManager interface we previously defined
public class LoggerManager : ILoggerManager
{
	private static ILogger logger = LogManager.GetCurrentClassLogger();
	public LoggerManager(){}
	public void LogDebug(string message) => logger.Debug(message);
	public void LogError(string message) => logger.Error(message);
	public void LogInfo(string message) => logger.Info(message);
	public void LogWarn(string message) => logger.Warn(message);
}
```

- Our methods are just wrappers around NLog’s methods. Both ILogger and LogManager are part of the NLog namespace.
-  Configure it and inject it into the Program.cs  in the section related to the service configuration.
- NLog needs to have information about where to put log files on the file system, what the name of these files will be, and what is the minimum level of logging.
- Define all these constants in a text file in the main project and name it nlog.config.

```xml
<?xml version="1.0" encoding="utf-8" ?>  
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"  
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"  
      autoReload="true"  
      internalLogLevel="Trace"  
      internalLogFile=".\internal_logs\internallog.txt">  
  
   <targets>      <target name="logfile" xsi:type="File"  
            fileName=".\logs\${shortdate}_logfile.txt"  
            layout="${longdate} ${level:uppercase=true} ${message}"/>  
   </targets>  
   <rules>      <logger name="*" minlevel="Debug" writeTo="logfile" />  
   </rules>
</nlog>
```

- You can find the internal logs at the project root, and the logs folder in the `bin\debug` folder of the main project once we start the app.

````markdown
NOTE: : If you want to have more control over the log output, we suggest
renaming the current file to nlog.development.config and creating another
configuration file called nlog.production.config. Then you can do something like
this in the code: env.ConfigureNLog($"nlog.{env.EnvironmentName}.config");
to get the different configuration files for different environments. From our
experience production path is what matters, so this might be a bit redundant.
````

#### Configuring Logger Service for Logging Messages
- Update the Program.cs and include the path to the configuration file for the NLog configuration:
```cs
using NLog;

var builder = WebApplication.CreateBuilder(args);
// LoadConfiguration  provides a path to the configuration file
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),"/nlog.config"));

builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration()
```

- Register the logger service inside the .NET Core’s IOC container by  adding a new method in the ServiceExtensions class:
```cs
public static void ConfigureLoggerService(this IServiceCollection services) 
	=> services.AddSingleton<ILoggerManager, LoggerManager>();
```

-  Modify the Program class to include our newly created extension method:

```cs
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
// Logger is registered
builder.Services.ConfigureLoggerService();

builder.Services.AddControllers();
```

Every time we want to use a logger service, all we need to do is to inject it into the constructor of the class that needs it. .NET Core will resolve that service and the logging features will be available. This type of injecting a class is called Dependency Injection and it is built into .NET Core.

#### Testing the LoggerService
- To test our logger service, we are going to use the default WeatherForecastController. You can find it in the main project in the Controllers folder. It comes with the ASP.NET Core Web API template.
- In the Solution Explorer, we are going to open the Controllers folder and locate the WeatherForecastController class and add the following:

```cs
[Route("[controller]")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
	private ILoggerManager _logger;
	public WeatherForecastController(ILoggerManager logger)
	{
	_logger = logger;
	}

	[HttpGet]
	public IEnumerable<string> Get()
	{
		_logger.LogInfo("Here is info message from our values controller.");
		_logger.LogDebug("Here is debug message from our values controller.");
		_logger.LogWarn("Here is warn message from our values controller.");
		_logger.LogError("Here is an error message from our values controller.");
		return new string[] { "value1", "value2" };
	}
}
```

- Start the application and browse to https://localhost:5001/weatherforecast;
- As a result, you will see an array of two strings. Now go to the folder that you have specified in the nlog.config file, and check out the result.


---
Links:

[NLogger](https://nlog-project.org/)

[Logging in .NET Core and ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0)

[Logging in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)

[Implement a custom logging provider in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider)

[HTTP Logging in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-6.0)
