/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file contains Android-specific handler for camera barcode reader view
 * with frame cropping to restrict scanning to a specific region of interest (ROI).
 */

using Android.Graphics;
using AndroidX.Camera.Core;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ZXing.Net.Maui.Controls;
using System.Reflection;
using View = Android.Views.View;
using IImageProxy = AndroidX.Camera.Core.IImageProxy;
using AndroidRect = Android.Graphics.Rect;
using Microsoft.Maui.ApplicationModel;

namespace AcuPower.AcumaticaInventoryScanner.Platforms.Android;

/// <summary>
/// Custom handler that wraps the ZXing handler and intercepts camera frames
/// to crop them to the scanning rectangle before processing.
/// </summary>
public class CroppedCameraBarcodeReaderViewHandler : IViewHandler
{
    private IViewHandler? _baseHandler;
    private AndroidRect? _cropRect;
    private int _cameraWidth;
    private int _cameraHeight;
    private int _screenWidth;
    private int _screenHeight;
    private IMauiContext? _mauiContext;
    private Microsoft.Maui.IView? _virtualView;
    
    // Store the scanning rectangle bounds (in screen coordinates)
    public static AndroidRect? ScanningRectBounds { get; set; }
    
    public Microsoft.Maui.IView? VirtualView => _virtualView ?? _baseHandler?.VirtualView;
    Microsoft.Maui.IElement? IElementHandler.VirtualView => _baseHandler is IElementHandler elementHandler ? elementHandler.VirtualView : null;
    
    public object? PlatformView
    {
        get
        {
            // Critical: Must return the actual Android View from base handler
            // MAUI will try to convert this to View, so it must be a View or null
            if (_baseHandler?.PlatformView is View view)
            {
                return view;
            }
            // If base handler not ready, return null (MAUI will handle it)
            return null;
        }
    }
    
    public IMauiContext? MauiContext
    {
        get => _mauiContext ?? _baseHandler?.MauiContext;
    }
    
    public object? ContainerView => _baseHandler is IViewHandler viewHandler ? viewHandler.ContainerView : null;
    
    public bool HasContainer
    {
        get => _baseHandler is IViewHandler viewHandler && viewHandler.HasContainer;
        set
        {
            if (_baseHandler is IViewHandler viewHandler)
            {
                viewHandler.HasContainer = value;
            }
        }
    }
    
    public Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        if (_baseHandler is IViewHandler viewHandler)
        {
            return viewHandler.GetDesiredSize(widthConstraint, heightConstraint);
        }
        return Microsoft.Maui.Graphics.Size.Zero;
    }
    
    public void PlatformArrange(Microsoft.Maui.Graphics.Rect frame)
    {
        if (_baseHandler is IViewHandler viewHandler)
        {
            viewHandler.PlatformArrange(frame);
        }
    }
    
    public void SetVirtualView(Microsoft.Maui.IView view)
    {
        _virtualView = view;
        EnsureBaseHandler();
    }
    
    public void SetMauiContext(IMauiContext mauiContext)
    {
        _mauiContext = mauiContext;
        EnsureBaseHandler();
    }
    
    private void EnsureBaseHandler()
    {
        // Only create base handler if we have both VirtualView and MauiContext
        if (_baseHandler != null || _virtualView == null || _mauiContext == null)
        {
            return;
        }
        
        try
        {
            // Find and create the base ZXing handler using reflection
            var handlerType = typeof(CameraBarcodeReaderView).Assembly
                .GetTypes()
                .FirstOrDefault(t => t.Name == "CameraBarcodeReaderViewHandler" && 
                                     typeof(IViewHandler).IsAssignableFrom(t));
            
            if (handlerType != null)
            {
                _baseHandler = (IViewHandler)Activator.CreateInstance(handlerType)!;
                
                // Set MauiContext first - this is critical
                _baseHandler.SetMauiContext(_mauiContext);
                
                // Then set virtual view
                _baseHandler.SetVirtualView(_virtualView);
                
                System.Diagnostics.Debug.WriteLine("CroppedCameraBarcodeReaderViewHandler: Base handler created successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CroppedCameraBarcodeReaderViewHandler: Could not find base handler type");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CroppedCameraBarcodeReaderViewHandler: Error creating base handler: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    public void SetVirtualView(Microsoft.Maui.IElement element)
    {
        if (_baseHandler is IElementHandler elementHandler)
        {
            elementHandler.SetVirtualView(element);
        }
    }
    
    public void UpdateValue(string property)
    {
        _baseHandler?.UpdateValue(property);
    }
    
    public void Invoke(string command, object? args = null)
    {
        _baseHandler?.Invoke(command, args);
    }
    
    public void DisconnectHandler()
    {
        _baseHandler?.DisconnectHandler();
    }
    
    public void ConnectHandler(View platformView)
    {
        // Ensure base handler exists
        EnsureBaseHandler();
        
        if (_baseHandler != null)
        {
            try
            {
                // Use reflection to call ConnectHandler on base handler
                var connectMethod = _baseHandler.GetType().GetMethod("ConnectHandler", 
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(View) }, null);
                
                if (connectMethod != null)
                {
                    connectMethod.Invoke(_baseHandler, new object[] { platformView });
                    System.Diagnostics.Debug.WriteLine("CroppedCameraBarcodeReaderViewHandler: Connected to platform view");
                    
                    // Get screen dimensions for coordinate mapping
                    var context = platformView.Context;
                    if (context != null)
                    {
                        var displayMetrics = context.Resources?.DisplayMetrics;
                        if (displayMetrics != null)
                        {
                            _screenWidth = displayMetrics.WidthPixels;
                            _screenHeight = displayMetrics.HeightPixels;
                            System.Diagnostics.Debug.WriteLine($"Screen dimensions: {_screenWidth}x{_screenHeight}");
                        }
                    }
                    
                    UpdateCropRegion();
                    
                    // Delay analyzer interception to ensure everything is set up
                    System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            InterceptImageAnalyzer();
                        });
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CroppedCameraBarcodeReaderViewHandler: Could not find ConnectHandler method");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CroppedCameraBarcodeReaderViewHandler: Error in ConnectHandler: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
    
    private void InterceptImageAnalyzer()
    {
        // Try to access and replace the ImageAnalysis analyzer
        // This requires accessing internal fields/properties of the base handler
        try
        {
            if (_baseHandler?.PlatformView is View platformView)
            {
                // The ZXing handler likely has an ImageAnalysis instance
                // We need to find it and replace its analyzer with our cropped version
                var imageAnalysisField = _baseHandler.GetType()
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(f => f.FieldType == typeof(ImageAnalysis) || 
                                         f.FieldType.Name.Contains("ImageAnalysis"));
                
                if (imageAnalysisField != null)
                {
                    var imageAnalysis = imageAnalysisField.GetValue(_baseHandler) as ImageAnalysis;
                    if (imageAnalysis != null)
                    {
                        // Create a custom analyzer that crops frames
                        var croppedAnalyzer = new CroppedFrameAnalyzer(this);
                        // Use the executor from the camera library
                        var executor = Java.Util.Concurrent.Executors.NewSingleThreadExecutor();
                        if (executor != null)
                        {
                            imageAnalysis.SetAnalyzer(executor, croppedAnalyzer);
                        }
                        System.Diagnostics.Debug.WriteLine("CroppedCameraBarcodeReaderViewHandler: Custom analyzer installed");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CroppedCameraBarcodeReaderViewHandler: Error intercepting analyzer: {ex.Message}");
            // Continue without cropping - fallback to default behavior
        }
    }
    
    public void UpdateCropRegion()
    {
        if (ScanningRectBounds != null && _screenWidth > 0 && _screenHeight > 0)
        {
            var bounds = ScanningRectBounds;
            
            // Store screen bounds for coordinate conversion
            // The crop rect will be converted to camera coordinates when frames arrive
            _cropRect = new AndroidRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
            
            System.Diagnostics.Debug.WriteLine($"CroppedCameraBarcodeReaderViewHandler: Crop region (screen) set to ({bounds.Left}, {bounds.Top}, {bounds.Right}, {bounds.Bottom})");
        }
    }
    
    /// <summary>
    /// Converts screen coordinates to camera frame coordinates
    /// </summary>
    private AndroidRect ConvertToCameraCoordinates(AndroidRect screenRect, int cameraWidth, int cameraHeight)
    {
        if (_screenWidth <= 0 || _screenHeight <= 0)
            return screenRect;
        
        // Calculate scaling factors
        float scaleX = (float)cameraWidth / _screenWidth;
        float scaleY = (float)cameraHeight / _screenHeight;
        
        // Convert coordinates
        int left = (int)(screenRect.Left * scaleX);
        int top = (int)(screenRect.Top * scaleY);
        int right = (int)(screenRect.Right * scaleX);
        int bottom = (int)(screenRect.Bottom * scaleY);
        
        // Ensure within bounds
        left = Math.Max(0, Math.Min(left, cameraWidth));
        top = Math.Max(0, Math.Min(top, cameraHeight));
        right = Math.Max(left, Math.Min(right, cameraWidth));
        bottom = Math.Max(top, Math.Min(bottom, cameraHeight));
        
        return new AndroidRect(left, top, right, bottom);
    }
    
    /// <summary>
    /// Crops a YUV420_888 image to the specified rectangle
    /// </summary>
    public IImageProxy? CropImage(IImageProxy image)
    {
        if (_cropRect == null || _cropRect.IsEmpty)
        {
            return image; // No cropping needed
        }
        
        try
        {
            _cameraWidth = image.Width;
            _cameraHeight = image.Height;
            
            // Convert screen coordinates to camera coordinates
            var cameraCropRect = ConvertToCameraCoordinates(_cropRect, _cameraWidth, _cameraHeight);
            
            // Use ImageProxy's cropRect property if available (CameraX feature)
            // Note: ImageProxy might support direct cropping via setCropRect
            var cropRectMethod = image.GetType().GetMethod("setCropRect", new[] { typeof(AndroidRect) });
            if (cropRectMethod != null)
            {
                cropRectMethod.Invoke(image, new object[] { cameraCropRect });
                System.Diagnostics.Debug.WriteLine($"Cropped image to ({cameraCropRect.Left}, {cameraCropRect.Top}, {cameraCropRect.Right}, {cameraCropRect.Bottom})");
                return image;
            }
            
            // If direct cropping not available, we'd need to extract and crop the buffer
            // This is more complex and requires YUV format handling
            System.Diagnostics.Debug.WriteLine("Direct cropRect not available, using full frame");
            return image;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cropping image: {ex.Message}");
            return image;
        }
    }
    
    /// <summary>
    /// Custom ImageAnalysis.Analyzer that crops frames before processing
    /// </summary>
    private class CroppedFrameAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        private readonly CroppedCameraBarcodeReaderViewHandler _handler;
        private readonly object? _originalAnalyzer;
        
        public CroppedFrameAnalyzer(CroppedCameraBarcodeReaderViewHandler handler)
        {
            _handler = handler;
            
            // Try to get the original analyzer from the base handler
            try
            {
                var baseHandler = handler._baseHandler;
                if (baseHandler != null)
                {
                    // Look for FrameAnalyzer or similar
                    var analyzerField = baseHandler.GetType()
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(f => typeof(ImageAnalysis.IAnalyzer).IsAssignableFrom(f.FieldType));
                    
                    if (analyzerField != null)
                    {
                        _originalAnalyzer = analyzerField.GetValue(baseHandler);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting original analyzer: {ex.Message}");
            }
        }
        
        public void Analyze(IImageProxy image)
        {
            try
            {
                // Crop the image to the scanning rectangle
                var croppedImage = _handler.CropImage(image);
                
                // If we have the original analyzer, call it with the cropped image
                if (_originalAnalyzer is ImageAnalysis.IAnalyzer original)
                {
                    original.Analyze(croppedImage ?? image);
                }
                else
                {
                    // Fallback: try to find and call the ZXing analyzer
                    // This might require additional reflection to access ZXing's internal analyzer
                    System.Diagnostics.Debug.WriteLine("Original analyzer not found, frame may not be processed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CroppedFrameAnalyzer.Analyze: {ex.Message}");
            }
            finally
            {
                image?.Close();
            }
        }
    }
}
