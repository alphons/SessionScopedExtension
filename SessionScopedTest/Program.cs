
using SessionScopedTest.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	ContentRootPath = AppContext.BaseDirectory
});

var services = builder.Services;

services.AddHttpContextAccessor();

services.AddMvc();

services.AddDistributedMemoryCache();
services.AddSession(options =>
{
	// for testing timeout TimeSpan.FromSeconds(10);
	options.IdleTimeout = TimeSpan.FromMinutes(20);
	options.Cookie.Name = ".AspNetCore.Session";
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

services.AddSessionScoped<ExampleService>();


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();
app.MapDefaultControllerRoute();

app.Run();

