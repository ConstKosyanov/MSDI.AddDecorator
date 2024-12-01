using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSDI.Decorator.Tests;

public class AddDecoratorTests
{
	[Theory, MemberData(nameof(ServiceDescriptors))]
	public void AddDecorator_ShouldAddServiceDescriptorWithTheSameInterface(ServiceDescriptor serviceDescriptor)
	{
		var services = new ServiceCollection()
			.Add(serviceDescriptor);

		Assert.Single(services);

		services.AddDecorator<IInteface, Level1>();

		Assert.Equal(2, services.Count);

		Assert.All(services, x => Assert.Equal(serviceDescriptor.ServiceType, x.ServiceType));
	}

	[Theory, MemberData(nameof(ServiceDescriptors))]
	public void Decorator_ShouldCopyLifetime(ServiceDescriptor serviceDescriptor)
	{
		var services = new ServiceCollection()
			.Add(serviceDescriptor)
			.AddDecorator<IInteface, Level1>();

		Assert.Equal(serviceDescriptor.Lifetime, services[1].Lifetime);
	}

	[Theory, MemberData(nameof(ServiceDescriptors))]
	public void GetService_WithAddDecorator_ShouldReturnDecoratorType(ServiceDescriptor serviceDescriptor)
	{
		var service = new ServiceCollection()
			.Add(serviceDescriptor)
			.AddDecorator<IInteface, Level1>()
			.BuildServiceProvider()
			.GetRequiredService<IInteface>();

		Assert.True(service is Level1
		{
			Inner: Level0
		});

		Assert.Equal(2, service.Foo());
	}

	[Theory, MemberData(nameof(ServiceDescriptors))]
	public void GetService_WithDecorateDecorator_ShouldReturnDecoratorType(ServiceDescriptor serviceDescriptor)
	{
		var service = new ServiceCollection()
			.Add(serviceDescriptor)
			.AddDecorator<IInteface, Level1>()
			.AddDecorator<IInteface, Level2>()
			.BuildServiceProvider()
			.GetRequiredService<IInteface>();

		Assert.True(service is Level2
		{
			Inner: Level1
			{
				Inner: Level0
			}
		});

		Assert.Equal(3, service.Foo());
	}

	public static TheoryData<ServiceDescriptor> ServiceDescriptors { get; } = [
		new ServiceDescriptor(typeof(IInteface), typeof(Level0), ServiceLifetime.Scoped),
		new ServiceDescriptor(typeof(IInteface), typeof(Level0), ServiceLifetime.Singleton),
		new ServiceDescriptor(typeof(IInteface), typeof(Level0), ServiceLifetime.Transient),
		new ServiceDescriptor(typeof(IInteface), _ => new Level0(), ServiceLifetime.Scoped),
		new ServiceDescriptor(typeof(IInteface), _ => new Level0(), ServiceLifetime.Singleton),
		new ServiceDescriptor(typeof(IInteface), _ => new Level0(), ServiceLifetime.Transient),
		new ServiceDescriptor(typeof(IInteface), new Level0())
	];
}

#region Test entities
//=============================================================================

internal interface IInteface
{
	int Foo();
}

internal class Level0 : IInteface
{
	public int Foo() => 1;
}

internal class Level1(IInteface inner) : IInteface
{
	public IInteface Inner => inner;

	public int Foo() => inner.Foo() + 1;
}

internal class Level2(IInteface inner) : IInteface
{
	public IInteface Inner => inner;
	public int Foo() => inner.Foo() + 1;
}

//=============================================================================
#endregion