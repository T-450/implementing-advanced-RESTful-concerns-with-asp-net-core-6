## Asp.Net Core Project Configuration

#### 1. launchSettings.json

- `launchSettings.json` file determines the launch behavior of the Asp.Net Core applications;

- It contains both configurations to launch settings for IIS and self-hosted applications (Kestrel);

- Change  `launchBrowser` property to false to prevent web browser from launching on application start. This is
  convenient since we are developing a Web API project and we
  don’t need a browser to check our API out.

```javascript
{  
  "$schema": "https://json.schemastore.org/launchsettings.json",  
  "iisSettings": {  
    "windowsAuthentication": false,  
    "anonymousAuthentication": true,  
    "iisExpress": {  
      "applicationUrl": "http://localhost:1629",  
      "sslPort": 44370  
    }  
  },  "profiles": {  
    "CompanyEmployees": {  
      "commandName": "Project",  
      "dotnetRunMessages": true,  
      "launchBrowser": false,  
      "launchUrl": "weatherforecast",  
      "applicationUrl": "https://localhost:5001;http://localhost:5000",  
      "environmentVariables": {  
        "ASPNETCORE_ENVIRONMENT": "Development"  
      }  
    },    "IIS Express": {  
      "commandName": "IISExpress",  
      "launchBrowser": false,  
      "launchUrl": "weatherforecast",  
      "environmentVariables": {  
        "ASPNETCORE_ENVIRONMENT": "Development"  
      }  
    }  }}
```

- When HTTPS is configured two URLs in the `applicationUrl` section is generated — one for HTTPS (localhost:5001), and
  one for HTTP (localhost:5000).
- `sslPort`  indicates that our application, when running in IISExpress, will be configured for HTTPS (port 44370), too.

```markdown
NOTE: This HTTPS configuration is only valid in the local environment.
You will have to configure a valid certificate and HTTPS redirection once you deploy the application.
```

- `launchUrl` property determines which URL will the application navigate to initially. For `launchUrl` property to
  work, we need to set the launchBrowser property to true.

#### 2 Program.cs

- Program.cs is the entry point of the application:

```cs
using CompanyEmployees.Extensions;  
using Microsoft.AspNetCore.HttpOverrides;  
  
var builder = WebApplication.CreateBuilder(args);  

// Add services to the container

builder.Services.AddControllers();  
  
var app = builder.Build();  

// Configure the HTTP request pipeline

if (app.Environment.IsDevelopment())  
   app.UseDeveloperExceptionPage();  
else  
   app.UseHsts();  
  
app.UseHttpsRedirection();  
app.UseStaticFiles();  
app.UseForwardedHeaders(new ForwardedHeadersOptions  
{  
   ForwardedHeaders = ForwardedHeaders.All  
});  
  
app.UseCors("CorsPolicy");  
  
app.UseAuthorization();  
  
app.MapControllers();  
  
app.Run();
```

- The application create a builder variable of the type `WebApplicationBuilder`. The `WebApplicationBuilder` class is
  responsible for four main things:

	1. Adding Configuration to the project by using the `builder.Configuration` ;
	2. Registering services in the app with the `builder.Services`;
	3. Logging configuration with the `builder.Logging` ;
	4. Other `IhostBuilder` and `IWebHostBuilder` configuration;

```cs
var builder = WebApplication.CreateBuilder(args);  
```

- Larger applications could potentially contain a lot of different
  services in `Program.cs` file, we can structure the code into logical blocks and separate those blocks into extension
  methods.

#### 3. Extension Methods and CORS Configuration

#### Extending a service

- [Extension methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
  enable you to "add" methods to existing types without creating a new derived type, recompiling, or otherwise modifying
  the original type.
- Create a new folder Extensions in the project and create a new class inside that folder named ServiceExtensions. The
  ServiceExtensions class should be static.

```cs
// This class will extend the Service property
public static class ServiceExtensions { }
```

#### Configuring CORS

- CORS (Cross-Origin Resource Sharing) is a mechanism to give or restrict access rights to applications from different
  domains.
- If we want to send requests from a different domain to our application, configuring CORS is mandatory.

```cs
public static class ServiceExtensions  
{  

	// Allows all requests from all origins to be sent to our API
	public static void ConfigureCors(this IServiceCollection services) =>  
	      services.AddCors(options =>  
	      {  
	         options.AddPolicy("CorsPolicy", builder =>  
	         //  Method which allows requests from any source
	         builder.AllowAnyOrigin()
	         // Allows all HTTP methods
	         .AllowAnyMethod()
	         // Allows any Header
	         .AllowAnyHeader());  
	      });
}
```

- Basic CORS policy settings because allowing any origin, method, and header is okay for now;

- Should be more restrictive with those settings in the production environment;

- Instead of the AllowAnyOrigin()  use the WithOrigins("https://example.com") which will
  allow requests only from that concrete source;

- Instead of AllowAnyMethod() that allows all HTTP methods, we can use WithMethods("POST", "GET") that will allow only
  specific HTTP methods;\

- AllowAnyHeader() method by using, for example, the WithHeaders("accept", "content-type") method to allow only specific
  headers.

#### 4.IIS Configuration

- ASP.NET Core applications are by default self-hosted;
- If you want to host our application on IIS, we need to configure an IIS integration, which will eventually help with
  the deployment to IIS.
- Add the following method inside ServiceExtensions.cs:

```cs
// ServiceExtensions.cs
public static void ConfigureIISIntegration(this IServiceCollection services) 
	=> services.Configure<IISOptions>(options => 
	{ 
	  //Can initialize property with options })
	}
```

- [Available configuration options](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-6.0#iis-options)

#### 5.Enabling Configurations

- Let’s go back to our Program class and modify it to support CORS and IIS integration now that we’ve written extension
  methods for those functionalities:

```cs
using CompanyEmployees.Extensions;
using Microsoft.AspNetCore.HttpOverrides;  

var builder = WebApplication.CreateBuilder(args);  


// Our created services
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
  
builder.Services.AddControllers();  
  
var app = builder.Build();  


if (app.Environment.IsDevelopment())  
   app.UseDeveloperExceptionPage();  
else  
   app.UseHsts();  
  
app.UseHttpsRedirection();  
app.UseStaticFiles();  
app.UseForwardedHeaders(new ForwardedHeadersOptions  
{  
   ForwardedHeaders = ForwardedHeaders.All  
});  
  
app.UseCors("CorsPolicy");  
  
app.UseAuthorization();  
  
app.MapControllers();  
  
app.Run();
```

- Let's add a few mandatory methods to the second part of the Program
  class (the one for the request pipeline configuration):

```cs
using CompanyEmployees.Extensions;  
using Microsoft.AspNetCore.HttpOverrides;  
  
var builder = WebApplication.CreateBuilder(args);  
  
builder.Services.ConfigureCors();  
builder.Services.ConfigureIISIntegration();  
  
builder.Services.AddControllers();  
  
var app = builder.Build();  
 
if (app.Environment.IsDevelopment())  
    app.UseDeveloperExceptionPage();  
else  
    app.UseHsts(); // Middleware for using HSTS (Strict-Transport-Security) header.  
  
app.UseHttpsRedirection();  

// app.UseStaticFiles() enables using static files for the request. If* we don’t set a path to the static files directory, it will use a wwwroot  
// folder in our project by default.  
app.UseStaticFiles();  
  
// app.UseForwardedHeaders() will forward proxy headers to the current request.  
// This will help during application deployment.Pay attention that we require  
// Microsoft.AspNetCore.HttpOverrides using directive to  introduce the ForwardedHeaders enumeration  
app.UseForwardedHeaders(new ForwardedHeadersOptions  
{  
    ForwardedHeaders = ForwardedHeaders.All  
});  
  
app.UseCors("CorsPolicy");  
  
app.UseAuthorization();  
  
app.MapControllers();  
  
app.Run();
```

#### 6.Additional Code in the Program.cs

- `AddControllers()` method registers only the controllers in IServiceCollection and not
  Views or Pages because they are not required in the Web API project
  which we are building;
- Right below the controller registration, we have to add this line:

```cs
// With the Build method, we are creating the app variable of the type WebApplication
var app = builder.Build();
```

- This class `WebApplication` is very important since it implements multiple interfaces like IHost that we can use to
  start and stop the host, IApplicationBuilder that we use to build the middleware pipeline (as you could’ve seen from
  our previous custom code), and IEndpointRouteBuilder used to add endpoints in our app;

- The `UseHttpRedirection` method is used to add the middleware for the redirection from HTTP to HTTPS. Also, we can see
  the UseAuthorization method that adds the authorization middleware to the specified IApplicationBuilder to enable
  authorization capabilities;

- `MapControllers` method that adds the endpoints from controller actions to the IEndpointRouteBuilder and the Run
  method that runs the application and block the calling thread until the host shutdown;

- Microsoft advises that the order of adding different middlewares to the
  application builder is very importan;

#### 7. Enviroment-Based Settings

- We need to have a separate configuration for each environment and that’s easy to accomplish by using .NET
  Core-provided mechanisms;

- As soon as we create a project, we are going to see the `appsettings.json` file in the root, which is our main
  settings file, and when we expand it we are going to see the
  `appsetings.Development.json` file by default;

- The` apsettings.{EnvironmentSuffix}.json` files are used to override the main `appsettings.json` file. When we use a
  key-value pair from the original file, we override it.

- We can also define environment-specific values too;

- The `appsettings.Production.json` file should contain the configuration for the production environment;

- To set which environment our application runs on, we need to set up the `ASPNETCORE_ENVIRONMENT` environment variable.

- We can set the variable through the command prompt by typing:
	- `set ASPNETCORE_ENVIRONMENT=Production`  in Windows
	- `export ASPNET_CORE_ENVIRONMENT=Production`  in Linux.

- ASP.NET Core applications use the value of that environment variable to
  decide which appsettings file to use accordingly.

---

#### Resources:

[Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0)

[Options pattern in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0)

[Use multiple environments in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-6.0)

[ASP.NET Core fundamentals overview](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/?view=aspnetcore-6.0&tabs=windows)



---
Source: 
