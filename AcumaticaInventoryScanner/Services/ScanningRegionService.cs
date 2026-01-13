/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This service provides a way to communicate scanning rectangle bounds
 * from the MAUI layer to the Android platform-specific handler.
 */

using Microsoft.Maui.Graphics;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public static class ScanningRegionService
{
    private static Rect? _scanningRectBounds;
    
    public static Rect? ScanningRectBounds
    {
        get => _scanningRectBounds;
        set
        {
            _scanningRectBounds = value;
            UpdateAndroidHandler();
        }
    }
    
    private static void UpdateAndroidHandler()
    {
#if ANDROID
        try
        {
            if (_scanningRectBounds.HasValue)
            {
                var bounds = _scanningRectBounds.Value;
                var androidRect = new Android.Graphics.Rect(
                    (int)bounds.X,
                    (int)bounds.Y,
                    (int)(bounds.X + bounds.Width),
                    (int)(bounds.Y + bounds.Height)
                );
                
                Platforms.Android.CroppedCameraBarcodeReaderViewHandler.ScanningRectBounds = androidRect;
                System.Diagnostics.Debug.WriteLine($"ScanningRegionService: Updated Android handler with bounds ({bounds.X}, {bounds.Y}, {bounds.Width}, {bounds.Height})");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ScanningRegionService: Error updating Android handler: {ex.Message}");
        }
#endif
    }
}
