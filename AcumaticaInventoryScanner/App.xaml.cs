/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file contains the application lifecycle management for the Acumatica Inventory Scanner
 * mobile application.
 */

using Microsoft.Extensions.DependencyInjection;

namespace AcuPower.AcumaticaInventoryScanner;

public partial class App : Application
{
	private static MauiApp? _mauiApp;

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("App.CreateWindow called");
			var appShell = new AppShell();
			var window = new Window(appShell);
			
			// Load MainPage after window handler is set
			window.HandlerChanged += (s, e) =>
			{
				System.Diagnostics.Debug.WriteLine("Window HandlerChanged event fired");
				if (window.Handler?.MauiContext?.Services != null)
				{
					System.Diagnostics.Debug.WriteLine("Services available, loading MainPage...");
					appShell.LoadMainPage(window.Handler.MauiContext.Services);
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("Services not yet available in HandlerChanged");
				}
			};
			
			// Also try immediately if handler is already set
			if (window.Handler?.MauiContext?.Services != null)
			{
				System.Diagnostics.Debug.WriteLine("Services available immediately, loading MainPage...");
				appShell.LoadMainPage(window.Handler.MauiContext.Services);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Services not available yet, will wait for HandlerChanged");
			}
			
			return window;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"EXCEPTION in CreateWindow: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
			// Return a basic window to prevent complete failure
			return new Window(new AppShell());
		}
	}

	public static void SetMauiApp(MauiApp app)
	{
		_mauiApp = app;
	}
}