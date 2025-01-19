namespace SessionScopedTest.Services;

public class ExampleService(ILoggerFactory loggerFactory) : IHostedService
{
	private readonly ILogger Logger = loggerFactory.CreateLogger<ExampleService>();

	public async Task<int> CalcAsync(int a, int b)
	{
		Logger.LogInformation("CalcAsync {a} x {b}", a , b);

		await Task.Delay(100);

		return a * b;
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
