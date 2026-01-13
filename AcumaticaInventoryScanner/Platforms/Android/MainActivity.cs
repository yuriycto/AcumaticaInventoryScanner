/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This file contains Android platform-specific initialization and permission handling
 * for camera access required for barcode scanning functionality.
 */

using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace AcuPower.AcumaticaInventoryScanner;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestCameraPermission();
    }

    private void RequestCameraPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Android.Manifest.Permission.Camera }, 100);
            }
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        // OnRequestPermissionsResult requires API level 23+ (Android M)
        // Our min SDK is 21, but this method is only called on 23+, so we add a runtime check
        // and suppress the analyzer warning since we've verified the API level
#pragma warning disable CA1416 // Validate platform compatibility
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore CA1416
        
        if (requestCode == 100)
        {
            if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                // Camera permission granted
            }
            else
            {
                // Camera permission denied - show message
                Android.Widget.Toast.MakeText(this, "Camera permission is required for barcode scanning", Android.Widget.ToastLength.Long)?.Show();
            }
        }
    }
}
