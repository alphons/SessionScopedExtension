using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Wrapper for an IHostedService, designed for session-based execution.
/// This wrapper ensures that a hosted service can be started and stopped
/// based on the lifecycle of a session, providing greater control over
/// session-specific tasks. It integrates seamlessly with session management
/// by allowing services to be dynamically tied to a session's lifetime.
/// </summary>
public class SessionScopedWrapper(IHostedService hostedService)
{
	/// <summary>
	/// The hosted service that is being wrapped.
	/// </summary>
	public readonly IHostedService hostedService = hostedService;

	/// <summary>
	/// A CancellationTokenSource used to cancel the task.
	/// </summary>
	private CancellationTokenSource? cts;

	/// <summary>
	/// The task that is running for the hosted service.
	/// </summary>
	private Task? runningTask;

	/// <summary>
	/// Starts the hosted service.
	/// </summary>
	public void Start()
	{
		cts = new CancellationTokenSource();
		runningTask = hostedService.StartAsync(cts.Token);
	}

	/// <summary>
	/// Stops the hosted service and cancels the task.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task StopAsync()
	{
		if (cts != null)
		{
			cts.Cancel();
			try
			{
				await hostedService.StopAsync(cts.Token);

				if (runningTask != null)
					await runningTask;
			}
			catch (AggregateException)
			{
				// Suppress exceptions caused by task cancellation
			}

			cts.Dispose();
		}
	}
}
