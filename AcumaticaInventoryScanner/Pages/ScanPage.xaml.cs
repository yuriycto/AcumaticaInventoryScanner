/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Shared scanning page for workflow operations
 */

using AcuPower.AcumaticaInventoryScanner.Services;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class ScanPage : ContentPage
{
    private readonly PermissionsService _permissionsService;
    private readonly TaskCompletionSource<string?> _tcs;
    private bool _completed;

    public ScanPage(PermissionsService permissionsService, TaskCompletionSource<string?> tcs)
    {
        InitializeComponent();
        _permissionsService = permissionsService;
        _tcs = tcs;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var hasCameraPermission = await _permissionsService.RequestCameraPermissionAsync();
        if (!hasCameraPermission)
        {
            await DisplayAlert("Permission Required", "Camera permission is required for barcode scanning.", "OK");
            Complete(null);
            return;
        }

        CameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };

        CameraView.IsDetecting = true;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_completed) return;
        var result = e.Results?.FirstOrDefault();
        if (result == null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            CameraView.IsDetecting = false;
            Complete(result.Value);
            await Navigation.PopAsync();
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        CameraView.IsDetecting = false;
        Complete(null);
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!_completed)
        {
            Complete(null);
        }
    }

    private void Complete(string? value)
    {
        if (_completed) return;
        _completed = true;
        _tcs.TrySetResult(value);
    }
}
