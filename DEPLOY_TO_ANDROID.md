# How to Deploy and Run on Android Emulator

## Method 1: Using Visual Studio (Recommended)

### Step 1: Restart Visual Studio
**IMPORTANT:** After installing the MAUI workload, you MUST restart Visual Studio for it to recognize Android targets.

1. Close Visual Studio completely
2. Reopen Visual Studio
3. Open the `AcumaticaInventoryScanner` solution

### Step 2: Verify Android Emulator is Running
1. Open **Tools** → **Android** → **Android Device Manager**
2. Ensure your emulator (Pixel 5 - API 32) is listed and running
3. If not running, click **Start** on the emulator

### Step 3: Select Android Target in Visual Studio
1. In the toolbar, look for the **device dropdown** (next to the green Play button)
2. You should now see options like:
   - **Android Emulators** (expandable)
   - **Physical Devices** (if any connected)
   - Under Android Emulators, you should see: **pixel_5_-_api_32** or similar
3. Select your Android emulator from the dropdown

### Step 4: Run the App
1. Press **F5** or click the green **Start Debugging** button
2. Visual Studio will build, deploy, and launch the app on the emulator

## Method 2: Manual Deployment via ADB (If Visual Studio Still Doesn't Show Android)

If Visual Studio still doesn't show Android targets after restarting, you can manually deploy:

### Step 1: Build the APK
```powershell
cd D:\SourceCode\AcumaticaInventoryScanner\AcumaticaInventoryScanner
dotnet build -f net9.0-android -c Debug
```

### Step 2: Find the APK
The APK will be located at:
```
bin\Debug\net9.0-android\com.companyname.acumaticainventoryscanner-Signed.apk
```

### Step 3: Install via ADB
```powershell
# Make sure emulator is running
adb devices

# Install the APK
adb install -r "bin\Debug\net9.0-android\com.companyname.acumaticainventoryscanner-Signed.apk"

# Launch the app
adb shell am start -n com.companyname.acumaticainventoryscanner/crc6488302ad6e9e4df1a.MauiApplication
```

## Troubleshooting

### Visual Studio Still Doesn't Show Android Targets

1. **Check Workload Installation:**
   ```powershell
   dotnet workload list
   ```
   You should see `maui` and `android` in the list.

2. **Reload the Project:**
   - Right-click the project in Solution Explorer
   - Select **Unload Project**
   - Right-click again and select **Reload Project**

3. **Clean and Rebuild:**
   - **Build** → **Clean Solution**
   - **Build** → **Rebuild Solution**

4. **Check Visual Studio Installation:**
   - Open **Visual Studio Installer**
   - Click **Modify** on your Visual Studio installation
   - Ensure **.NET Multi-platform App UI development** workload is installed
   - Ensure **Mobile development with .NET** workload is installed

5. **Check Project Properties:**
   - Right-click project → **Properties**
   - Go to **Application** tab
   - Verify **Target framework** shows `net9.0-android`

### Emulator Not Detected

1. **Restart ADB:**
   ```powershell
   adb kill-server
   adb start-server
   adb devices
   ```

2. **Check Android SDK Path:**
   - Visual Studio → **Tools** → **Options** → **Xamarin** → **Android Settings**
   - Verify Android SDK location is correct

3. **Restart Emulator:**
   - Close the emulator
   - Start it again from Android Device Manager

## Quick Deploy Script

Save this as `deploy-android.ps1` in the project root:

```powershell
$projectPath = "D:\SourceCode\AcumaticaInventoryScanner\AcumaticaInventoryScanner"
$apkPath = "$projectPath\bin\Debug\net9.0-android\com.companyname.acumaticainventoryscanner-Signed.apk"

Write-Host "Building project..."
Set-Location $projectPath
dotnet build -f net9.0-android -c Debug

if (Test-Path $apkPath) {
    Write-Host "Installing APK on emulator..."
    adb install -r $apkPath
    Write-Host "Launching app..."
    adb shell am start -n com.companyname.acumaticainventoryscanner/crc6488302ad6e9e4df1a.MauiApplication
    Write-Host "Done!"
} else {
    Write-Host "Error: APK not found at $apkPath"
}
```

Run it with:
```powershell
.\deploy-android.ps1
```

