/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file contains the MAUI application startup configuration and dependency injection
 * setup for the Acumatica Inventory Scanner mobile application.
 */

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using CommunityToolkit.Maui;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using System.Reflection;
using System.Linq;
using Refit;
using AcuPower.AcumaticaInventoryScanner.Services;
using AcuPower.AcumaticaInventoryScanner.Pages;
using AcuPower.AcumaticaInventoryScanner.Controls;

namespace AcuPower.AcumaticaInventoryScanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseBarcodeReader()  // Proper ZXing initialization method
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		
		// Register custom handlers and keep ZXing registration as backup
		builder.ConfigureMauiHandlers(handlers =>
		{
#if ANDROID
			// Register ScannerOverlay handler for Android
			handlers.AddHandler<ScannerOverlay, AcuPower.AcumaticaInventoryScanner.Platforms.Android.ScannerOverlayHandler>();
#endif
			
			try
			{
				System.Diagnostics.Debug.WriteLine("Attempting to register ZXing handler...");
				
				// Search all loaded assemblies for the handler
				Type? handlerType = null;
				var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
					.Concat(new[] { typeof(CameraBarcodeReaderView).Assembly })
					.Distinct();
				
				foreach (var assembly in allAssemblies)
				{
					try
					{
						// Try various possible namespaces
						var possibleNames = new[]
						{
							"ZXing.Net.Maui.Handlers.CameraBarcodeReaderViewHandler",
							"ZXing.Net.Maui.Platforms.Android.CameraBarcodeReaderViewHandler",
							"ZXing.Net.Maui.Controls.Handlers.CameraBarcodeReaderViewHandler"
						};
						
						foreach (var name in possibleNames)
						{
							handlerType = assembly.GetType(name);
							if (handlerType != null)
							{
								System.Diagnostics.Debug.WriteLine($"Found handler: {handlerType.FullName} in {assembly.FullName}");
								break;
							}
						}
						
						if (handlerType == null)
						{
							// Search by name only
							handlerType = assembly.GetTypes()
								.FirstOrDefault(t => t.Name == "CameraBarcodeReaderViewHandler" && 
								                     typeof(IViewHandler).IsAssignableFrom(t));
							if (handlerType != null)
							{
								System.Diagnostics.Debug.WriteLine($"Found handler by name: {handlerType.FullName} in {assembly.FullName}");
								break;
							}
						}
						else
						{
							break;
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Error searching assembly {assembly.FullName}: {ex.Message}");
					}
				}
				
				if (handlerType != null)
				{
					// Use the default ZXing handler - we'll hook into it after creation
					handlers.AddHandler(typeof(CameraBarcodeReaderView), handlerType);
					System.Diagnostics.Debug.WriteLine($"ZXing handler registered successfully: {handlerType.FullName}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("ERROR: Could not find ZXing handler type in any assembly!");
					System.Diagnostics.Debug.WriteLine("Loaded assemblies:");
					foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName?.Contains("ZXing") == true))
					{
						System.Diagnostics.Debug.WriteLine($"  - {asm.FullName}");
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error registering ZXing handler: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
			}
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<PermissionsService>();
		
		// Add Refit HTTP client factory
		builder.Services.AddRefitClient<IAcumaticaApi>()
			.ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));
		
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<ItemDetailPage>();
		builder.Services.AddTransient<ArPage>();

		try
		{
			var app = builder.Build();
			App.SetMauiApp(app);
			System.Diagnostics.Debug.WriteLine("MauiApp built successfully");
			return app;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"ERROR building MauiApp: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
			throw;
		}
	}
}
