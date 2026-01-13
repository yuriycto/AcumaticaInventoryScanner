/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This application demonstrates barcode generation capabilities for testing
 * and demonstration purposes related to Acumatica ERP integration.
 */

using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ZXing;
using ZXing.Common;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using System.Windows.Media;

namespace AcuPower.BarcodeGenerator
{
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
        private ObservableCollection<BarcodeItem> _barcodeItems = new ObservableCollection<BarcodeItem>();

    public MainWindow()
    {
        InitializeComponent();
            InputTextBox.Focus();
            BarcodeContainer.ItemsSource = _barcodeItems;
            
            // Set up font resolver for PdfSharp - must be set before any XFont creation
            GlobalFontSettings.FontResolver = new StandardFontResolver();
        }

        public class BarcodeItem
        {
            public BitmapSource Image { get; set; }
            public string Label { get; set; }
            public string Code { get; set; }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateBarcode();
        }

        private void InputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Auto-generate when text changes (optional - can be removed if not desired)
            // GenerateBarcode();
        }

        private void BarcodeTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GenerateBarcode();
        }

        private void GenerateBarcode()
        {
            try
            {
                // Check if controls are initialized
                if (InputTextBox == null || BarcodeContainer == null || SaveButton == null || SaveAllButton == null)
                {
                    return; // Controls not ready yet
                }

                string inputText = InputTextBox.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(inputText))
                {
                    _barcodeItems.Clear();
                    SaveButton.IsEnabled = false;
                    SaveAllButton.IsEnabled = false;
                    return;
                }

                // Get selected barcode type
                string barcodeType = ((System.Windows.Controls.ComboBoxItem)BarcodeTypeComboBox.SelectedItem)?.Content?.ToString() ?? "Code 128";

                // Parse dimensions
                if (!int.TryParse(WidthTextBox.Text, out int width) || width <= 0)
                    width = 300;
                if (!int.TryParse(HeightTextBox.Text, out int height) || height <= 0)
                    height = 150;

                // Split input by semicolon to support multiple codes
                string[] codes = inputText.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(c => c.Trim())
                                          .Where(c => !string.IsNullOrEmpty(c))
                                          .ToArray();

                if (codes.Length == 0)
                {
                    _barcodeItems.Clear();
                    SaveButton.IsEnabled = false;
                    SaveAllButton.IsEnabled = false;
                    return;
                }

                // Clear existing barcodes
                _barcodeItems.Clear();

                // Get barcode format
                BarcodeFormat format = GetBarcodeFormat(barcodeType);
                
                // Generate barcode for each code
                foreach (string code in codes)
                {
                    try
                    {
                        // Create barcode writer using generic writer
                        BarcodeWriterGeneric writer = new BarcodeWriterGeneric
                        {
                            Format = format,
                            Options = new EncodingOptions
                            {
                                Height = height,
                                Width = width,
                                Margin = 2,
                                PureBarcode = false
                            }
                        };

                        // Generate barcode matrix - let the library handle validation
                        var barcodeMatrix = writer.Encode(code);
                        
                        if (barcodeMatrix == null)
                        {
                            _barcodeItems.Add(new BarcodeItem
                            {
                                Image = null,
                                Label = $"Error: Failed to encode '{code}' - Check format requirements for {barcodeType}",
                                Code = code
                            });
                            continue;
                        }
                        
                        // Convert to WPF BitmapSource using actual matrix dimensions
                        BitmapSource barcodeBitmap = ConvertToBitmapSource(barcodeMatrix);

                        // Add to collection
                        _barcodeItems.Add(new BarcodeItem
                        {
                            Image = barcodeBitmap,
                            Label = $"Type: {barcodeType} | Code: {code}",
                            Code = code
                        });
                    }
                    catch (Exception ex)
                    {
                        // Provide helpful error messages based on exception
                        string errorMsg = ex.Message;
                        if (errorMsg.Contains("digits") || errorMsg.Contains("numeric"))
                        {
                            errorMsg = $"{barcodeType} requires numeric-only codes with specific length. {ex.Message}";
                        }
                        
                        _barcodeItems.Add(new BarcodeItem
                        {
                            Image = null,
                            Label = $"Error encoding '{code}': {errorMsg}",
                            Code = code
                        });
                    }
                }

                // Enable save buttons if we have any successful barcodes
                bool hasValidBarcodes = _barcodeItems.Any(item => item.Image != null);
                SaveButton.IsEnabled = hasValidBarcodes;
                SaveAllButton.IsEnabled = hasValidBarcodes;
                SaveAsPdfButton.IsEnabled = hasValidBarcodes;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating barcodes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _barcodeItems.Clear();
                SaveButton.IsEnabled = false;
                SaveAllButton.IsEnabled = false;
            }
        }

        private BarcodeFormat GetBarcodeFormat(string barcodeType)
        {
            return barcodeType switch
            {
                "Code 128" => BarcodeFormat.CODE_128,
                "EAN-13" => BarcodeFormat.EAN_13,
                "EAN-8" => BarcodeFormat.EAN_8,
                "UPC-A" => BarcodeFormat.UPC_A,
                "UPC-E" => BarcodeFormat.UPC_E,
                "QR Code" => BarcodeFormat.QR_CODE,
                "Data Matrix" => BarcodeFormat.DATA_MATRIX,
                "PDF417" => BarcodeFormat.PDF_417,
                "ITF" => BarcodeFormat.ITF,
                "Codabar" => BarcodeFormat.CODABAR,
                _ => BarcodeFormat.CODE_128
            };
        }

        private string ValidateBarcodeFormat(string code, BarcodeFormat format, string formatName)
        {
            // Check if code contains only digits for numeric-only formats
            bool isNumericOnly = code.All(char.IsDigit);
            string digitsOnly = isNumericOnly ? code : new string(code.Where(char.IsDigit).ToArray());
            int digitCount = digitsOnly.Length;

            switch (format)
            {
                case BarcodeFormat.EAN_13:
                    if (!isNumericOnly)
                        return $"'{code}' - EAN-13 requires only digits (no letters), but contains non-numeric characters";
                    if (digitCount != 12 && digitCount != 13)
                        return $"'{code}' - EAN-13 requires 12 (without checksum) or 13 digits, but got {digitCount}";
                    break;
                    
                case BarcodeFormat.EAN_8:
                    if (!isNumericOnly)
                        return $"'{code}' - EAN-8 requires only digits (no letters), but contains non-numeric characters";
                    if (digitCount != 7 && digitCount != 8)
                        return $"'{code}' - EAN-8 requires 7 (without checksum) or 8 digits, but got {digitCount}";
                    break;
                    
                case BarcodeFormat.UPC_A:
                    if (!isNumericOnly)
                        return $"'{code}' - UPC-A requires only digits (no letters), but contains non-numeric characters";
                    if (digitCount != 11 && digitCount != 12)
                        return $"'{code}' - UPC-A requires 11 (without checksum) or 12 digits, but got {digitCount}";
                    break;
                    
                case BarcodeFormat.UPC_E:
                    if (!isNumericOnly)
                        return $"'{code}' - UPC-E requires only digits (no letters), but contains non-numeric characters";
                    if (digitCount != 7 && digitCount != 8)
                        return $"'{code}' - UPC-E requires 7 (without checksum) or 8 digits, but got {digitCount}";
                    break;
                    
                case BarcodeFormat.ITF:
                    if (!isNumericOnly)
                        return $"'{code}' - ITF requires only digits (no letters), but contains non-numeric characters";
                    // ITF requires even number of digits
                    if (digitCount % 2 != 0)
                        return $"'{code}' - ITF requires an even number of digits, but got {digitCount}";
                    break;
            }

            return string.Empty; // Valid
        }

        private BitmapSource ConvertToBitmapSource(ZXing.Common.BitMatrix matrix)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            int stride = width; // One byte per pixel for Gray8 format
            byte[] pixels = new byte[height * stride];

            // Convert matrix to pixel array (black = 0, white = 255)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x;
                    // If matrix[x,y] is true, it's a black pixel (0), otherwise white (255)
                    pixels[index] = matrix[x, y] ? (byte)0 : (byte)255;
                }
            }

            // Create BitmapSource with grayscale format
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var validBarcodes = _barcodeItems.Where(item => item.Image != null).ToList();
                
                if (validBarcodes.Count == 0)
                {
                    System.Windows.MessageBox.Show("No barcodes to save. Please generate barcodes first.", "No Barcode", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // If only one barcode, save it directly
                if (validBarcodes.Count == 1)
                {
                    SaveBarcodeToFile(validBarcodes[0]);
                }
                else
                {
                    // Let user select which barcode to save (save the first one)
                    var selectedBarcode = validBarcodes.FirstOrDefault();
                    if (selectedBarcode != null)
                    {
                        SaveBarcodeToFile(selectedBarcode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving barcode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var validBarcodes = _barcodeItems.Where(item => item.Image != null).ToList();
                
                if (validBarcodes.Count == 0)
                {
                    System.Windows.MessageBox.Show("No barcodes to save. Please generate barcodes first.", "No Barcode", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Ask user for folder to save all barcodes
                using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select folder to save all barcodes"
                })
                {
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    int savedCount = 0;

                    foreach (var barcodeItem in validBarcodes)
                    {
                        try
                        {
                            string safeFileName = barcodeItem.Code.Replace(" ", "_").Replace(";", "_").Replace(":", "_");
                            string filePath = Path.Combine(folderPath, $"Barcode_{safeFileName}.png");
                            
                            SaveBarcodeToFile(barcodeItem, filePath);
                            savedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Error saving barcode '{barcodeItem.Code}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                        System.Windows.MessageBox.Show($"Successfully saved {savedCount} of {validBarcodes.Count} barcodes to:\n{folderPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving barcodes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBarcodeToFile(BarcodeItem barcodeItem, string filePath = null)
        {
            if (barcodeItem?.Image == null)
                return;

            if (string.IsNullOrEmpty(filePath))
            {
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                    FileName = $"Barcode_{barcodeItem.Code?.Replace(" ", "_") ?? "Image"}.png"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                filePath = saveDialog.FileName;
            }

            BitmapSource bitmap = barcodeItem.Image;
            BitmapEncoder encoder = filePath.ToLower().EndsWith(".jpg") 
                ? new JpegBitmapEncoder() 
                : filePath.ToLower().EndsWith(".bmp") 
                    ? new BmpBitmapEncoder() 
                    : new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            if (string.IsNullOrEmpty(filePath) || !filePath.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                System.Windows.MessageBox.Show($"Barcode saved successfully to:\n{filePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveAsPdfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var validBarcodes = _barcodeItems.Where(item => item.Image != null).ToList();
                
                if (validBarcodes.Count == 0)
                {
                    System.Windows.MessageBox.Show("No barcodes to save. Please generate barcodes first.", "No Barcode", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Ask user for PDF file location
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = "Barcodes.pdf",
                    DefaultExt = "pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    CreatePdfDocument(validBarcodes, saveDialog.FileName);
                    System.Windows.MessageBox.Show($"PDF saved successfully to:\n{saveDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreatePdfDocument(List<BarcodeItem> barcodes, string filePath)
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Barcodes - AcuPower";
            document.Info.Author = "AcuPower";
            document.Info.Subject = "Generated Barcodes";
            document.Info.Keywords = "Barcode, AcuPower, Inventory";

            // Get selected barcode type for display
            string barcodeType = ((System.Windows.Controls.ComboBoxItem)BarcodeTypeComboBox.SelectedItem)?.Content?.ToString() ?? "Code 128";

            // Page settings
            double pageWidth = XUnit.FromPoint(595); // A4 width in points
            double pageHeight = XUnit.FromPoint(842); // A4 height in points
            double margin = 50;
            double currentY = margin;
            double maxY = pageHeight - margin;
            double barcodeSpacing = 20;

            PdfPage page = document.AddPage();
            page.Width = pageWidth;
            page.Height = pageHeight;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Header - Render text as image to avoid font resolver issues
            // This is a workaround for PdfSharp 6.x font resolver problems
            string headerText = "Barcodes created by AcuPower BarCodes creator";
            XImage headerImage = RenderTextAsImage(headerText, 16, true);
            if (headerImage != null)
            {
                double headerHeight = headerImage.PixelHeight * 72.0 / 96.0; // Convert pixels to points
                double headerWidth = headerImage.PixelWidth * 72.0 / 96.0;
                double headerX = (pageWidth - headerWidth) / 2; // Center
                gfx.DrawImage(headerImage, headerX, currentY, headerWidth, headerHeight);
                currentY += headerHeight + 20;
            }
            else
            {
                // Fallback: draw simple text using basic approach
                currentY += 30;
            }

            // Draw barcode type info as image
            string typeText = $"Barcode Type: {barcodeType}";
            XImage typeImage = RenderTextAsImage(typeText, 12, true);
            if (typeImage != null)
            {
                double typeHeight = typeImage.PixelHeight * 72.0 / 96.0;
                double typeWidth = typeImage.PixelWidth * 72.0 / 96.0;
                gfx.DrawImage(typeImage, margin, currentY, typeWidth, typeHeight);
                currentY += typeHeight + 30;
            }
            else
            {
                currentY += 30;
            }

            // Draw each barcode
            foreach (var barcodeItem in barcodes)
            {
                // Check if we need a new page
                if (currentY > maxY - 200) // Leave space for barcode and label
                {
                    page = document.AddPage();
                    page.Width = pageWidth;
                    page.Height = pageHeight;
                    gfx = XGraphics.FromPdfPage(page);
                    currentY = margin;
                }

                try
                {
                    // Convert BitmapSource to XImage
                    XImage barcodeImage = ConvertBitmapSourceToXImage(barcodeItem.Image);
                    
                    if (barcodeImage != null)
                    {
                        // Calculate barcode size (max width 500 points, maintain aspect ratio)
                        double maxBarcodeWidth = 500;
                        double barcodeWidth = Math.Min(barcodeImage.PixelWidth, maxBarcodeWidth);
                        double barcodeHeight = (barcodeImage.PixelHeight / (double)barcodeImage.PixelWidth) * barcodeWidth;

                        // Center the barcode
                        double xPos = (pageWidth - barcodeWidth) / 2;

                        // Draw barcode
                        gfx.DrawImage(barcodeImage, xPos, currentY, barcodeWidth, barcodeHeight);
                        currentY += barcodeHeight + 10;

                        // Draw label as image
                        string label = barcodeItem.Label ?? $"Code: {barcodeItem.Code}";
                        XImage labelImage = RenderTextAsImage(label, 10, false);
                        if (labelImage != null)
                        {
                            double labelHeight = labelImage.PixelHeight * 72.0 / 96.0;
                            double labelWidth = labelImage.PixelWidth * 72.0 / 96.0;
                            double labelX = (pageWidth - labelWidth) / 2; // Center
                            gfx.DrawImage(labelImage, labelX, currentY, labelWidth, labelHeight);
                            currentY += labelHeight + barcodeSpacing;
                        }
                        else
                        {
                            currentY += 15 + barcodeSpacing;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If barcode image conversion fails, draw error as image
                    string errorLabel = $"Error displaying barcode for: {barcodeItem.Code}";
                    XImage errorImage = RenderTextAsImage(errorLabel, 10, false);
                    if (errorImage != null)
                    {
                        double errorHeight = errorImage.PixelHeight * 72.0 / 96.0;
                        double errorWidth = errorImage.PixelWidth * 72.0 / 96.0;
                        gfx.DrawImage(errorImage, margin, currentY, errorWidth, errorHeight);
                        currentY += errorHeight + 10;
                    }
                    else
                    {
                        currentY += 20;
                    }
                }
            }

            // Save the document
            document.Save(filePath);
            document.Dispose();
        }

        private XImage? ConvertBitmapSourceToXImage(BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                return null;

            try
            {
                // Convert BitmapSource to byte array
                byte[] pixels = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * (bitmapSource.Format.BitsPerPixel / 8)];
                bitmapSource.CopyPixels(pixels, bitmapSource.PixelWidth * (bitmapSource.Format.BitsPerPixel / 8), 0);

                // Create a memory stream
                using (MemoryStream stream = new MemoryStream())
                {
                    // Create PNG encoder
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(stream);

                    // Convert to XImage
                    stream.Position = 0;
                    return XImage.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private XImage? RenderTextAsImage(string text, double fontSize, bool isBold)
        {
            try
            {
                // Create a WPF TextBlock to render text
                System.Windows.Controls.TextBlock textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = text,
                    FontSize = fontSize,
                    FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = System.Windows.Media.Brushes.Black,
                    FontFamily = new System.Windows.Media.FontFamily("Arial")
                };

                // Measure text
                textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                textBlock.Arrange(new System.Windows.Rect(textBlock.DesiredSize));

                // Render to bitmap
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)Math.Ceiling(textBlock.ActualWidth),
                    (int)Math.Ceiling(textBlock.ActualHeight),
                    96, 96, PixelFormats.Pbgra32);
                rtb.Render(textBlock);

                // Convert to XImage
                using (MemoryStream stream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(stream);
                    stream.Position = 0;
                    return XImage.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }
    }

    // Font resolver for PdfSharp - provides standard PDF fonts
    // PdfSharp 6.x requires a font resolver, but standard fonts are handled internally
    public class StandardFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // For PdfSharp 6.x, we return "Times" as the base font family
            // PdfSharp will handle bold/italic variants internally for standard fonts
            return new FontResolverInfo("Times", isBold, isItalic);
        }

        public byte[] GetFont(string faceName)
        {
            // For standard PDF fonts (Times, Helvetica, Courier), PdfSharp handles them internally
            // Returning null tells PdfSharp to use its built-in font handling
            // These fonts are embedded in every PDF viewer, so no font data is needed
            return null;
        }
    }
}
