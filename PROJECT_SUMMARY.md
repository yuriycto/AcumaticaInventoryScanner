# Acumatica Inventory Scanner - Project Summary

## Created by: AcuPower
## Website: acupowererp.com
## Purpose: Demonstration of how to deal with barcode scanning via API

---

## Project Structure

### 1. AcumaticaInventoryScanner (MAUI Mobile App)
A .NET MAUI mobile application for Android that:
- Scans barcodes/QR codes using device camera
- Connects to Acumatica ERP via REST API
- Queries inventory items by barcode/Inventory ID
- Displays item details (ID, Description, Price)
- Supports OAuth 2.0 and cookie-based authentication
- Caches scanned items locally using SQLite
- Includes AR mode for overlay display

**Target Framework:** .NET 9.0 Android  
**Key Technologies:**
- ZXing.Net.Maui for barcode scanning
- Refit for REST API calls
- SQLite.NET for local caching
- CommunityToolkit.Maui for UI components

### 2. BarcodeGenerator (WPF Desktop App)
A Windows WPF application that:
- Generates various barcode formats (Code 128, EAN, UPC, QR Code, etc.)
- Customizable dimensions
- Save barcodes as PNG, JPEG, or BMP
- Useful for testing and demonstration

**Target Framework:** .NET 8.0 Windows  
**Key Technologies:**
- ZXing.Net for barcode generation
- WPF for user interface

---

## All C# Files Include Header Comments

Every C# source file in both projects includes the following header comment:

```csharp
/*
 * Created by: AcuPower
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * [Additional description specific to each file]
 */
```

### Files with Headers Added:

#### AcumaticaInventoryScanner Project:
- ✅ MainPage.xaml.cs
- ✅ MauiProgram.cs
- ✅ App.xaml.cs
- ✅ AppShell.xaml.cs
- ✅ Services/AuthService.cs
- ✅ Services/IAcumaticaApi.cs
- ✅ Services/SettingsService.cs
- ✅ Services/DatabaseService.cs
- ✅ Services/PermissionsService.cs
- ✅ Pages/LoginPage.xaml.cs
- ✅ Pages/ItemDetailPage.xaml.cs
- ✅ Pages/ArPage.xaml.cs
- ✅ Models/InventoryItem.cs
- ✅ Models/LoginRequest.cs
- ✅ Models/TokenResponse.cs
- ✅ Platforms/Android/MainActivity.cs
- ✅ Platforms/Android/MainApplication.cs

#### BarcodeGenerator Project:
- ✅ MainWindow.xaml.cs
- ✅ App.xaml.cs
- ✅ AssemblyInfo.cs

---

## How to Use

### Mobile App (AcumaticaInventoryScanner):
1. Build and deploy to Android device/emulator
2. Login with Acumatica credentials
3. Scan barcodes or enter Inventory IDs manually
4. View item details from Acumatica ERP

### Barcode Generator (BarcodeGenerator):
1. Build and run the WPF application
2. Enter text/Inventory ID
3. Select barcode type
4. Generate and save barcode images
5. Use generated barcodes to test the mobile app

---

## Integration Workflow

1. **Generate Test Barcodes:**
   - Use BarcodeGenerator WPF app
   - Create barcodes with Inventory IDs from Acumatica
   - Save as image files

2. **Test Scanning:**
   - Use AcumaticaInventoryScanner mobile app
   - Scan generated barcodes
   - Verify API queries work correctly
   - Check item details display

3. **Demonstration:**
   - Show end-to-end workflow
   - Demonstrate API integration
   - Show barcode scanning capabilities

---

## Technical Notes

- All code includes proper attribution to AcuPower
- Code is structured for demonstration purposes
- API integration follows Acumatica REST API standards
- Barcode scanning uses industry-standard ZXing library
- Both projects are production-ready and fully functional

---

## Support

For questions or support regarding this demonstration:
- Website: acupowererp.com
- Company: AcuPower
