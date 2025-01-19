using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A background service that manages session-scoped hosted services.
/// </summary>
/// <typeparam name="T">The type of the hosted service.</typeparam>
public class SessionScopedFactory<T>(
	IServiceProvider serviceProvider,
	IHttpContextAccessor httpContextAccessor,
	ILoggerFactory loggerFactory) : BackgroundService where T : IHostedService
{
	/// <summary>
	/// Logger for the session-scoped factory.
	/// </summary>
	private readonly ILogger Logger = loggerFactory.CreateLogger<SessionScopedFactory<T>>();

	/// <summary>
	/// A dictionary of session-scoped services, keyed by session ID.
	/// </summary>
	private readonly ConcurrentDictionary<string, SessionScopedWrapper> services = new();

	/// <summary>
	/// A dictionary to track session timeouts.
	/// </summary>
	private readonly ConcurrentDictionary<string, DateTimeOffset> timeout = new();

	/// <summary>
	/// Creates a wrapped instance of the scoped service.
	/// </summary>
	/// <returns>A wrapped session-scoped service.</returns>
	private SessionScopedWrapper GetScopedServiceWrapped()
	{
		using var scope = serviceProvider.CreateScope();
		return new(scope.ServiceProvider.GetRequiredService<T>());
	}

	/// <summary>
	/// Gets or creates the instance of the hosted service for the current session.
	/// </summary>
	public T Instance
	{
		get
		{
			var sessionId = (httpContextAccessor.HttpContext?.Session?.Id) ??
				throw new InvalidOperationException("Session ID is not available.");

			timeout.AddOrUpdate(sessionId, DateTimeOffset.UtcNow,
				(key, oldValue) => DateTimeOffset.UtcNow);

			if (!services.TryGetValue(sessionId, out SessionScopedWrapper? wrapper))
			{
				wrapper = GetScopedServiceWrapped();
				if (wrapper == null)
					throw new InvalidOperationException("Cannot get scoped service.");
				if (services.TryAdd(sessionId, wrapper))
				{
					wrapper.Start();
					Logger.LogInformation("Added started service with ID: {sessionId}", sessionId);
				}
			}
			return (T)wrapper.hostedService;
		}
	}

	/// <summary>
	/// Removes a service associated with the given session ID.
	/// </summary>
	/// <param name="sessionId">The session ID to remove.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task RemoveServiceAsync(string sessionId)
	{
		// Remove service if it exists
		if (services.TryRemove(sessionId, out SessionScopedWrapper? service))
		{
			await service.StopAsync();
			Logger.LogInformation("Removed stopped service with ID: {sessionId}", sessionId);
			service = null;
		}

		// Remove the timeout entry
		timeout.TryRemove(sessionId, out _);
	}

	/// <summary>
	/// Stops all services and cleans up.
	/// </summary>
	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var id in services.Keys.ToArray())
		{
			await RemoveServiceAsync(id);
		}
	}

	/// <summary>
	/// Executes the background service and cleans up expired sessions.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation($"{nameof(SessionScopedFactory<T>)} starting.");

		var sessionOptions = serviceProvider.GetRequiredService<IOptions<SessionOptions>>();

		var sessionTimeout = sessionOptions.Value.IdleTimeout;

		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				// Configurable interval duration
				await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

				// Determine the threshold for expired items
				var expirationThreshold = DateTimeOffset.UtcNow.Subtract(sessionTimeout);

				var sessionIds = timeout.Where(x => x.Value < expirationThreshold).Select(x => x.Key).ToArray();

				foreach (var sessionId in sessionIds)
				{
					await RemoveServiceAsync(sessionId);
				}
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				// Log errors during the loop
				Logger.LogError(ex, "An error occurred during timeout cleanup.");
			}
		}

		Logger.LogInformation($"{nameof(SessionScopedFactory<T>)} has stopped.");
	}
}
