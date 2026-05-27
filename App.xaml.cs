using Microsoft.Extensions.DependencyInjection;

namespace SmartPark;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var mainPage = activationState?.Context.Services.GetRequiredService<MainPage>()
			?? throw new InvalidOperationException("MainPage is not available from the service provider.");

		return new Window(mainPage)
		{
			Title = "SmartPark"
		};
	}
}
