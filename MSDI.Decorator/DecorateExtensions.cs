using Microsoft.Extensions.DependencyInjection;

namespace MSDI.Decorator;

public static class DecorateExtensions
{
	public static IServiceCollection AddDecorator<TService, TDecorator>(this IServiceCollection services)
	{
		var descriptor = services.LastOrDefault(x => x.ServiceType == typeof(TService))
			?? throw new InvalidOperationException($"{typeof(TService).Name} not registered");

		Func<IServiceProvider, TService> decoratorFactory = descriptor switch
		{
			{ ImplementationFactory: Func<IServiceProvider, object> factory } => s => (TService) factory(s),
			{ ImplementationInstance: TService instance } => _ => instance,
			{ ImplementationType: Type } => s => (TService) ActivatorUtilities.CreateInstance(s, descriptor.ImplementationType),
			_ => throw new NotImplementedException()
		};

		var decorator = new ServiceDescriptor(
			serviceType: typeof(TService),
			factory: s => ActivatorUtilities.CreateInstance<TDecorator>(s, decoratorFactory(s)!)!,
			lifetime: descriptor.Lifetime);
		services.Add(decorator);
		return services;
	}
}
