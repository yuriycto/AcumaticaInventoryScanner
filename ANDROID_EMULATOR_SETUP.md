# Android Emulator Setup Guide

This guide will help you set up and run the Acumatica Inventory Scanner app on an Android emulator.

## Prerequisites

1. **Visual Studio 2022** (or later) with:
   - .NET MAUI workload installed
   - Android SDK and tools
   - Android Emulator

2. **Android SDK** (minimum API 21, target API 35)

## Setting Up Android Emulator

### Option 1: Using Visual Studio

1. Open **Tools** → **Android** → **Android Device Manager**
2. Click **New** to create a new emulator
3. Select a device definition (e.g., Pixel 5, Pixel 6)
4. Select a system image:
   - **Recommended**: Android 13 (API 33) or Android 14 (API 34)
   - Minimum: Android 5.0 (API 21)
5. Click **Create** and then **Start** the emulator

### Option 2: Using Android Studio

1. Open Android Studio
2. Go to **Tools** → **Device Manager**
3. Click **Create Device**
4. Select a device and system image
5. Finish the setup

## Running the App

### From Visual Studio

1. Ensure an Android emulator is running
2. Set the project as startup project
3. Select the Android emulator from the debug target dropdown
4. Press **F5** or click **Start Debugging**

### From Command Line

**Note:** .NET MAUI Android projects cannot be run directly with `dotnet run`. Use Visual Studio for debugging, or build and deploy manually:

```bash
# Navigate to project directory
cd AcumaticaInventoryScanner

# Build the project
dotnet build -f net9.0-android

# List available devices/emulators
adb devices

# Install and run on connected device/emulator (after build)
# The APK will be in: bin/Debug/net9.0-android/
# You can install it manually with: adb install bin/Debug/net9.0-android/com.companyname.acumaticainventoryscanner-Signed.apk
```

**Recommended:** Use Visual Studio's built-in run/debug functionality (F5) for the best experience.

## Permissions

The app will automatically request the following permissions at runtime:
- **Camera**: Required for barcode/QR code scanning
- **Internet**: Required for API calls to Acumatica
- **Network State**: Required to check network connectivity

## Troubleshooting

### Emulator Not Showing Up

1. Ensure Android SDK is properly installed
2. Check that Android Emulator is installed via Visual Studio Installer
3. Restart Visual Studio
4. Verify emulator is running: `adb devices`

### Camera Not Working

1. Ensure the emulator has camera support enabled
2. In emulator settings, go to **Extended Controls** (three dots) → **Camera**
3. Set **Front** and **Back** cameras to **Webcam0** or **VirtualScene**
4. Restart the emulator

### Build Errors

1. Clean solution: **Build** → **Clean Solution**
2. Delete `bin` and `obj` folders
3. Restore packages: `dotnet restore`
4. Rebuild: **Build** → **Rebuild Solution**

### Network Issues

1. Ensure emulator has internet access
2. Check that `usesCleartextTraffic="true"` is set in AndroidManifest.xml (already configured)
3. For HTTPS, ensure proper certificates are installed

## Testing Features

### Barcode Scanning
- Use the emulator's camera or a webcam
- Test with QR codes or barcodes
- Manual entry is available as fallback

### API Connection
- Enter your Acumatica instance URL
- Provide OAuth credentials (Client ID, Client Secret)
- Test login functionality

### Offline Mode
- Scanned items are cached locally using SQLite
- Works without network connection for viewing cached data

## Emulator Recommendations

For best performance:
- **RAM**: Allocate at least 2GB to emulator
- **Graphics**: Use Hardware acceleration (HAXM or Hyper-V)
- **API Level**: Use API 33 or 34 for best compatibility
- **Architecture**: x86_64 for Windows hosts

## Additional Notes

- The app supports Android 5.0 (API 21) and above
- AR features require ARCore support (optional, may not work on all emulators)
- For production builds, ensure proper signing configuration

