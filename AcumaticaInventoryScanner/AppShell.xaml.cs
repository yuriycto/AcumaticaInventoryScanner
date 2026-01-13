/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file manages the application shell and navigation structure for the
 * Acumatica Inventory Scanner mobile application.
 */

using Microsoft.Extensions.DependencyInjection;
using AcuPower.AcumaticaInventoryScanner.Pages;

namespace AcuPower.AcumaticaInventoryScanner;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
		Routing.RegisterRoute("LoginPage", typeof(LoginPage));
	}

	public void LoadMainPage(IServiceProvider? serviceProvider)
	{
		try
		{
			if (serviceProvider == null)
			{
				// Try to get from handler if available
				serviceProvider = Handler?.MauiContext?.Services;
			}

			if (serviceProvider == null)
			{
				System.Diagnostics.Debug.WriteLine("Warning: ServiceProvider is null, cannot load MainPage - will retry when handler is available");
				// Try again when handler is available
				if (Handler == null)
				{
					HandlerChanged += OnHandlerChanged;
				}
				return;
			}

			System.Diagnostics.Debug.WriteLine("Attempting to resolve MainPage from service provider...");
			var mainPage = serviceProvider.GetService<MainPage>();
			
			if (mainPage != null)
			{
				System.Diagnostics.Debug.WriteLine("MainPage resolved successfully, adding to Shell...");
				Items.Clear();
				Items.Add(new ShellContent
				{
					Content = mainPage,
					Route = "MainPage",
					Title = "Home"
				});
				System.Diagnostics.Debug.WriteLine("MainPage loaded successfully into Shell");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("ERROR: MainPage could not be resolved from service provider");
				// Create a fallback page to prevent crash
				Items.Clear();
				Items.Add(new ShellContent
				{
					Content = new ContentPage 
					{ 
						Content = new Label 
						{ 
							Text = "Error: Could not load MainPage. Check debug output for details.",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center
						} 
					},
					Route = "ErrorPage",
					Title = "Error"
				});
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"EXCEPTION loading MainPage: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().FullName}");
			System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
			if (ex.InnerException != null)
			{
				System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
			}
			// Create a fallback page to prevent crash
			try
			{
				Items.Clear();
				Items.Add(new ShellContent
				{
					Content = new ContentPage 
					{ 
						Content = new Label 
						{ 
							Text = $"Error: {ex.Message}\n\nCheck Debug output for details.",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center
						} 
					},
					Route = "ErrorPage",
					Title = "Error"
				});
			}
			catch (Exception fallbackEx)
			{
				System.Diagnostics.Debug.WriteLine($"CRITICAL: Could not create fallback page: {fallbackEx.Message}");
			}
		}
	}

	private void OnHandlerChanged(object? sender, EventArgs e)
	{
		if (Handler?.MauiContext?.Services != null)
		{
			HandlerChanged -= OnHandlerChanged;
			LoadMainPage(Handler.MauiContext.Services);
		}
	}
}
