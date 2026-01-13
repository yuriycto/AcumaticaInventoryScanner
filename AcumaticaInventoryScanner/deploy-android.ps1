# Quick deployment script for Android emulator
$projectPath = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $projectPath "AcumaticaInventoryScanner"
$apkPath = Join-Path $projectPath "bin\Debug\net9.0-android\com.companyname.acumaticainventoryscanner-Signed.apk"

Write-Host "Building project..."
Set-Location $projectPath
dotnet build -f net9.0-android -c Debug

if ($LASTEXITCODE -eq 0) {
    if (Test-Path $apkPath) {
        Write-Host "Checking for connected devices..."
        $devices = adb devices
        if ($devices -match "device$") {
            Write-Host "Installing APK on emulator..."
            adb install -r $apkPath
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Launching app..."
                adb shell am start -n com.companyname.acumaticainventoryscanner/crc6488302ad6e9e4df1a.MauiApplication
                Write-Host "Done! App should be running on emulator."
            } else {
                Write-Host "Error: Failed to install APK"
            }
        } else {
            Write-Host "Error: No Android device/emulator detected. Please start an emulator first."
            Write-Host "Run: adb devices to check"
        }
    } else {
        Write-Host "Error: APK not found at $apkPath"
        Write-Host "Build may have failed. Check the output above."
    }
} else {
    Write-Host "Error: Build failed"
}

