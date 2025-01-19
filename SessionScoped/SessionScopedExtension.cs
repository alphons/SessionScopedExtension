using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension method to add session-scoped services to the dependency injection container.
/// </summary>
public static class SessionScopedExtension
{
	/// <summary>
	/// Adds a session-scoped IHostedService service to the services collection.
	/// </summary>
	/// <typeparam name="T">The type of the hosted service, which must implement <see cref="IHostedService" />.</typeparam>
	/// <param name="services">The service collection to add the service to.</param>
	/// <returns>The updated service collection.</returns>
	public static IServiceCollection AddSessionScoped<T>(this IServiceCollection services) where T : class, IHostedService
	{
		services.AddScoped<T>();
		services.AddSingleton<SessionScopedFactory<T>>();
		services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<SessionScopedFactory<T>>());

		return services;
	}
}
