# Debugging on Physical Android Device (Galaxy Note 10+)

## Prerequisites

1. **Enable USB Debugging on Your Galaxy Note 10+**
   - Go to **Settings** → **About phone**
   - Tap **Software information**
   - Tap **Build number** 7 times until you see "You are now a developer"
   - Go back to **Settings** → **Developer options**
   - Enable **USB debugging**
   - Enable **Install via USB** (if available)
   - Enable **USB debugging (Security settings)** (if available)

2. **Connect Your Device**
   - Connect your Galaxy Note 10+ to your PC via USB cable
   - On your phone, when prompted, tap **Allow USB debugging** and check **Always allow from this computer**
   - Select **File Transfer** or **MTP** mode (not "Charge only")

3. **Verify Device Detection**
   - Open Command Prompt or PowerShell
   - Navigate to Android SDK platform-tools (usually `C:\Program Files (x86)\Android\android-sdk\platform-tools`)
   - Run: `adb devices`
   - You should see your device listed (e.g., `R58M1234567    device`)

## Visual Studio Configuration

1. **Select Device in Visual Studio**
   - In Visual Studio, look at the debug target dropdown (next to the green play button)
   - You should see your device listed (e.g., "Galaxy Note 10+")
   - Select your device instead of the emulator

2. **Build and Deploy**
   - Press **F5** or click the green **Start** button
   - Visual Studio will build and deploy to your device
   - The app will launch automatically on your phone

## Troubleshooting

### Device Not Detected
- Ensure USB debugging is enabled
- Try a different USB cable (some cables are charge-only)
- Try a different USB port
- Restart ADB: `adb kill-server` then `adb start-server`
- Check Windows Device Manager - your device should appear under "Portable Devices" or "Android Phone"

### "Unauthorized" Device
- On your phone, revoke USB debugging authorizations: **Settings** → **Developer options** → **Revoke USB debugging authorizations**
- Disconnect and reconnect the USB cable
- Accept the authorization prompt on your phone

### Build Errors
- Ensure your device's Android version is compatible (Android 5.0+ / API 21+)
- Galaxy Note 10+ runs Android 9-12, which is fully compatible

## Notes

- The app will be installed on your device and can be launched from the app drawer
- You can debug, set breakpoints, and view logs just like with the emulator
- Physical device testing is better for camera functionality than emulators
