/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Centralized barcode scanning workflow for multiple pages
 */

using AcuPower.AcumaticaInventoryScanner.Pages;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class ScanService
{
    private readonly PermissionsService _permissionsService;

    public ScanService(PermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    public async Task<string?> ScanAsync(INavigation navigation)
    {
        var tcs = new TaskCompletionSource<string?>();
        var scanPage = new ScanPage(_permissionsService, tcs);
        await navigation.PushAsync(scanPage);
        return await tcs.Task;
    }
}
