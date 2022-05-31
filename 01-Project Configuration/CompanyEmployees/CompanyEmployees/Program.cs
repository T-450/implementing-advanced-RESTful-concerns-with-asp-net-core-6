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
/*
 * app.UseStaticFiles() enables using static files for the request. If
* we don’t set a path to the static files directory, it will use a wwwroot
* folder in our project by default.
*/
app.UseStaticFiles();

/*
 *
* app.UseForwardedHeaders() will forward proxy headers to the current request.
* This will help during application deployment.Pay attention that we require
*Microsoft.AspNetCore.HttpOverrides using directive to  introduce the ForwardedHeaders enumeration
*/
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
