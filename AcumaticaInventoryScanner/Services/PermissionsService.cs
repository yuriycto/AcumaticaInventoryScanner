/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This service handles runtime permission requests for camera and network access
 * required for barcode scanning functionality.
 */

using Microsoft.Maui.ApplicationModel;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class PermissionsService
{
    public async Task<bool> RequestCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        
        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.Android)
        {
            // On Android, we need to request permission
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestNetworkPermissionAsync()
    {
        // Network permissions are typically granted at install time on Android
        // But we can check internet access
        var status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();
        return status == PermissionStatus.Granted;
    }
}

