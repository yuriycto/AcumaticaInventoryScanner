/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file contains the main scanning page logic for the Acumatica Inventory Scanner
 * mobile application, demonstrating barcode scanning and Acumatica ERP API integration.
 */

using AcuPower.AcumaticaInventoryScanner.Controls;
using AcuPower.AcumaticaInventoryScanner.Models;
using AcuPower.AcumaticaInventoryScanner.Pages;
using AcuPower.AcumaticaInventoryScanner.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Refit;
using System.Net;
using System.Reflection;
using System.Threading;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AcuPower.AcumaticaInventoryScanner;

public partial class MainPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly DatabaseService _dbService;
    private readonly SettingsService _settingsService;
    private readonly PermissionsService _permissionsService;
    private readonly IServiceProvider _serviceProvider;
    private bool _isScanning = true;

    public MainPage(AuthService authService, DatabaseService dbService, SettingsService settingsService, PermissionsService permissionsService, IServiceProvider serviceProvider)
    {
        try
    {
        InitializeComponent();
        _authService = authService;
        _dbService = dbService;
        _settingsService = settingsService;
            _permissionsService = permissionsService;
        _serviceProvider = serviceProvider;
        
            // Don't create camera view here - wait for page to be fully loaded
            System.Diagnostics.Debug.WriteLine("MainPage initialized, camera view will be created in OnAppearing");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing MainPage: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void CameraView_HandlerChanged(object? sender, EventArgs e)
    {
        // Ensure Options are set if handler is ready
        try
        {
            if (cameraView != null && cameraView.Handler != null)
            {
                if (cameraView.Options == null)
                {
                    cameraView.Options = new BarcodeReaderOptions
                    {
                        Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };
                    System.Diagnostics.Debug.WriteLine("Camera view Options set in HandlerChanged (backup)");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CameraView_HandlerChanged: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private CameraBarcodeReaderView? cameraView;
    private const double ScanningRectWidth = 400;  // Increased from 250 for easier scanning
    private const double ScanningRectHeight = 150; // Reduced from 200 - less tall but same width
    private Rect? _scanningRectBounds; // Store the actual bounds of the scanning rectangle
    private BoxView? _scanningLine; // Animated red scanning line
    private ScannerOverlay? _scannerOverlay; // Custom semi-transparent overlay for Android

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isScanning = true;
        
        // Request camera permission first
        var hasCameraPermission = await _permissionsService.RequestCameraPermissionAsync();
        if (!hasCameraPermission)
        {
            await DisplayAlert("Permission Required", "Camera permission is required for barcode scanning. Please enable it in app settings.", "OK");
            return;
        }
        
        // Create camera view programmatically AFTER page is loaded and permissions are granted
        if (cameraView == null && MainGrid != null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creating camera view programmatically...");
                
                // Create camera view with Options set BEFORE adding to visual tree
                cameraView = new CameraBarcodeReaderView
                {
                    Options = new BarcodeReaderOptions
                    {
                        Formats = BarcodeFormats.All,
                        AutoRotate = true,
                        Multiple = false,
                        // Note: ZXing.Net.Maui doesn't support ROI (Region of Interest) directly
                        // The entire camera view is processed. We'll filter results visually
                        // by only showing confirmation for barcodes, but all detected barcodes
                        // will be processed. For true ROI support, camera preview cropping would be needed.
                    },
                    IsDetecting = false, // Start with detection off
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    BackgroundColor = Colors.Black // Ensure background is black to match black bars
                };
                
                cameraView.BarcodesDetected += CameraBarcodeReaderView_BarcodesDetected;
                cameraView.HandlerChanged += CameraView_HandlerChanged;
                
                        // Insert at index 0 so it's behind other controls (camera should be at the bottom layer)
                        MainGrid.Insert(0, cameraView);
                        
                        // Ensure overlay is on top by bringing it to front
                        if (OverlayLayout != null)
                        {
                            MainGrid.Children.Remove(OverlayLayout);
                            MainGrid.Children.Add(OverlayLayout);
                        }
                
                System.Diagnostics.Debug.WriteLine("Camera view created and added to grid");
                
#if ANDROID
                // Hook into handler to apply frame cropping for ROI scanning
                cameraView.HandlerChanged += (s, e) =>
                {
                    if (cameraView.Handler != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Handler ready, applying frame cropping for ROI scanning...");
                        // Delay slightly to ensure camera is fully initialized
                        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1000), () =>
                        {
                            Platforms.Android.CameraBarcodeReaderViewExtensions.ApplyFrameCropping(cameraView.Handler);
                        });
                    }
                };
#endif
                
                // Create dark overlay after camera is added and layout is ready
                // Wait longer to ensure camera view is fully initialized
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
                {
                    CreateDarkOverlay();
                });
                
                // Also recreate overlay when layout changes
                MainGrid.SizeChanged += (s, e) =>
                {
                    if (MainGrid.Width > 0 && MainGrid.Height > 0)
                    {
                        // Delay to ensure layout is stable
                        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
                        {
                            CreateDarkOverlay();
                            // Restart animation after layout change to prevent it from getting stuck
                            if (_isScanning)
                            {
                                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), () =>
                                {
                                    StartScanningLineAnimation();
                                });
                            }
                        });
                    }
                };
                
                // Also recreate overlay when camera view size changes
                if (cameraView != null)
                {
                    cameraView.SizeChanged += (s, e) =>
                    {
                        if (cameraView.Width > 0 && cameraView.Height > 0)
                        {
                            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
                            {
                                CreateDarkOverlay();
                                // Restart animation after layout change to prevent it from getting stuck
                                if (_isScanning)
                                {
                                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), () =>
                                    {
                                        StartScanningLineAnimation();
                                    });
                                }
                            });
                        }
                    };
                }
                
                // Enable detection after a delay to ensure handler is ready
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
                {
                    if (cameraView != null)
                    {
                        cameraView.IsDetecting = true;
                        System.Diagnostics.Debug.WriteLine("Camera detection enabled");
                        
                        // Start scanning line animation
                        CreateScanningLine();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating camera view: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Failed to initialize camera: {ex.Message}", "OK");
            }
        }
        
        var token = await _settingsService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            var loginPage = _serviceProvider.GetService<LoginPage>();
            if (loginPage != null)
            {
            await Navigation.PushModalAsync(new NavigationPage(loginPage));
            }
        }
    }

    private void ConfigureCameraClipping()
    {
        // NOTE: ZXing.Net.Maui limitation
        // The library processes the entire camera view, not just the scanning rectangle area.
        // Visual clipping (Clip property) only affects display, not barcode detection.
        // 
        // To truly restrict scanning to the rectangle, we would need:
        // 1. Platform-specific camera preview cropping (Android/iOS native code)
        // 2. Image processing to crop frames before passing to ZXing
        // 3. A different library that supports ROI natively
        //
        // For now, the scanning rectangle serves as a visual guide, and the confirmation
        // dialog ensures the user intended to scan the barcode they see.
    }

           private void CreateDarkOverlay()
           {
               System.Diagnostics.Debug.WriteLine("=== CreateDarkOverlay START ===");

               if (OverlayLayout == null || ScanningRectangle == null)
               {
                   System.Diagnostics.Debug.WriteLine("ERROR: OverlayLayout or ScanningRectangle is null!");
                   return;
               }

               // Wait for layout to be measured
               MainThread.BeginInvokeOnMainThread(() =>
               {
                   // Ensure MainGrid fills the screen
                   if (MainGrid != null)
                   {
                       MainGrid.HorizontalOptions = LayoutOptions.Fill;
                       MainGrid.VerticalOptions = LayoutOptions.Fill;
                   }
                   
                   // Get actual layout dimensions
                   double screenWidth = MainGrid?.Width > 0 ? MainGrid.Width : (OverlayLayout.Width > 0 ? OverlayLayout.Width : DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
                   double screenHeight = MainGrid?.Height > 0 ? MainGrid.Height : (OverlayLayout.Height > 0 ? OverlayLayout.Height : DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density);
            
                   // If still not ready, use display info
                   if (screenWidth <= 0 || screenHeight <= 0)
                   {
                       var displayInfo = DeviceDisplay.MainDisplayInfo;
                       screenWidth = displayInfo.Width / displayInfo.Density;
                       screenHeight = displayInfo.Height / displayInfo.Density;
                       System.Diagnostics.Debug.WriteLine($"Using DeviceDisplay info: {screenWidth}x{screenHeight} (density: {displayInfo.Density})");
                   }
                   else
                   {
                       System.Diagnostics.Debug.WriteLine($"Using layout dimensions: {screenWidth}x{screenHeight}");
                   }
            
                   // Get the actual position of the scanning rectangle (centered)
                   double centerX = screenWidth / 2;
                   double centerY = screenHeight / 2;
            
                   // Calculate exact bounds of scanning rectangle
                   double rectLeft = centerX - (ScanningRectWidth / 2);
                   double rectTop = centerY - (ScanningRectHeight / 2);
            
                   System.Diagnostics.Debug.WriteLine($"Scanning rectangle bounds:");
                   System.Diagnostics.Debug.WriteLine($"  Center: ({centerX}, {centerY})");
                   System.Diagnostics.Debug.WriteLine($"  Size: {ScanningRectWidth}x{ScanningRectHeight}");
                   System.Diagnostics.Debug.WriteLine($"  Position: Left={rectLeft}, Top={rectTop}");
            
                   // Store the scanning rectangle bounds for filtering barcodes
                   _scanningRectBounds = new Rect(rectLeft, rectTop, ScanningRectWidth, ScanningRectHeight);
            
                   // Update the Android handler with the scanning rectangle bounds
                   Services.ScanningRegionService.ScanningRectBounds = _scanningRectBounds;
                   if (_scanningRectBounds.HasValue)
                   {
                       var bounds = _scanningRectBounds.Value;
                       System.Diagnostics.Debug.WriteLine($"ScanningRegionService updated with bounds: X={bounds.X}, Y={bounds.Y}, Width={bounds.Width}, Height={bounds.Height}");
                   }

#if ANDROID
                   // On Android, use the custom ScannerOverlay that properly handles transparency over camera SurfaceView
                   CreateAndroidOverlay(screenWidth, screenHeight, rectLeft, rectTop);
#else
                   // On other platforms, use BoxViews (they work correctly on iOS)
                   CreateBoxViewOverlay(screenWidth, screenHeight, rectLeft, rectTop);
#endif
                   
                   // Create and start scanning line animation after a short delay
                   Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
                   {
                       CreateScanningLine();
                   });
               });
           }

#if ANDROID
           private void CreateAndroidOverlay(double screenWidth, double screenHeight, double rectLeft, double rectTop)
           {
               System.Diagnostics.Debug.WriteLine("Creating Android-specific ScannerOverlay...");
               
               if (MainGrid == null) return;
               
               // Remove existing scanner overlay if any
               if (_scannerOverlay != null)
               {
                   MainGrid.Children.Remove(_scannerOverlay);
               }
               
               // Also remove any old BoxViews from OverlayLayout
               if (OverlayLayout != null)
               {
                   var boxesToRemove = OverlayLayout.Children.OfType<BoxView>().ToList();
                   foreach (var box in boxesToRemove)
                   {
                       OverlayLayout.Children.Remove(box);
                   }
               }
               
               // Create the custom Android overlay
               _scannerOverlay = new ScannerOverlay
               {
                   ScanRectX = rectLeft,
                   ScanRectY = rectTop,
                   ScanRectWidth = ScanningRectWidth,
                   ScanRectHeight = ScanningRectHeight,
                   OverlayOpacity = 0.5, // 50% transparent
                   HorizontalOptions = LayoutOptions.Fill,
                   VerticalOptions = LayoutOptions.Fill,
                   InputTransparent = true, // Allow touch events to pass through
                   ZIndex = 500 // Above camera but below buttons
               };
               
               // Add to MainGrid (after camera view)
               MainGrid.Children.Add(_scannerOverlay);
               
               // Ensure OverlayLayout with the scanning rectangle border is on top
               if (OverlayLayout != null)
               {
                   MainGrid.Children.Remove(OverlayLayout);
                   MainGrid.Children.Add(OverlayLayout);
               }
               
               System.Diagnostics.Debug.WriteLine($"Android ScannerOverlay created: Rect=({rectLeft}, {rectTop}, {ScanningRectWidth}, {ScanningRectHeight})");
           }
#endif

           private void CreateBoxViewOverlay(double screenWidth, double screenHeight, double rectLeft, double rectTop)
           {
               if (OverlayLayout == null) return;
               
               System.Diagnostics.Debug.WriteLine("Creating BoxView overlay (non-Android)...");
               
               // Clear existing overlay BoxViews
               var boxesToRemove = OverlayLayout.Children.OfType<BoxView>().ToList();
               foreach (var box in boxesToRemove)
               {
                   OverlayLayout.Children.Remove(box);
               }

               // Ensure overlay layout fills the screen
               OverlayLayout.HorizontalOptions = LayoutOptions.Fill;
               OverlayLayout.VerticalOptions = LayoutOptions.Fill;
               OverlayLayout.IsVisible = true;
               OverlayLayout.Opacity = 1.0;
               OverlayLayout.InputTransparent = false;
               OverlayLayout.ZIndex = 1000;
               
               double rectRight = rectLeft + ScanningRectWidth;
               double rectBottom = rectTop + ScanningRectHeight;
            
               // Semi-transparent black for overlay
               var semiTransparentBlack = Color.FromRgba(0, 0, 0, 0.5);
            
               // Top dark area
               var topBox = new BoxView { Color = semiTransparentBlack, IsVisible = true, InputTransparent = true };
               AbsoluteLayout.SetLayoutBounds(topBox, new Rect(0, 0, screenWidth, rectTop + 1));
               AbsoluteLayout.SetLayoutFlags(topBox, AbsoluteLayoutFlags.None);
               OverlayLayout.Children.Add(topBox);
            
               // Bottom dark area
               var bottomBox = new BoxView { Color = semiTransparentBlack, IsVisible = true, InputTransparent = true };
               double bottomY = rectBottom - 1;
               AbsoluteLayout.SetLayoutBounds(bottomBox, new Rect(0, bottomY, screenWidth, screenHeight - bottomY));
               AbsoluteLayout.SetLayoutFlags(bottomBox, AbsoluteLayoutFlags.None);
               OverlayLayout.Children.Add(bottomBox);
            
               // Left dark area
               var leftBox = new BoxView { Color = semiTransparentBlack, IsVisible = true, InputTransparent = true };
               AbsoluteLayout.SetLayoutBounds(leftBox, new Rect(0, rectTop - 1, rectLeft + 1, ScanningRectHeight + 2));
               AbsoluteLayout.SetLayoutFlags(leftBox, AbsoluteLayoutFlags.None);
               OverlayLayout.Children.Add(leftBox);
            
               // Right dark area
               var rightBox = new BoxView { Color = semiTransparentBlack, IsVisible = true, InputTransparent = true };
               AbsoluteLayout.SetLayoutBounds(rightBox, new Rect(rectRight - 1, rectTop - 1, screenWidth - rectRight + 1, ScanningRectHeight + 2));
               AbsoluteLayout.SetLayoutFlags(rightBox, AbsoluteLayoutFlags.None);
               OverlayLayout.Children.Add(rightBox);
               
               System.Diagnostics.Debug.WriteLine($"=== CreateDarkOverlay END - Created {OverlayLayout.Children.Count} BoxView overlays ===");
           }
           
           private void CreateScanningLine()
           {
               if (ScanningRectangle == null) return;
               
               // If scanning line already exists, just restart the animation
               if (_scanningLine != null)
               {
                   System.Diagnostics.Debug.WriteLine("Scanning line already exists, restarting animation");
                   StartScanningLineAnimation();
                   return;
               }
               
               // Create red scanning line
               _scanningLine = new BoxView
               {
                   Color = Colors.Red,
                   HeightRequest = 3,
                   WidthRequest = ScanningRectWidth - 20, // Slightly smaller than rectangle width
                   HorizontalOptions = LayoutOptions.Center,
                   VerticalOptions = LayoutOptions.Start,
                   Opacity = 0.9,
                   Shadow = new Shadow
                   {
                       Brush = new SolidColorBrush(Colors.Red),
                       Offset = new Point(0, 0),
                       Radius = 8,
                       Opacity = 0.8f
                   }
               };
               
               ScanningRectangle.Children.Add(_scanningLine);
               
               // Start the animation
               StartScanningLineAnimation();
               
               System.Diagnostics.Debug.WriteLine("Scanning line created and animation started");
           }
           
           private void StartScanningLineAnimation()
           {
               if (_scanningLine == null || !_isScanning) return;
               
               // Abort any existing animation first
               ScanningRectangle?.AbortAnimation("ScanningLineAnimation");
               
               // Reset position to top
               _scanningLine.TranslationY = 0;
               
               System.Diagnostics.Debug.WriteLine("Starting scanning line animation...");
               
               // Create a fresh animation each time to avoid state issues
               var scanHeight = ScanningRectHeight - 2;
               var animation = new Animation();
               
               // Down animation (top to bottom)
               animation.Add(0, 0.5, new Animation(v => 
               {
                   if (_scanningLine != null) _scanningLine.TranslationY = v;
               }, 0, scanHeight));
               
               // Up animation (bottom to top)
               animation.Add(0.5, 1.0, new Animation(v => 
               {
                   if (_scanningLine != null) _scanningLine.TranslationY = v;
               }, scanHeight, 0));
               
               // Commit with repeat: true for infinite looping (handled by MAUI)
               animation.Commit(ScanningRectangle, "ScanningLineAnimation", 16, 2000, Easing.Linear, 
                   finished: null, 
                   repeat: () => _isScanning && _scanningLine != null); // Return true to repeat while scanning
           }

    private void CameraBarcodeReaderView_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (!_isScanning) return;
        
        if (e.Results == null || !e.Results.Any()) return;

        // Filter barcodes to only process those within the scanning rectangle
        var resultsInRect = FilterBarcodesInScanningArea(e.Results);
        if (!resultsInRect.Any()) return; // No barcodes in scanning area
        
        var result = resultsInRect.FirstOrDefault();
        if (result == null) return;

        // Prevent multiple simultaneous scans - set flag immediately
        _isScanning = false;

        // Barcode detection runs on a background thread
        // Show confirmation dialog on main thread before querying
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // Show confirmation dialog with the scanned barcode value
                var scannedValue = result.Value;
                var confirmed = await DisplayAlert(
                    "Barcode Scanned",
                    $"Do you want to look for item: {scannedValue}?",
                    "Yes, Search",
                    "Cancel");

                if (confirmed)
                {
                    // User confirmed - proceed with query
                    await QueryInventoryItemAsync(scannedValue);
                     }
                     else
                     {
                    // User cancelled - re-enable scanning
                     _isScanning = true;
                     // Restart scanning line animation
                     CreateScanningLine();
                 }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in barcode detection: {ex.Message}");
                await DisplayAlert("Error", $"Failed to process barcode: {ex.Message}", "OK");
                _isScanning = true; // Re-enable scanning
                // Restart scanning line animation
                CreateScanningLine();
            }
        });
    }

    private List<BarcodeResult> FilterBarcodesInScanningArea(IReadOnlyList<BarcodeResult> results)
    {
        // IMPORTANT LIMITATION:
        // ZXing.Net.Maui's BarcodeResult class doesn't expose position/coordinate information.
        // The library processes the entire camera view, not just the scanning rectangle.
        //
        // To truly restrict scanning to the rectangle area, we would need:
        // 1. Platform-specific camera preview cropping (Android/iOS native code)
        // 2. Image processing to crop frames before passing to ZXing
        // 3. A different barcode library that supports ROI (Region of Interest) natively
        //
        // Current workaround: The scanning rectangle serves as a visual guide.
        // The confirmation dialog ensures the user intended to scan the barcode.
        // 
        // For production use, consider implementing camera frame cropping in platform-specific code.
        // This would require modifying the ZXing handler to crop the camera preview to the
        // scanning rectangle bounds before processing.
        
        // For now, return all results since we can't filter by position without position data
        return results?.ToList() ?? new List<BarcodeResult>();
    }

    private async void OnArClicked(object sender, EventArgs e)
    {
        // Navigate to AR Page
         var arPage = _serviceProvider.GetService<ArPage>();
         await Navigation.PushAsync(arPage);
    }

    private async void OnManualEntryClicked(object sender, EventArgs e)
    {
        string manualCode = await DisplayPromptAsync("Manual Entry", "Enter barcode or Inventory ID:");
        if (!string.IsNullOrWhiteSpace(manualCode))
        {
            // Reuse the same logic as barcode scanning
            await QueryInventoryItemAsync(manualCode);
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirmed = await DisplayAlert(
            "Logout",
            "This will clear your saved credentials and allow you to login with a different API version.\n\nDo you want to logout?",
            "Yes, Logout",
            "Cancel");
        
        if (confirmed)
        {
            // Clear all saved settings
            _settingsService.Clear();
            _authService.ClearApiCache();
            
            System.Diagnostics.Debug.WriteLine("User logged out - all cached credentials cleared");
            
            // Navigate to login page
            var loginPage = _serviceProvider.GetService<LoginPage>();
            if (loginPage != null)
            {
                await Navigation.PushModalAsync(new NavigationPage(loginPage));
            }
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = _serviceProvider.GetService<Pages.SettingsPage>();
            if (settingsPage != null)
            {
                await Navigation.PushAsync(settingsPage);
            }
            else
            {
                await DisplayAlert("Error", "Could not load Settings page.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to Settings: {ex.Message}");
            await DisplayAlert("Error", $"Navigation failed: {ex.Message}", "OK");
        }
    }

    private async Task QueryInventoryItemAsync(string itemIdentifier)
    {
        if (string.IsNullOrWhiteSpace(itemIdentifier)) return;

        // Update UI on main thread
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;
        });

        try
        {
            // Get API version from settings
            var apiVersion = await _settingsService.GetApiVersionAsync() ?? "24.200.001";
            System.Diagnostics.Debug.WriteLine($"Using API version: {apiVersion}");
            
            // Create API client on background thread to avoid deadlocks
            var api = await _authService.GetApiAsync();
            if (api != null)
            {
                // Try multiple search approaches for Acumatica
                // First try: Search by InventoryID
                // Second try: Search by barcode/alternate ID
                List<InventoryItem>? items = null;
                
                // Add timeout to prevent hanging
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    try
                    {
                        // Try multiple search approaches for Acumatica
                        var escapedId = itemIdentifier.Replace("'", "''").Trim(); // Escape single quotes and trim whitespace
                        
                        System.Diagnostics.Debug.WriteLine($"Searching for item: '{itemIdentifier}' (escaped: '{escapedId}')");
                        
                        // Expand WarehouseDetails to get per-warehouse quantities
                        const string expandParam = "WarehouseDetails";
                        
                        // First: Try OData response format (wrapped in "value")
                        try
                        {
                            var filter = $"InventoryID eq '{escapedId}'";
                            System.Diagnostics.Debug.WriteLine($"Trying filter: {filter} with expand: {expandParam}");
                            var odataResponse = await api.GetInventoryItemsAsync(apiVersion, filter, expandParam).WaitAsync(cts.Token);
                            items = odataResponse?.Value;
                            System.Diagnostics.Debug.WriteLine($"OData response: {items?.Count ?? 0} items");
                        }
                        catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            System.Diagnostics.Debug.WriteLine($"OData format failed with 401 Unauthorized: {apiEx.Message}");
                            System.Diagnostics.Debug.WriteLine("Token may be expired, clearing API cache...");
                            _authService.ClearApiCache();
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                var loginPage = _serviceProvider.GetService<LoginPage>();
                                if (loginPage != null)
                                {
                                    await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                }
                            });
                            return; // Exit early, user needs to re-authenticate
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"OData format failed, trying direct list: {ex.Message}");
                            // Fallback to direct list format
                            try
                            {
                                var filter = $"InventoryID eq '{escapedId}'";
                                items = await api.GetInventoryItemsListAsync(apiVersion, filter, expandParam).WaitAsync(cts.Token);
                                System.Diagnostics.Debug.WriteLine($"Direct list: {items?.Count ?? 0} items");
                            }
                            catch (Refit.ApiException apiEx2) when (apiEx2.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                System.Diagnostics.Debug.WriteLine($"Direct list also failed with 401 Unauthorized: {apiEx2.Message}");
                                System.Diagnostics.Debug.WriteLine("Token may be expired, clearing API cache...");
                                _authService.ClearApiCache();
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                    var loginPage = _serviceProvider.GetService<LoginPage>();
                                    if (loginPage != null)
                                    {
                                        await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                    }
                                });
                                return; // Exit early, user needs to re-authenticate
                            }
                            catch (Exception ex2)
                            {
                                System.Diagnostics.Debug.WriteLine($"Direct list also failed: {ex2.Message}");
                            }
                        }
                        
                        // If not found, try searching with 'contains' instead of exact match
                        if (items == null || items.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Item not found by exact InventoryID match, trying contains search...");
                            try
                            {
                                var filter = $"contains(InventoryID, '{escapedId}')";
                                System.Diagnostics.Debug.WriteLine($"Trying filter: {filter} with expand: {expandParam}");
                                var odataResponse = await api.GetInventoryItemsAsync(apiVersion, filter, expandParam).WaitAsync(cts.Token);
                                items = odataResponse?.Value;
                                System.Diagnostics.Debug.WriteLine($"Contains match result: {items?.Count ?? 0} items");
                            }
                            catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                System.Diagnostics.Debug.WriteLine($"Contains search failed with 401 Unauthorized");
                                _authService.ClearApiCache();
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                    var loginPage = _serviceProvider.GetService<LoginPage>();
                                    if (loginPage != null)
                                    {
                                        await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                    }
                                });
                                return;
                            }
                            catch
                            {
                                try
                                {
                                    var filter = $"contains(InventoryID, '{escapedId}')";
                                    items = await api.GetInventoryItemsListAsync(apiVersion, filter, expandParam).WaitAsync(cts.Token);
                                }
                                catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Contains search (direct list) failed with 401 Unauthorized");
                                    _authService.ClearApiCache();
                                    await MainThread.InvokeOnMainThreadAsync(async () =>
                                    {
                                        await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                        var loginPage = _serviceProvider.GetService<LoginPage>();
                                        if (loginPage != null)
                                        {
                                            await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                        }
                                    });
                                    return;
                                }
                                catch { }
                            }
                        }
                        
                        // If still not found, try startswith
                        if (items == null || items.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Trying startswith search...");
                            try
                            {
                                var filter = $"startswith(InventoryID, '{escapedId}')";
                                System.Diagnostics.Debug.WriteLine($"Trying filter: {filter} with expand: {expandParam}");
                                var odataResponse = await api.GetInventoryItemsAsync(apiVersion, filter, expandParam).WaitAsync(cts.Token);
                                items = odataResponse?.Value;
                                System.Diagnostics.Debug.WriteLine($"StartsWith match result: {items?.Count ?? 0} items");
                            }
                            catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                System.Diagnostics.Debug.WriteLine($"StartsWith search failed with 401 Unauthorized");
                                _authService.ClearApiCache();
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                    var loginPage = _serviceProvider.GetService<LoginPage>();
                                    if (loginPage != null)
                                    {
                                        await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                    }
                                });
                                return;
                            }
                            catch
                            {
                                try
                                {
                                    var filter = $"startswith(InventoryID, '{escapedId}')";
                                    items = await api.GetInventoryItemsListAsync(apiVersion, filter, expandParam).WaitAsync(cts.Token);
                                }
                                catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    System.Diagnostics.Debug.WriteLine($"StartsWith search (direct list) failed with 401 Unauthorized");
                                    _authService.ClearApiCache();
                                    await MainThread.InvokeOnMainThreadAsync(async () =>
                                    {
                                        await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                        var loginPage = _serviceProvider.GetService<LoginPage>();
                                        if (loginPage != null)
                                        {
                                            await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                        }
                                    });
                                    return;
                                }
                                catch { }
                            }
                        }
                        
                        // If still not found, try without filter to see if API is working at all
                        if (items == null || items.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Trying to get all items (no filter) to verify API connection...");
                            try
                            {
                                // Try OData format first (without expand for performance when getting all)
                                var allItemsResponse = await api.GetInventoryItemsAsync(apiVersion, null, expandParam).WaitAsync(cts.Token);
                                var allItems = allItemsResponse?.Value;
                                System.Diagnostics.Debug.WriteLine($"API returned {allItems?.Count ?? 0} total items");
                                
                                if (allItems != null && allItems.Count > 0)
                                {
                                    // Log first few item IDs for debugging
                                    var firstFew = allItems.Take(5).Select(i => 
                                        i.GetInventoryId() ?? "null"
                                    ).ToList();
                                    System.Diagnostics.Debug.WriteLine($"First 5 item IDs: {string.Join(", ", firstFew)}");
                                    
                                    // Try client-side filtering as last resort
                                    items = allItems.Where(i => 
                                        i.GetInventoryId().Equals(itemIdentifier, StringComparison.OrdinalIgnoreCase) ||
                                        i.GetInventoryId().Contains(itemIdentifier, StringComparison.OrdinalIgnoreCase) ||
                                        i.Id?.Equals(itemIdentifier, StringComparison.OrdinalIgnoreCase) == true
                                    ).ToList();
                                    System.Diagnostics.Debug.WriteLine($"Client-side filter found: {items?.Count ?? 0} items");
                                }
                            }
                            catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                System.Diagnostics.Debug.WriteLine($"Get all items failed with 401 Unauthorized");
                                _authService.ClearApiCache();
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                    var loginPage = _serviceProvider.GetService<LoginPage>();
                                    if (loginPage != null)
                                    {
                                        await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                    }
                                });
                                return;
                            }
                            catch
                            {
                                // Fallback to direct list format
                                try
                                {
                                    var allItems = await api.GetInventoryItemsListAsync(apiVersion, null, expandParam).WaitAsync(cts.Token);
                                    if (allItems != null && allItems.Count > 0)
                                    {
                                        items = allItems.Where(i => 
                                            i.InventoryID?.Value?.ToString()?.Equals(itemIdentifier, StringComparison.OrdinalIgnoreCase) == true ||
                                            i.InventoryID?.Value?.ToString()?.Contains(itemIdentifier, StringComparison.OrdinalIgnoreCase) == true ||
                                            i.Id?.Equals(itemIdentifier, StringComparison.OrdinalIgnoreCase) == true
                                        ).ToList();
                                    }
                                }
                                catch (Refit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error getting all items: 401 Unauthorized");
                                    _authService.ClearApiCache();
                                    await MainThread.InvokeOnMainThreadAsync(async () =>
                                    {
                                        await DisplayAlert("Authentication Error", "Your session has expired. Please log in again.", "OK");
                                        var loginPage = _serviceProvider.GetService<LoginPage>();
                                        if (loginPage != null)
                                        {
                                            await Navigation.PushModalAsync(new NavigationPage(loginPage));
                                        }
                                    });
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error getting all items: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Refit.ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
                    {
                        // 404 means the endpoint or item doesn't exist
                        // Re-throw to be caught by outer handler
                        throw;
                    }

                    // All UI operations must be on main thread
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (items != null && items.Count > 0)
                        {
                            var item = items.First();
                            // Save to local cache
                            await _dbService.SaveItemAsync(item);
                            
                            // Navigate to detail
                            var detailPage = _serviceProvider.GetService<ItemDetailPage>();
                            if (detailPage != null)
                            {
                                detailPage.SetItem(item);
                                await Navigation.PushAsync(detailPage);
                            }
                        }
                        else
                        {
                            await DisplayAlert("Not Found", $"Item '{itemIdentifier}' not found in Acumatica.", "OK");
                            _isScanning = true;
                        }
                    });
                }
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", "Not connected to Acumatica. Please login again.", "OK");
                    _isScanning = true;
                });
            }
        }
        catch (TaskCanceledException)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Timeout", "The request took too long. Please check your connection and try again.", "OK");
                _isScanning = true;
            });
        }
        catch (Refit.ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
        {
            // Handle 404 Not Found - item doesn't exist or endpoint is wrong
            var currentApiVersion = await _settingsService.GetApiVersionAsync() ?? "24.200.001";
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert(
                    "Not Found (404)", 
                    $"Item '{itemIdentifier}' was not found.\n\nPossible reasons:\n• The item doesn't exist in Acumatica\n• The Inventory ID is different\n• Wrong API version (current: {currentApiVersion})\n\nTo fix API version issue:\n1. Log out (go to Settings)\n2. Log in again with correct version\n   Common versions: 22.200.001, 23.200.001, 24.200.001", 
                    "OK");
                _isScanning = true;
            });
        }
        catch (Refit.ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Handle 401 Unauthorized - token expired or invalid
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Clear the cached API client
                _authService.ClearApiCache();
                
                // Clear stored credentials
                _settingsService.Clear();
                
                // Prompt user to login again
                var shouldLogin = await DisplayAlert(
                    "Session Expired", 
                    "Your session has expired. Please login again to continue.", 
                    "Login", 
                    "Cancel");
                
                if (shouldLogin)
                {
                    var loginPage = _serviceProvider.GetService<LoginPage>();
                    if (loginPage != null)
                    {
                        await Navigation.PushModalAsync(new NavigationPage(loginPage));
                    }
                }
                
                _isScanning = true;
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Check if it's an HTTP error with 401 status
                string errorMessage = ex.Message;
                string errorTitle = "Error";
                
                if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
                {
                    errorTitle = "Authentication Error";
                    errorMessage = "Authentication failed. Please login again.";
                    _authService.ClearApiCache();
                    _settingsService.Clear();
                }
                else if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                {
                    errorTitle = "Item Not Found";
                    errorMessage = $"Item '{itemIdentifier}' was not found.\n\nPlease verify:\n• The item exists in Acumatica\n• The barcode/ID is correct\n• The API endpoint is configured properly";
                }
                else if (ex.Message.Contains("500") || ex.Message.Contains("Internal Server Error"))
                {
                    errorTitle = "Server Error";
                    errorMessage = "Acumatica server returned an error. Please try again later.";
                }
                else if (ex.Message.Contains("timeout") || ex.Message.Contains("Timeout"))
                {
                    errorTitle = "Connection Timeout";
                    errorMessage = "The request took too long. Please check your connection and try again.";
                }
                else
                {
                    errorMessage = $"Failed to query item: {ex.Message}";
                }
                
                await DisplayAlert(errorTitle, errorMessage, "OK");
                _isScanning = true;
            });
        }
        finally
        {
            // Ensure UI updates happen on the main thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LoadingSpinner.IsVisible = false;
                LoadingSpinner.IsRunning = false;
            });
        }
    }
}
