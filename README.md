# builder.Services.AddSessionScoped&lt;T&gt;();

A lightweight extension for managing session-scoped data in ASP.NET Core applications.

## Overview

SessionScoped simplifies session management by providing scoped data storage tied to the user's session. It integrates seamlessly with ASP.NET Core's dependency injection, making session-based state management easy and robust.

## Features

- Scoped data tied to the user's session.
- Fully compatible with ASP.NET Core dependency injection.
- Minimal setup required.

## Getting Started

1. **Create a Service implementing IHostedService interface**

```csharp
public class ExampleService(ILoggerFactory loggerFactory) : IHostedService
{
	private readonly ILogger Logger = loggerFactory.CreateLogger<ExampleService>();

	private string name = "unknown";

	public async Task<bool> SetNameAsync(string Name)
	{
		this.name = Name;

		Logger.LogInformation("SetNameAsync {Name}", Name);

		await Task.Delay(100);

		return true;
	}

	public async Task<string> GetNameAsync()
	{
		Logger.LogInformation("GetNameAsync {name}", name);

		await Task.Delay(100);

		return name;
	}

	// IHostedService StartAsync
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		Logger.LogWarning("StartAsync");

		await Task.Delay(100);
	}

	// IHostedService StopAsync
	public async Task StopAsync(CancellationToken cancellationToken)
	{
		Logger.LogWarning("StopAsync");

		await Task.Delay(100);
	}
}
```

2. **Configure AddSessionScoped**

Register the session and `SessionScoped` in your `Startup.cs` or `Program.cs` file:

```csharp
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
```

3. **Use SessionScopedFactory<T>**

Inject `SessionScopedFactory<ExampleService> factory` into your services or controllers to get the instance of your session-scoped service:

```csharp
public class ExampleController(SessionScopedFactory<ExampleService> factory) : ControllerBase
{
	private readonly ExampleService exampleService = factory.Instance;

	public async Task<IActionResult> Index()
	{
		HttpContext.Session.SetString("Started", DateTime.Now.ToString());

		var result = await exampleService.SetNameAsync($"alphons {Guid.NewGuid()}");

		return Ok("name is set, check url /Example/WhatsYourName");
	}
	public async Task<IActionResult> WhatsYourName()
	{
		var name = await exampleService.GetNameAsync();

		return Ok(name);
	}
}
```

## Requirements

- .NET 8 or higher
- ASP.NET Core

## Contributing

Contributions are welcome! Feel free to fork the repository and submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/alphons/SessionScopedExtension/blob/master/LICENSE) file for details.

---

For more details, visit the [GitHub repository](https://github.com/alphons/SessionScopedExtension/tree/master/SessionScoped).
