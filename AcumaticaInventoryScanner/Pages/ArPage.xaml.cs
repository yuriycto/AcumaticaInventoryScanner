/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This page demonstrates AR (Augmented Reality) overlay functionality for displaying
 * inventory information over scanned barcodes, showing integration possibilities with
 * Acumatica ERP data via API.
 */

using AcuPower.AcumaticaInventoryScanner.Services;
using ZXing.Net.Maui;
using Microsoft.Maui.Controls;
#if ANDROID
using Microsoft.Maui.ApplicationModel;
#endif

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class ArPage : ContentPage
{
    private readonly AuthService _authService;

    public ArPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
#if ANDROID
        CheckArSupport();
#else
        StatusLabel.Text = "ARCore only supported on Android";
#endif
    }

    private void CheckArSupport()
    {
#if ANDROID
        try
        {
            // ARCore support check - simplified for MAUI
            // Note: Full ARCore integration requires additional setup and native bindings
            // For now, we'll use a basic check and show AR overlay when barcode is detected
            // In MAUI, we can check AR support through the platform-specific code
            // For this demo, we assume AR is available if we're on Android
            StatusLabel.Text = "AR Mode Active! Point at barcode to see overlay.";
            
            // Note: Full ARCore integration requires:
            // 1. Adding Xamarin.Google.ARCore NuGet package (if available for MAUI)
            // 2. Checking ARCore availability using ArCoreApk.CheckAvailability()
            // 3. Creating an AR Session
            // 4. Attaching AR view to the page
            // 5. Tracking planes and objects
            // 6. Overlaying 3D content based on detected items
            // For now, we simulate AR by overlaying 2D labels when barcodes are detected
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"AR Error: {ex.Message}";
        }
#else
        StatusLabel.Text = "ARCore only supported on Android";
#endif
    }

    private void ArCameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var result = e.Results?.FirstOrDefault();
        if (result == null) return;

        Dispatcher.Dispatch(async () =>
        {
            // Simulate AR Overlay retrieval
            // Ideally we map 3D coordinates, but here we just show info on screen when detected
            
            // Debounce
            if (OverlayLayer.Children.Count > 1) return; // Already showing something

            var label = new Label
            {
                Text = $"InventoryID: {result.Value}\nStock: 42\nPrice: $99.99",
                TextColor = Colors.Lime,
                BackgroundColor = Color.FromRgba(0,0,0,150),
                Padding = 10
            };
            
            // Randomish position or center
            AbsoluteLayout.SetLayoutBounds(label, new Rect(0.5, 0.5, 200, 100));
            AbsoluteLayout.SetLayoutFlags(label, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.PositionProportional);
            
            OverlayLayer.Children.Add(label);
            
            // Remove after 3 seconds
            await Task.Delay(3000);
            OverlayLayer.Children.Remove(label);
        });
    }
}
