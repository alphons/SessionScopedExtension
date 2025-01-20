namespace SessionScopedTest.Services;

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

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		Logger.LogWarning("StartAsync");

		await Task.Delay(100);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		Logger.LogWarning("StopAsync");

		await Task.Delay(100);
	}
}
