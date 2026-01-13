/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Custom Android view that draws a semi-transparent overlay with a transparent cutout
 * for the scanning rectangle. This properly handles transparency over camera SurfaceView.
 */

using Android.Content;
using Android.Graphics;

namespace AcuPower.AcumaticaInventoryScanner.Platforms.Android;

/// <summary>
/// Custom Android view that draws a semi-transparent overlay with a transparent rectangular cutout.
/// This is necessary because standard MAUI BoxView transparency doesn't work properly over camera SurfaceView.
/// </summary>
public class ScannerOverlayView : global::Android.Views.View
{
    private readonly global::Android.Graphics.Paint _overlayPaint;
    private readonly global::Android.Graphics.Paint _clearPaint;
    private global::Android.Graphics.RectF _scanningRect = new global::Android.Graphics.RectF();
    private int _overlayColor = global::Android.Graphics.Color.Argb(128, 0, 0, 0); // 50% black

    public ScannerOverlayView(Context? context) : base(context)
    {
        // Paint for the semi-transparent overlay
        _overlayPaint = new global::Android.Graphics.Paint
        {
            AntiAlias = true,
            Color = new global::Android.Graphics.Color(_overlayColor)
        };

        // Paint to clear (cut out) the scanning rectangle
        _clearPaint = new global::Android.Graphics.Paint
        {
            AntiAlias = true
        };
        _clearPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));

        // Required for PorterDuff.Mode.Clear to work
        SetLayerType(global::Android.Views.LayerType.Hardware, null);
    }

    /// <summary>
    /// Sets the scanning rectangle bounds in pixel coordinates.
    /// </summary>
    public void SetScanningRect(float left, float top, float right, float bottom)
    {
        _scanningRect = new global::Android.Graphics.RectF(left, top, right, bottom);
        Invalidate();
    }

    /// <summary>
    /// Sets the overlay color with alpha for transparency.
    /// </summary>
    public void SetOverlayColor(int alpha, int red, int green, int blue)
    {
        _overlayColor = global::Android.Graphics.Color.Argb(alpha, red, green, blue);
        _overlayPaint.Color = new global::Android.Graphics.Color(_overlayColor);
        Invalidate();
    }

    protected override void OnDraw(Canvas? canvas)
    {
        base.OnDraw(canvas);

        if (canvas == null) return;

        // Save canvas state
        int saveCount = canvas.SaveLayer(0, 0, Width, Height, null);

        // Draw the semi-transparent overlay covering the entire view
        canvas.DrawRect(0, 0, Width, Height, _overlayPaint);

        // Cut out the scanning rectangle (make it fully transparent)
        if (_scanningRect.Width() > 0 && _scanningRect.Height() > 0)
        {
            canvas.DrawRect(_scanningRect, _clearPaint);
        }

        // Restore canvas state
        canvas.RestoreToCount(saveCount);
    }
}
