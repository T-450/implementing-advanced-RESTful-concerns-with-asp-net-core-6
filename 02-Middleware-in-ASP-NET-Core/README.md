### ASP.NET Core Middleware
- ASP.NET Core middleware is a piece of code integrated inside the application’s pipeline that we can use to handle requests and responses.
- ASP.NET Core middleware, we can think of it as a code section that executes with every request.
- Usually, we have more than a single middleware component in our application, each component can:
	- Pass the request to the next middleware component in the pipeline;
	- It can execute some work before and after the next component in the pipeline;
- To build a pipeline, we are using request delegates, which handle each HTTP request. To configure request delegates, we use the `Run`, `Map` , and `Use` extension methods.
- An application executes each component in the same order they are placed in the code

![](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/request-delegate-pipeline.png?view=aspnetcore-6.0)

- Each component can execute custom logic before using the next delegate to pass the execution to another component;
- The last middleware component doesn’t call the next delegate, which means that this component is `short-circuiting`  the pipeline. It executes the additional logic and then returns the execution to the previous middleware components.
- The order in which we should register our middleware component is important for the security, performance, and functionality of our applications:

![](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/middleware-pipeline.svg?view=aspnetcore-6.0)

### Creating a First Middleware Component

- Create a new ASP.NET Core Web API project, and name it MiddlewareExample;
- In the `launchSettings.json` file, we are going to add some changes regarding the launch profiles:
```json
{  
  "profiles": {  
    "MiddlewareExample": {  
      "commandName": "Project",  
      "dotnetRunMessages": true,  
      "launchBrowser": true,  
      "launchUrl": "weatherforecast",  
      "applicationUrl": "https://localhost:5001;http://localhost:5000",  
      "environmentVariables": {  
        "ASPNETCORE_ENVIRONMENT": "Development"  
      }  
    }  
  }  
}
```

- Now, inside the Program class, right below the UseAuthorization part, we are going to use an anonymous method to create a first middleware component:

```cs
app.UseAuthorization();
// We use the Run method, which adds a terminal component to the app pipeline.
// We are not using the next delegate because the Run method is always terminal and terminates the pipeline.
app.Run(async context => 
	{ 
		await context.Response.WriteAsync("Hello from the middleware component."); 
	});
	
app.MapControllers();
```

- The  `Run`  method accepts a single parameter of the RequestDelegate type;
- The RequestDelegate type accepts a single HttpContext parameter:

```cs
//We are using that context parameter to modify our requests and responses inside the middleware component.
namespace Microsoft.AspNetCore.Http { 

	public delegate Task RequestDelegate(HttpContext context);

}
```

- For this middleware we are modifying the response by using the WriteAsync method;
- Start the app, and inspect the result from our middleware.


### Working with the Use Method

- To chain multiple request delegates, use the `Use` method.
- The `Use` method accepts a Func delegate as a parameter and returns a Task as a result:

```cs
public static IApplicationBuilder Use(this IApplicationBuilder app, Func<HttpContext, Func<Task>, Task> middleware);
```

- This means when we use it, we can make use of two parameters, context and next:
```cs
/*
We add several logging messages to be sure what the order of executions inside middleware components
*/
app.Use(async (context, next) =>  
{  
    Console.WriteLine("Logic before executing the next delegate in the Use method");  
    await next.Invoke();  
    Console.WriteLine("Logic after executing the next delegate in the Use method");  
});
/*
The Run method doesn’t accept the next delegate as a parameter, so without it to send the execution to another component, this component short-circuits the request pipeline.
*/
app.Run(async context => { 
	Console.WriteLine($"Writing the response to the client in the Run method"); 
	await context.Response.WriteAsync("Hello from the middleware component."); 
});
```

- Start the app and inspect the result, which proves our execution order:
```markdown
Logic before executing the next delegate in the Use method
Writing the response to the client in the Run method
Logic after executing the next delegate in the Use method
```

#### IMPORTANT
- We shouldn’t call the `next.Invoke` after we send the response to the client. This can cause exceptions if we try to set the status code or modify the headers of the response. For Example:

```cs
app.Use(async (context, next) =>  
{  
    await context.Response.WriteAsync("Hello from the middleware component."); 
	// Here we write a response to the client and then call next.Invoke.
	// This passes the execution to the next component in the pipeline
    await next.Invoke();  
    Console.WriteLine("Logic after executing the next delegate in the Use method");  
});
app.Run(async context => { 
	Console.WriteLine($"Writing the response to the client in the Run method");
	// we try to set the status code of the response and write another one.
	context.Response.StatusCode = 200;
	await context.Response.WriteAsync("Hello from the middleware component."); 
});
```

- Start the app and see the error message:
```markdown  
System.InvalidOperationException: StatusCode cannot be set because the response has already started;
```

#### Using the Map and MapWhen Methods
- To branch the middleware pipeline, we can use both Map and MapWhen methods.
- The Map method is an extension method that accepts a path string as one of the parameters:
```cs
public static IApplicationBuilder Map(this IApplicationBuilder app, PathString pathMatch, Action<IApplicationBuilder> configuration)
```

- When we provide the pathMatch string, the Map method will compare it to the start of the request path. If they match, the app will execute the branch.
- Use this method by modifying the Program class:

```cs
app.Use(async (context, next) =>  
{  
    Console.WriteLine("Logic before executing the next delegate in the Use method");  
    await next.Invoke();  
    Console.WriteLine("Logic after executing the next delegate in the Use method");  
});

/*
* By using the Map method, we provide the path match, and then in the delegate, we use our * well-known Use and Run methods to execute middleware components.
*/
app.Map("/usingmapbranch", builder =>  
{  
    builder.Use(async (context, next) =>  
    {  
        Console.WriteLine("Map branch logic in the Use method before the next delegate");  
        await next.Invoke();  
        Console.WriteLine("Map branch logic in the Use method after the next delegate");  
    });
      
    builder.Run(async context =>  
    {  
        Console.WriteLine("Map branch response to the client in the Run method");  
        await context.Response.WriteAsync("Hello from the map branch.");  
    });  
});  
 
app.Run(async context =>  
{  
    Console.WriteLine("Writing the response to the client in the Run method");  
    await context.Response.WriteAsync("Hello from the middleware component.");  
});
```

- Now, if we start the app and navigate to ``/usingmapbranch`, we are going to see the response in the browser.
```markdown
Hello from the middleware component.
```

- But also, if we inspect logs, we are going to see our new messages:
```markdown
Logic before executing the next delegate in the Use method
Map branch logic in the Use method before the next delegate
Map branch response to the client in the Run method
Map branch logic in the Use method after the next delegate
Logic after executing the next delegate in the Use method
```

**It is important to know that any middleware component that we add after the Map method in the pipeline won’t be executed.This is true even if we don’t use the Run middleware inside the branch.**

#### Using MapWhen Method
- If we inspect the MapWhen method, we are going to see that it accepts two parameters:
```cs
// This method uses the result of the given predicate to branch the request pipeline.
public static IApplicationBuilder MapWhen(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> configuration)
```

- So, let’s see it in action:
```cs
app.Map("/usingmapbranch", builder => { ... });
/*
Here, if our request contains the provided query string, we execute the Run method by writing the response to the client. So, as we said, based on the predicate’s result the MapWhen method branch the request pipeline.
*/
app.MapWhen(context => context.Request.Query.ContainsKey("testquerystring"),  
    builder =>  
    {  
        builder.Run(async context => { await context.Response.WriteAsync("Hello from the MapWhen branch."); });  
    });  
app.Run(async context => { ... });
```

- Now, we can start the app and navigate to https://localhost:5001?testquerystring=test  amd see our expected message. Of course, we can chain multiple middleware components inside this method as well.

---
Links:

[ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0)
[Write custom ASP.NET Core middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-6.0)
[Test ASP.NET Core middleware](https://docs.microsoft.com/en-us/aspnet/core/test/middleware?view=aspnetcore-6.0)
[DEEP DIVE: HOW IS THE ASP.NET CORE MIDDLEWARE PIPELINE BUILT?](stevejgordon.co.uk/how-is-the-asp-net-core-middleware-pipeline-built)


---
