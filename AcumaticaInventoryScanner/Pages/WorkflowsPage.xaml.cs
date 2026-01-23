/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Workflow hub for inventory operations
 */

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class WorkflowsPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public WorkflowsPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    private async void OnPhysicalCountClicked(object sender, EventArgs e)
    {
        var page = _serviceProvider.GetService<PhysicalCountPage>();
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnInventoryAdjustmentClicked(object sender, EventArgs e)
    {
        var page = _serviceProvider.GetService<InventoryAdjustmentPage>();
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnReceivingClicked(object sender, EventArgs e)
    {
        var page = _serviceProvider.GetService<ReceivingPutAwayPage>();
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnPickingClicked(object sender, EventArgs e)
    {
        var page = _serviceProvider.GetService<PickingPackingPage>();
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnCycleCountClicked(object sender, EventArgs e)
    {
        var page = _serviceProvider.GetService<CycleCountPage>();
        if (page != null) await Navigation.PushAsync(page);
    }
}
