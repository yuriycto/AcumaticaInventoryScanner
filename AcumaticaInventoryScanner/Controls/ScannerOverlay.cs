/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Cross-platform scanner overlay control that shows semi-transparent dark area
 * with a transparent cutout for the scanning rectangle.
 */

using Microsoft.Maui.Graphics;

namespace AcuPower.AcumaticaInventoryScanner.Controls;

/// <summary>
/// A scanner overlay control that displays a semi-transparent dark overlay with a transparent
/// rectangular cutout for the scanning area. Works properly over camera views on Android.
/// </summary>
public class ScannerOverlay : View
{
    public static readonly BindableProperty ScanRectXProperty =
        BindableProperty.Create(nameof(ScanRectX), typeof(double), typeof(ScannerOverlay), 0.0, propertyChanged: OnScanRectChanged);

    public static readonly BindableProperty ScanRectYProperty =
        BindableProperty.Create(nameof(ScanRectY), typeof(double), typeof(ScannerOverlay), 0.0, propertyChanged: OnScanRectChanged);

    public static readonly BindableProperty ScanRectWidthProperty =
        BindableProperty.Create(nameof(ScanRectWidth), typeof(double), typeof(ScannerOverlay), 250.0, propertyChanged: OnScanRectChanged);

    public static readonly BindableProperty ScanRectHeightProperty =
        BindableProperty.Create(nameof(ScanRectHeight), typeof(double), typeof(ScannerOverlay), 150.0, propertyChanged: OnScanRectChanged);

    public static readonly BindableProperty OverlayOpacityProperty =
        BindableProperty.Create(nameof(OverlayOpacity), typeof(double), typeof(ScannerOverlay), 0.5, propertyChanged: OnOverlayOpacityChanged);

    public double ScanRectX
    {
        get => (double)GetValue(ScanRectXProperty);
        set => SetValue(ScanRectXProperty, value);
    }

    public double ScanRectY
    {
        get => (double)GetValue(ScanRectYProperty);
        set => SetValue(ScanRectYProperty, value);
    }

    public double ScanRectWidth
    {
        get => (double)GetValue(ScanRectWidthProperty);
        set => SetValue(ScanRectWidthProperty, value);
    }

    public double ScanRectHeight
    {
        get => (double)GetValue(ScanRectHeightProperty);
        set => SetValue(ScanRectHeightProperty, value);
    }

    /// <summary>
    /// The opacity of the dark overlay (0.0 to 1.0). Default is 0.5 (50% transparent).
    /// </summary>
    public double OverlayOpacity
    {
        get => (double)GetValue(OverlayOpacityProperty);
        set => SetValue(OverlayOpacityProperty, value);
    }

    /// <summary>
    /// Event raised when the scanning rectangle changes.
    /// </summary>
    public event EventHandler<Rect>? ScanRectChanged;

    /// <summary>
    /// Event raised when the overlay opacity changes.
    /// </summary>
    public event EventHandler<double>? OverlayOpacityValueChanged;

    private static void OnScanRectChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ScannerOverlay overlay)
        {
            var rect = new Rect(overlay.ScanRectX, overlay.ScanRectY, overlay.ScanRectWidth, overlay.ScanRectHeight);
            overlay.ScanRectChanged?.Invoke(overlay, rect);
        }
    }

    private static void OnOverlayOpacityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ScannerOverlay overlay && newValue is double opacity)
        {
            overlay.OverlayOpacityValueChanged?.Invoke(overlay, opacity);
        }
    }

    /// <summary>
    /// Sets the scanning rectangle to be centered at the specified position with the specified size.
    /// </summary>
    public void SetScanRectCentered(double centerX, double centerY, double width, double height)
    {
        ScanRectX = centerX - (width / 2);
        ScanRectY = centerY - (height / 2);
        ScanRectWidth = width;
        ScanRectHeight = height;
    }
}
