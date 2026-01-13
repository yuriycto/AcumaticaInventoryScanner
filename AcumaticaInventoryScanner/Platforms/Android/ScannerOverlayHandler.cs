/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Android handler for ScannerOverlay control that maps to native ScannerOverlayView.
 */

using AcuPower.AcumaticaInventoryScanner.Controls;
using Microsoft.Maui.Handlers;

namespace AcuPower.AcumaticaInventoryScanner.Platforms.Android;

/// <summary>
/// Android handler that maps the cross-platform ScannerOverlay control to the native ScannerOverlayView.
/// </summary>
public class ScannerOverlayHandler : ViewHandler<ScannerOverlay, global::Android.Views.View>
{
    public static IPropertyMapper<ScannerOverlay, ScannerOverlayHandler> PropertyMapper =
        new PropertyMapper<ScannerOverlay, ScannerOverlayHandler>(ViewMapper)
        {
            [nameof(ScannerOverlay.ScanRectX)] = MapScanRect,
            [nameof(ScannerOverlay.ScanRectY)] = MapScanRect,
            [nameof(ScannerOverlay.ScanRectWidth)] = MapScanRect,
            [nameof(ScannerOverlay.ScanRectHeight)] = MapScanRect,
            [nameof(ScannerOverlay.OverlayOpacity)] = MapOverlayOpacity,
        };

    public ScannerOverlayHandler() : base(PropertyMapper)
    {
    }

    protected override global::Android.Views.View CreatePlatformView()
    {
        return new ScannerOverlayView(Context);
    }

    protected override void ConnectHandler(global::Android.Views.View platformView)
    {
        base.ConnectHandler(platformView);
        
        // Subscribe to events
        if (VirtualView != null)
        {
            VirtualView.ScanRectChanged += OnScanRectChanged;
            VirtualView.OverlayOpacityValueChanged += OnOverlayOpacityChanged;
            
            // Initial update
            UpdateScanRect();
            UpdateOverlayOpacity();
        }
    }

    protected override void DisconnectHandler(global::Android.Views.View platformView)
    {
        if (VirtualView != null)
        {
            VirtualView.ScanRectChanged -= OnScanRectChanged;
            VirtualView.OverlayOpacityValueChanged -= OnOverlayOpacityChanged;
        }
        
        base.DisconnectHandler(platformView);
    }

    private void OnScanRectChanged(object? sender, Microsoft.Maui.Graphics.Rect rect)
    {
        UpdateScanRect();
    }

    private void OnOverlayOpacityChanged(object? sender, double opacity)
    {
        UpdateOverlayOpacity();
    }

    private ScannerOverlayView? OverlayView => PlatformView as ScannerOverlayView;

    private void UpdateScanRect()
    {
        if (OverlayView == null || VirtualView == null) return;

        var density = Context.Resources?.DisplayMetrics?.Density ?? 1f;
        
        float left = (float)(VirtualView.ScanRectX * density);
        float top = (float)(VirtualView.ScanRectY * density);
        float right = left + (float)(VirtualView.ScanRectWidth * density);
        float bottom = top + (float)(VirtualView.ScanRectHeight * density);

        OverlayView.SetScanningRect(left, top, right, bottom);
    }

    private void UpdateOverlayOpacity()
    {
        if (OverlayView == null || VirtualView == null) return;

        int alpha = (int)(VirtualView.OverlayOpacity * 255);
        alpha = Math.Clamp(alpha, 0, 255);
        
        OverlayView.SetOverlayColor(alpha, 0, 0, 0);
    }

    private static void MapScanRect(ScannerOverlayHandler handler, ScannerOverlay view)
    {
        handler.UpdateScanRect();
    }

    private static void MapOverlayOpacity(ScannerOverlayHandler handler, ScannerOverlay view)
    {
        handler.UpdateOverlayOpacity();
    }
}
