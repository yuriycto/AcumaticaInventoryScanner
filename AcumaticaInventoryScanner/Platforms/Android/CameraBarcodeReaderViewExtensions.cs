/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file contains extensions to hook into the ZXing camera handler
 * and apply frame cropping for ROI (Region of Interest) scanning.
 */

using Android.Graphics;
using AndroidX.Camera.Core;
using Microsoft.Maui.Handlers;
using System.Reflection;
using View = Android.Views.View;
using IImageProxy = AndroidX.Camera.Core.IImageProxy;
using AndroidRect = Android.Graphics.Rect;
using Microsoft.Maui.ApplicationModel;
using ZXing.Net.Maui.Controls;
using AcuPower.AcumaticaInventoryScanner.Services;

namespace AcuPower.AcumaticaInventoryScanner.Platforms.Android;

/// <summary>
/// Extension methods to hook into the ZXing handler and apply frame cropping for ROI scanning
/// </summary>
public static class CameraBarcodeReaderViewExtensions
{
    private static readonly Dictionary<IViewHandler, CroppedFrameAnalyzerWrapper> _activeAnalyzers = new();
    private static readonly HashSet<IViewHandler> _retryAttempted = new();
    
    /// <summary>
    /// Hooks into an existing ZXing handler to apply frame cropping
    /// </summary>
    public static void ApplyFrameCropping(this IViewHandler handler)
    {
        System.Diagnostics.Debug.WriteLine("=== ApplyFrameCropping START ===");
        
        if (handler == null || handler.PlatformView is not View platformView)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Handler or PlatformView is null");
            return;
        }
        
        // Check if we already have a wrapper for this handler
        if (_activeAnalyzers.ContainsKey(handler))
        {
            System.Diagnostics.Debug.WriteLine("Wrapper already exists for this handler, skipping");
            return;
        }
        
        try
        {
            // Find the ImageAnalysis in the cameraManager
            ImageAnalysis? imageAnalysis = FindImageAnalysis(handler);
            
            if (imageAnalysis == null)
            {
                // ImageAnalysis not ready yet, retry later
                if (!_retryAttempted.Contains(handler))
                {
                    _retryAttempted.Add(handler);
                    System.Diagnostics.Debug.WriteLine("ImageAnalysis not found, will retry after delay...");
                    Task.Delay(1500).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            System.Diagnostics.Debug.WriteLine("Retrying ApplyFrameCropping after delay...");
                            ApplyFrameCropping(handler);
                        });
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Retry already attempted, giving up.");
                }
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"ImageAnalysis found: {imageAnalysis.GetType().FullName}");
            
            // Get the VirtualView (CameraBarcodeReaderView) to trigger events
            object? virtualView = FindVirtualView(handler);
            
            // Create our wrapper with direct ZXing decoding
            System.Diagnostics.Debug.WriteLine("Creating CroppedFrameAnalyzerWrapper with direct ZXing decoding...");
            var wrapper = new CroppedFrameAnalyzerWrapper(handler, virtualView);
            _activeAnalyzers[handler] = wrapper;
            
            // Set the wrapper as the analyzer
            SetAnalyzerOnImageAnalysis(imageAnalysis, wrapper);
            
            System.Diagnostics.Debug.WriteLine("=== ApplyFrameCropping SUCCESS ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in ApplyFrameCropping: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    private static ImageAnalysis? FindImageAnalysis(IViewHandler handler)
    {
        var allFields = handler.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        
        // Try to find in cameraManager first (most common location)
        var cameraManagerField = allFields.FirstOrDefault(f => 
            f.Name.Contains("cameraManager", StringComparison.OrdinalIgnoreCase));
        
        if (cameraManagerField != null)
        {
            var cameraManager = cameraManagerField.GetValue(handler);
            if (cameraManager != null)
            {
                var cameraManagerFields = cameraManager.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                var imageAnalysisField = cameraManagerFields.FirstOrDefault(f => 
                    f.FieldType == typeof(ImageAnalysis) || 
                    f.FieldType.Name.Contains("ImageAnalysis"));
                
                if (imageAnalysisField != null)
                {
                    var imageAnalysis = imageAnalysisField.GetValue(cameraManager) as ImageAnalysis;
                    if (imageAnalysis != null)
                    {
                        return imageAnalysis;
                    }
                    System.Diagnostics.Debug.WriteLine("ImageAnalysis field found but value is null");
                }
            }
        }
        
        // Try directly in handler
        var directField = allFields.FirstOrDefault(f => 
            f.FieldType == typeof(ImageAnalysis) || 
            f.FieldType.Name.Contains("ImageAnalysis"));
        
        if (directField != null)
        {
            return directField.GetValue(handler) as ImageAnalysis;
        }
        
        return null;
    }
    
    private static object? FindVirtualView(IViewHandler handler)
    {
        try
        {
            // Find the VirtualView property that returns ICameraBarcodeReaderView
            var props = handler.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var virtualViewProp = props.FirstOrDefault(p => 
                p.Name == "VirtualView" && 
                p.PropertyType.Name.Contains("ICameraBarcodeReaderView"));
            
            if (virtualViewProp != null)
            {
                return virtualViewProp.GetValue(handler);
            }
            
            // Fallback: try any VirtualView property
            virtualViewProp = props.FirstOrDefault(p => p.Name == "VirtualView");
            if (virtualViewProp != null)
            {
                return virtualViewProp.GetValue(handler);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error finding VirtualView: {ex.Message}");
        }
        
        return null;
    }
    
    private static void SetAnalyzerOnImageAnalysis(ImageAnalysis imageAnalysis, ImageAnalysis.IAnalyzer wrapper)
    {
        // Try SetAnalyzer(Executor, IAnalyzer) - most common in CameraX
        var setAnalyzerWithExecutor = imageAnalysis.GetType()
            .GetMethod("SetAnalyzer", new[] { typeof(Java.Util.Concurrent.IExecutor), typeof(ImageAnalysis.IAnalyzer) });
        
        if (setAnalyzerWithExecutor != null)
        {
            var executor = Java.Util.Concurrent.Executors.NewSingleThreadExecutor();
            if (executor != null)
            {
                setAnalyzerWithExecutor.Invoke(imageAnalysis, new object[] { executor, wrapper });
                System.Diagnostics.Debug.WriteLine("SUCCESS: Analyzer set via SetAnalyzer(Executor, IAnalyzer)");
                return;
            }
        }
        
        // Try SetAnalyzer(IAnalyzer)
        var setAnalyzerMethod = imageAnalysis.GetType()
            .GetMethod("SetAnalyzer", new[] { typeof(ImageAnalysis.IAnalyzer) });
        
        if (setAnalyzerMethod != null)
        {
            setAnalyzerMethod.Invoke(imageAnalysis, new object[] { wrapper });
            System.Diagnostics.Debug.WriteLine("SUCCESS: Analyzer set via SetAnalyzer(IAnalyzer)");
        }
    }
    
    /// <summary>
    /// Removes frame cropping from a handler
    /// </summary>
    public static void RemoveFrameCropping(this IViewHandler handler)
    {
        if (_activeAnalyzers.TryGetValue(handler, out var analyzer))
        {
            _activeAnalyzers.Remove(handler);
            analyzer?.Dispose();
            System.Diagnostics.Debug.WriteLine("Frame cropping removed");
        }
    }
    
    /// <summary>
    /// Wrapper that crops frames and decodes barcodes only in the ROI using ZXing MultiFormatReader
    /// </summary>
    private class CroppedFrameAnalyzerWrapper : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        private readonly IViewHandler _handler;
        private readonly object? _virtualView;
        private readonly ZXing.MultiFormatReader _multiFormatReader;
        private AndroidRect? _cropRect;
        private int _screenWidth;
        private int _screenHeight;
        private int _frameCount = 0;
        private const int LogEveryNFrames = 30; // Log more frequently for debugging
        private DateTime _lastDetectionTime = DateTime.MinValue;
        private const int DetectionCooldownMs = 1000; // Prevent rapid-fire detections
        private bool _hasLoggedFirstFrame = false;
        
        public CroppedFrameAnalyzerWrapper(IViewHandler handler, object? virtualView)
        {
            _handler = handler;
            _virtualView = virtualView;
            
            // Create ZXing MultiFormatReader with decoding hints
            _multiFormatReader = new ZXing.MultiFormatReader();
            
            var hints = new Dictionary<ZXing.DecodeHintType, object>
            {
                { ZXing.DecodeHintType.TRY_HARDER, true },
                { ZXing.DecodeHintType.POSSIBLE_FORMATS, new List<ZXing.BarcodeFormat>
                    {
                        ZXing.BarcodeFormat.QR_CODE,
                        ZXing.BarcodeFormat.CODE_128,
                        ZXing.BarcodeFormat.CODE_39,
                        ZXing.BarcodeFormat.EAN_13,
                        ZXing.BarcodeFormat.EAN_8,
                        ZXing.BarcodeFormat.UPC_A,
                        ZXing.BarcodeFormat.UPC_E,
                        ZXing.BarcodeFormat.DATA_MATRIX,
                        ZXing.BarcodeFormat.PDF_417,
                        ZXing.BarcodeFormat.AZTEC,
                        ZXing.BarcodeFormat.ITF,
                        ZXing.BarcodeFormat.CODABAR
                    }
                }
            };
            _multiFormatReader.Hints = hints;
            
            System.Diagnostics.Debug.WriteLine("ZXing MultiFormatReader created for direct decoding");
            
            // Get screen dimensions
            var context = Platform.CurrentActivity;
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
        }
        
        public void Analyze(IImageProxy image)
        {
            _frameCount++;
            bool shouldLog = (_frameCount % LogEveryNFrames == 1);
            
            try
            {
                // Update crop region
                UpdateCropRegion();
                
                int cameraWidth = image.Width;
                int cameraHeight = image.Height;
                int rotation = image.ImageInfo?.RotationDegrees ?? 90;
                
                if (!_hasLoggedFirstFrame)
                {
                    _hasLoggedFirstFrame = true;
                    System.Diagnostics.Debug.WriteLine($"First frame info: Camera={cameraWidth}x{cameraHeight}, Rotation={rotation}°, Screen={_screenWidth}x{_screenHeight}");
                    System.Diagnostics.Debug.WriteLine($"Screen crop rect (pixels): {_cropRect?.Left},{_cropRect?.Top},{_cropRect?.Right},{_cropRect?.Bottom}");
                }
                
                // Every 60 frames, try decoding the full image to verify decoder works
                bool tryFullFrame = (_frameCount % 60 == 30);
                
                if (tryFullFrame)
                {
                    TryDecodeFullFrame(image, rotation, shouldLog);
                }
                else
                {
                    if (_cropRect == null || _cropRect.IsEmpty)
                    {
                        if (shouldLog) System.Diagnostics.Debug.WriteLine("No crop rect set, skipping frame");
                        image.Close();
                        return;
                    }
                    
                    // Convert screen coordinates to camera coordinates (accounting for rotation)
                    var cameraCropRect = ConvertToCameraCoordinates(_cropRect, cameraWidth, cameraHeight, rotation);
                    
                    if (shouldLog)
                    {
                        System.Diagnostics.Debug.WriteLine($"Frame {_frameCount}: Camera={cameraWidth}x{cameraHeight}, Rotation={rotation}°, CameraCropRect=({cameraCropRect.Left},{cameraCropRect.Top},{cameraCropRect.Right},{cameraCropRect.Bottom})");
                    }
                    
                    // Extract luminance data and decode
                    DecodeBarcodesInRegion(image, cameraCropRect, rotation, shouldLog);
                }
            }
            catch (Exception ex)
            {
                if (shouldLog) System.Diagnostics.Debug.WriteLine($"Error in Analyze: {ex.Message}");
            }
            finally
            {
                try
                {
                    image.Close();
                }
                catch { }
            }
        }
        
        private void TryDecodeFullFrame(IImageProxy image, int rotation, bool shouldLog)
        {
            try
            {
                var planes = image.GetPlanes();
                if (planes == null || planes.Length == 0) return;
                
                var yPlane = planes[0];
                var yBuffer = yPlane.Buffer;
                if (yBuffer == null) return;
                
                int imageWidth = image.Width;
                int imageHeight = image.Height;
                int rowStride = yPlane.RowStride;
                
                yBuffer.Rewind();
                byte[] fullBuffer = new byte[yBuffer.Remaining()];
                yBuffer.Get(fullBuffer);
                
                // Extract just the luminance data (may have padding due to rowStride)
                byte[] luminanceData;
                if (rowStride == imageWidth)
                {
                    // No padding, use directly
                    luminanceData = fullBuffer;
                }
                else
                {
                    // Need to remove padding
                    luminanceData = new byte[imageWidth * imageHeight];
                    for (int y = 0; y < imageHeight; y++)
                    {
                        Array.Copy(fullBuffer, y * rowStride, luminanceData, y * imageWidth, imageWidth);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Trying FULL FRAME decode: {imageWidth}x{imageHeight}, rotation={rotation}°");
                
                // Try decoding with rotation
                byte[] rotatedData;
                int finalWidth, finalHeight;
                
                if (rotation == 90)
                {
                    rotatedData = RotateFullImage(luminanceData, imageWidth, imageHeight, rotation);
                    finalWidth = imageHeight;
                    finalHeight = imageWidth;
                }
                else if (rotation == 270)
                {
                    rotatedData = RotateFullImage(luminanceData, imageWidth, imageHeight, rotation);
                    finalWidth = imageHeight;
                    finalHeight = imageWidth;
                }
                else
                {
                    rotatedData = luminanceData;
                    finalWidth = imageWidth;
                    finalHeight = imageHeight;
                }
                
                // Try to decode
                DecodeWithZXing(rotatedData, finalWidth, finalHeight, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TryDecodeFullFrame: {ex.Message}");
            }
        }
        
        private byte[] RotateFullImage(byte[] data, int width, int height, int rotation)
        {
            int newWidth = height;
            int newHeight = width;
            byte[] rotated = new byte[newWidth * newHeight];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcPos = y * width + x;
                    int dstX, dstY;
                    
                    if (rotation == 90)
                    {
                        dstX = height - 1 - y;
                        dstY = x;
                    }
                    else // 270
                    {
                        dstX = y;
                        dstY = width - 1 - x;
                    }
                    
                    int dstPos = dstY * newWidth + dstX;
                    if (srcPos < data.Length && dstPos < rotated.Length)
                    {
                        rotated[dstPos] = data[srcPos];
                    }
                }
            }
            
            return rotated;
        }
        
        private void DecodeBarcodesInRegion(IImageProxy image, AndroidRect cropRect, int rotation, bool shouldLog)
        {
            try
            {
                // Get the image planes
                var planes = image.GetPlanes();
                if (planes == null || planes.Length == 0)
                {
                    if (shouldLog) System.Diagnostics.Debug.WriteLine("No image planes available");
                    return;
                }
                
                // Get Y plane (luminance) - sufficient for barcode detection
                var yPlane = planes[0];
                var yBuffer = yPlane.Buffer;
                
                if (yBuffer == null)
                {
                    if (shouldLog) System.Diagnostics.Debug.WriteLine("Y buffer is null");
                    return;
                }
                
                int rowStride = yPlane.RowStride;
                int pixelStride = yPlane.PixelStride;
                int imageWidth = image.Width;
                int imageHeight = image.Height;
                
                // Ensure crop rect is valid
                int cropLeft = Math.Max(0, Math.Min(cropRect.Left, imageWidth - 1));
                int cropTop = Math.Max(0, Math.Min(cropRect.Top, imageHeight - 1));
                int cropRight = Math.Max(cropLeft + 1, Math.Min(cropRect.Right, imageWidth));
                int cropBottom = Math.Max(cropTop + 1, Math.Min(cropRect.Bottom, imageHeight));
                
                int cropWidth = cropRight - cropLeft;
                int cropHeight = cropBottom - cropTop;
                
                if (cropWidth <= 10 || cropHeight <= 10)
                {
                    if (shouldLog) System.Diagnostics.Debug.WriteLine($"Crop region too small: {cropWidth}x{cropHeight}");
                    return;
                }
                
                // Extract full luminance buffer
                yBuffer.Rewind();
                byte[] fullBuffer = new byte[yBuffer.Remaining()];
                yBuffer.Get(fullBuffer);
                
                // For rotated images (common on Android), we need to handle the rotation
                // The camera typically outputs landscape (640x480) but phone is in portrait
                // After 90° rotation, width becomes height and vice versa
                
                byte[] croppedData;
                int finalWidth, finalHeight;
                
                if (rotation == 90 || rotation == 270)
                {
                    // We need to extract and rotate the data
                    // After 90° rotation: original (x, y) -> (y, width - 1 - x) for 90°
                    // Or we can just use the full image and let ZXing handle various orientations
                    
                    // For simplicity, extract the cropped region and rotate it
                    croppedData = ExtractAndRotate(fullBuffer, rowStride, pixelStride, 
                        imageWidth, imageHeight, cropLeft, cropTop, cropWidth, cropHeight, rotation);
                    
                    // After 90/270 rotation, width and height are swapped
                    finalWidth = cropHeight;
                    finalHeight = cropWidth;
                }
                else
                {
                    // No rotation needed, extract directly
                    croppedData = new byte[cropWidth * cropHeight];
                    
                    for (int y = 0; y < cropHeight; y++)
                    {
                        int srcY = cropTop + y;
                        if (srcY >= imageHeight) break;
                        
                        int srcRowStart = srcY * rowStride;
                        int dstRowStart = y * cropWidth;
                        
                        for (int x = 0; x < cropWidth; x++)
                        {
                            int srcX = cropLeft + x;
                            if (srcX >= imageWidth) break;
                            
                            int srcPos = srcRowStart + srcX * pixelStride;
                            if (srcPos < fullBuffer.Length)
                            {
                                croppedData[dstRowStart + x] = fullBuffer[srcPos];
                            }
                        }
                    }
                    
                    finalWidth = cropWidth;
                    finalHeight = cropHeight;
                }
                
                if (shouldLog)
                {
                    System.Diagnostics.Debug.WriteLine($"Decoding cropped region: {finalWidth}x{finalHeight} (rotation={rotation})");
                }
                
                // Decode using standard ZXing
                DecodeWithZXing(croppedData, finalWidth, finalHeight, shouldLog);
            }
            catch (Exception ex)
            {
                if (shouldLog) System.Diagnostics.Debug.WriteLine($"Error extracting image data: {ex.Message}");
            }
        }
        
        private byte[] ExtractAndRotate(byte[] fullBuffer, int rowStride, int pixelStride,
            int imageWidth, int imageHeight, int cropLeft, int cropTop, int cropWidth, int cropHeight, int rotation)
        {
            // After rotation: output dimensions are swapped
            int outputWidth = cropHeight;
            int outputHeight = cropWidth;
            byte[] rotatedData = new byte[outputWidth * outputHeight];
            
            for (int y = 0; y < cropHeight; y++)
            {
                int srcY = cropTop + y;
                if (srcY >= imageHeight) continue;
                
                int srcRowStart = srcY * rowStride;
                
                for (int x = 0; x < cropWidth; x++)
                {
                    int srcX = cropLeft + x;
                    if (srcX >= imageWidth) continue;
                    
                    int srcPos = srcRowStart + srcX * pixelStride;
                    if (srcPos >= fullBuffer.Length) continue;
                    
                    byte pixel = fullBuffer[srcPos];
                    
                    int dstX, dstY;
                    if (rotation == 90)
                    {
                        // 90° clockwise: (x, y) -> (height - 1 - y, x)
                        dstX = cropHeight - 1 - y;
                        dstY = x;
                    }
                    else // rotation == 270
                    {
                        // 270° clockwise (90° counter-clockwise): (x, y) -> (y, width - 1 - x)
                        dstX = y;
                        dstY = cropWidth - 1 - x;
                    }
                    
                    int dstPos = dstY * outputWidth + dstX;
                    if (dstPos >= 0 && dstPos < rotatedData.Length)
                    {
                        rotatedData[dstPos] = pixel;
                    }
                }
            }
            
            return rotatedData;
        }
        
        private void DecodeWithZXing(byte[] luminanceData, int width, int height, bool shouldLog)
        {
            try
            {
                // Check cooldown to prevent rapid detections
                if ((DateTime.Now - _lastDetectionTime).TotalMilliseconds < DetectionCooldownMs)
                {
                    return;
                }
                
                if (luminanceData == null || luminanceData.Length == 0)
                {
                    if (shouldLog) System.Diagnostics.Debug.WriteLine("Luminance data is empty");
                    return;
                }
                
                if (shouldLog)
                {
                    System.Diagnostics.Debug.WriteLine($"Attempting ZXing decode on {width}x{height} image ({luminanceData.Length} bytes)");
                }
                
                // Create luminance source from the cropped grayscale data
                var luminanceSource = new ZXing.PlanarYUVLuminanceSource(
                    luminanceData, 
                    width, 
                    height, 
                    0, 0, 
                    width, height, 
                    false);
                
                // Create binary bitmap for decoding
                var binarizer = new ZXing.Common.HybridBinarizer(luminanceSource);
                var binaryBitmap = new ZXing.BinaryBitmap(binarizer);
                
                // Decode using MultiFormatReader
                ZXing.Result? result = null;
                try
                {
                    _multiFormatReader.reset();
                    result = _multiFormatReader.decodeWithState(binaryBitmap);
                }
                catch (Exception decodeEx)
                {
                    // No barcode found or error during decode - this is normal, ignore
                    if (shouldLog) System.Diagnostics.Debug.WriteLine($"Decode attempt: {decodeEx.GetType().Name}");
                }
                
                if (result != null && !string.IsNullOrEmpty(result.Text))
                {
                    _lastDetectionTime = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine($"*** BARCODE DETECTED IN ROI: {result.Text} (Format: {result.BarcodeFormat}) ***");
                    
                    // Trigger the event on the main thread
                    TriggerBarcodesDetectedEvent(result);
                }
            }
            catch (Exception ex)
            {
                if (shouldLog) System.Diagnostics.Debug.WriteLine($"Error in DecodeWithZXing: {ex.Message}");
            }
        }
        
        private void TriggerBarcodesDetectedEvent(ZXing.Result result)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (_virtualView == null)
                    {
                        System.Diagnostics.Debug.WriteLine("No VirtualView to trigger event on");
                        return;
                    }
                    
                    // Find the BarcodesDetected event
                    var viewType = _virtualView.GetType();
                    
                    // Try to get the event's backing delegate field
                    var eventField = viewType.GetField("BarcodesDetected", 
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    
                    if (eventField == null)
                    {
                        // Try with event name pattern
                        var allFields = viewType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        eventField = allFields.FirstOrDefault(f => 
                            f.Name.Contains("BarcodesDetected") || 
                            f.Name.Contains("barcodesDetected"));
                    }
                    
                    // Create BarcodeResult and event args using ZXing.Net.Maui types
                    CreateAndInvokeEventWithMauiTypes(result);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error triggering BarcodesDetected: {ex.Message}");
                }
            });
        }
        
        private void CreateAndInvokeEventWithMauiTypes(ZXing.Result result)
        {
            try
            {
                // Find ZXing.Net.Maui.BarcodeResult type
                var mauiBarcodeResultType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                    .FirstOrDefault(t => t.FullName == "ZXing.Net.Maui.BarcodeResult");
                
                if (mauiBarcodeResultType == null)
                {
                    System.Diagnostics.Debug.WriteLine("Could not find ZXing.Net.Maui.BarcodeResult type");
                    return;
                }
                
                // Find ZXing.Net.Maui.BarcodeDetectionEventArgs type
                var eventArgsType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                    .FirstOrDefault(t => t.FullName == "ZXing.Net.Maui.BarcodeDetectionEventArgs");
                
                if (eventArgsType == null)
                {
                    System.Diagnostics.Debug.WriteLine("Could not find ZXing.Net.Maui.BarcodeDetectionEventArgs type");
                    return;
                }
                
                // Create BarcodeResult
                var barcodeResult = Activator.CreateInstance(mauiBarcodeResultType);
                
                // Set Value property
                var valueProp = mauiBarcodeResultType.GetProperty("Value");
                valueProp?.SetValue(barcodeResult, result.Text);
                
                // Set Format property (need to convert ZXing.BarcodeFormat to ZXing.Net.Maui.BarcodeFormat)
                var formatProp = mauiBarcodeResultType.GetProperty("Format");
                if (formatProp != null)
                {
                    var mauiFormatType = formatProp.PropertyType;
                    var formatName = result.BarcodeFormat.ToString();
                    
                    // Map common format names
                    if (formatName == "QR_CODE") formatName = "QrCode";
                    else if (formatName == "CODE_128") formatName = "Code128";
                    else if (formatName == "CODE_39") formatName = "Code39";
                    else if (formatName == "EAN_13") formatName = "Ean13";
                    else if (formatName == "EAN_8") formatName = "Ean8";
                    else if (formatName == "UPC_A") formatName = "UpcA";
                    else if (formatName == "UPC_E") formatName = "UpcE";
                    else if (formatName == "DATA_MATRIX") formatName = "DataMatrix";
                    else if (formatName == "PDF_417") formatName = "Pdf417";
                    else if (formatName == "AZTEC") formatName = "Aztec";
                    
                    try
                    {
                        if (Enum.TryParse(mauiFormatType, formatName, true, out var mauiFormat))
                        {
                            formatProp.SetValue(barcodeResult, mauiFormat);
                        }
                    }
                    catch { }
                }
                
                // Create BarcodeResult[] array (the constructor expects an array, not a List)
                var array = Array.CreateInstance(mauiBarcodeResultType, 1);
                array.SetValue(barcodeResult, 0);
                
                // Create BarcodeDetectionEventArgs
                var constructor = eventArgsType.GetConstructors().FirstOrDefault();
                object? eventArgs = null;
                
                if (constructor != null)
                {
                    var paramInfo = constructor.GetParameters();
                    System.Diagnostics.Debug.WriteLine($"BarcodeDetectionEventArgs constructor has {paramInfo.Length} params");
                    
                    if (paramInfo.Length == 1)
                    {
                        // Check if parameter is array type
                        var paramType = paramInfo[0].ParameterType;
                        System.Diagnostics.Debug.WriteLine($"Constructor param type: {paramType.FullName}");
                        
                        if (paramType.IsArray)
                        {
                            eventArgs = constructor.Invoke(new object[] { array });
                        }
                        else
                        {
                            // Try to convert array to the expected type (IEnumerable, IList, etc.)
                            eventArgs = constructor.Invoke(new object[] { array });
                        }
                    }
                    else if (paramInfo.Length == 0)
                    {
                        eventArgs = Activator.CreateInstance(eventArgsType);
                        var resultsProp = eventArgsType.GetProperty("Results");
                        if (resultsProp != null)
                        {
                            // Check if Results property expects array or IEnumerable
                            if (resultsProp.PropertyType.IsArray)
                            {
                                resultsProp.SetValue(eventArgs, array);
                            }
                            else
                            {
                                // Create List and set it
                                var listType = typeof(List<>).MakeGenericType(mauiBarcodeResultType);
                                var list = Activator.CreateInstance(listType);
                                var addMethod = listType.GetMethod("Add");
                                addMethod?.Invoke(list, new[] { barcodeResult });
                                resultsProp.SetValue(eventArgs, list);
                            }
                        }
                    }
                }
                
                if (eventArgs == null)
                {
                    System.Diagnostics.Debug.WriteLine("Could not create BarcodeDetectionEventArgs");
                    return;
                }
                
                // Find and invoke the event
                var viewType = _virtualView!.GetType();
                
                // Try to find the event handler field
                // Common patterns: _barcodesDetected, BarcodesDetectedHandler, etc.
                FieldInfo? eventField = null;
                foreach (var pattern in new[] { "BarcodesDetected", "_barcodesDetected", "barcodesDetected" })
                {
                    eventField = viewType.GetField(pattern, 
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (eventField != null) break;
                }
                
                if (eventField == null)
                {
                    // Try all fields and find one that's a delegate with matching signature
                    var allFields = viewType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in allFields)
                    {
                        if (typeof(Delegate).IsAssignableFrom(field.FieldType) && 
                            field.Name.ToLower().Contains("barcode"))
                        {
                            eventField = field;
                            break;
                        }
                    }
                }
                
                if (eventField != null)
                {
                    var handler = eventField.GetValue(_virtualView) as Delegate;
                    if (handler != null)
                    {
                        handler.DynamicInvoke(_virtualView, eventArgs);
                        System.Diagnostics.Debug.WriteLine("BarcodesDetected event invoked successfully!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Event handler delegate is null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Could not find BarcodesDetected event field");
                    
                    // Alternative: Try to invoke via event info
                    var eventInfo = viewType.GetEvent("BarcodesDetected");
                    if (eventInfo != null)
                    {
                        // Get the raise method if available
                        var raiseMethod = eventInfo.GetRaiseMethod(true);
                        if (raiseMethod != null)
                        {
                            raiseMethod.Invoke(_virtualView, new[] { _virtualView, eventArgs });
                            System.Diagnostics.Debug.WriteLine("BarcodesDetected event raised via RaiseMethod!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating/invoking MAUI event: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            }
        }
        
        private void UpdateCropRegion()
        {
            if (ScanningRegionService.ScanningRectBounds.HasValue)
            {
                var bounds = ScanningRegionService.ScanningRectBounds.Value;
                
                // Get density
                var context = Platform.CurrentActivity;
                float density = 1.0f;
                if (context != null)
                {
                    var displayMetrics = context.Resources?.DisplayMetrics;
                    if (displayMetrics != null)
                    {
                        density = displayMetrics.Density;
                    }
                }
                
                // Convert DIP to physical pixels
                int left = (int)(bounds.X * density);
                int top = (int)(bounds.Y * density);
                int right = (int)((bounds.X + bounds.Width) * density);
                int bottom = (int)((bounds.Y + bounds.Height) * density);
                
                _cropRect = new AndroidRect(left, top, right, bottom);
            }
        }
        
        private AndroidRect ConvertToCameraCoordinates(AndroidRect screenRect, int cameraWidth, int cameraHeight, int rotation)
        {
            if (_screenWidth <= 0 || _screenHeight <= 0)
            {
                return screenRect;
            }
            
            // Camera typically outputs in landscape (e.g., 640x480)
            // Screen is in portrait (e.g., 1080x2069)
            // With 90° rotation, camera's width maps to screen's height and vice versa
            
            int left, top, right, bottom;
            
            if (rotation == 90)
            {
                // 90° rotation: 
                // Screen X maps to Camera Y (inverted from bottom)
                // Screen Y maps to Camera X
                // Camera preview: screen portrait shows camera landscape rotated 90° clockwise
                
                float scaleX = (float)cameraHeight / _screenWidth;  // camera height -> screen width
                float scaleY = (float)cameraWidth / _screenHeight;  // camera width -> screen height
                
                // For 90° rotation:
                // Screen (x, y) -> Camera (y * scaleY, (screenWidth - x) * scaleX)
                // But the preview is fill/fit scaled, so we need to consider the aspect ratio
                
                // Simple mapping assuming fill mode
                left = (int)(screenRect.Top * scaleY);
                top = (int)(((_screenWidth - screenRect.Right)) * scaleX);
                right = (int)(screenRect.Bottom * scaleY);
                bottom = (int)(((_screenWidth - screenRect.Left)) * scaleX);
            }
            else if (rotation == 270)
            {
                float scaleX = (float)cameraHeight / _screenWidth;
                float scaleY = (float)cameraWidth / _screenHeight;
                
                left = (int)((_screenHeight - screenRect.Bottom) * scaleY);
                top = (int)(screenRect.Left * scaleX);
                right = (int)((_screenHeight - screenRect.Top) * scaleY);
                bottom = (int)(screenRect.Right * scaleX);
            }
            else
            {
                // No rotation
                float scaleX = (float)cameraWidth / _screenWidth;
                float scaleY = (float)cameraHeight / _screenHeight;
                
                left = (int)(screenRect.Left * scaleX);
                top = (int)(screenRect.Top * scaleY);
                right = (int)(screenRect.Right * scaleX);
                bottom = (int)(screenRect.Bottom * scaleY);
            }
            
            // Ensure left < right and top < bottom
            if (left > right) { int tmp = left; left = right; right = tmp; }
            if (top > bottom) { int tmp = top; top = bottom; bottom = tmp; }
            
            // Clamp to valid range
            left = Math.Max(0, Math.Min(left, cameraWidth - 1));
            top = Math.Max(0, Math.Min(top, cameraHeight - 1));
            right = Math.Max(left + 1, Math.Min(right, cameraWidth));
            bottom = Math.Max(top + 1, Math.Min(bottom, cameraHeight));
            
            return new AndroidRect(left, top, right, bottom);
        }
        
        public new void Dispose()
        {
            // Cleanup
        }
    }
}
