/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Inventory adjustment draft workflow
 */

using System.Collections.ObjectModel;
using System.Text;
using AcuPower.AcumaticaInventoryScanner.Models;
using AcuPower.AcumaticaInventoryScanner.Services;

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class InventoryAdjustmentPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly ScanService _scanService;
    private readonly WorkflowExportService _exportService;
    private readonly AcumaticaWorkflowService _workflowService;
    private readonly ObservableCollection<DocumentLine> _lines = new();
    private List<DocumentDraft> _drafts = new();
    private DocumentDraft? _currentDraft;

    public InventoryAdjustmentPage(DatabaseService dbService, ScanService scanService, WorkflowExportService exportService, AcumaticaWorkflowService workflowService)
    {
        InitializeComponent();
        _dbService = dbService;
        _scanService = scanService;
        _exportService = exportService;
        _workflowService = workflowService;
        LinesView.ItemsSource = _lines;
        EndpointEntry.Text = "InventoryAdjustment";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDraftsAsync();
    }

    private async Task LoadDraftsAsync()
    {
        _drafts = await _dbService.GetDocumentDraftsAsync(DocumentDraftType.InventoryAdjustment);
        DraftPicker.ItemsSource = _drafts;
        DraftPicker.ItemDisplayBinding = new Binding(nameof(DocumentDraft.ReferenceNumber));
    }

    private async void OnCreateDraftClicked(object sender, EventArgs e)
    {
        var draft = new DocumentDraft
        {
            DraftType = DocumentDraftType.InventoryAdjustment,
            ReferenceNumber = ReferenceEntry.Text?.Trim() ?? string.Empty,
            Warehouse = WarehouseEntry.Text?.Trim() ?? string.Empty,
            Notes = NotesEntry.Text?.Trim() ?? string.Empty
        };

        await _dbService.SaveDocumentDraftAsync(draft);
        _currentDraft = draft;
        _lines.Clear();
        await LoadDraftsAsync();
        DraftPicker.SelectedItem = draft;
    }

    private async void OnDraftSelected(object sender, EventArgs e)
    {
        if (DraftPicker.SelectedItem is not DocumentDraft draft) return;
        _currentDraft = draft;
        ReferenceEntry.Text = draft.ReferenceNumber;
        WarehouseEntry.Text = draft.Warehouse;
        NotesEntry.Text = draft.Notes;
        await LoadLinesAsync(draft.Id);
    }

    private async Task LoadLinesAsync(string draftId)
    {
        _lines.Clear();
        var lines = await _dbService.GetDocumentLinesAsync(draftId);
        foreach (var line in lines)
        {
            _lines.Add(line);
        }
    }

    private async void OnScanItemClicked(object sender, EventArgs e)
    {
        if (_currentDraft == null)
        {
            await DisplayAlert("No Draft", "Create or load a draft first.", "OK");
            return;
        }

        var code = await _scanService.ScanAsync(Navigation);
        if (!string.IsNullOrWhiteSpace(code))
        {
            await AddLineAsync(code.Trim());
        }
    }

    private async void OnManualAddClicked(object sender, EventArgs e)
    {
        if (_currentDraft == null)
        {
            await DisplayAlert("No Draft", "Create or load a draft first.", "OK");
            return;
        }

        var code = await DisplayPromptAsync("Manual Entry", "Enter Inventory ID:");
        if (!string.IsNullOrWhiteSpace(code))
        {
            await AddLineAsync(code.Trim());
        }
    }

    private async Task AddLineAsync(string inventoryId)
    {
        var qtyText = await DisplayPromptAsync("Quantity", "Enter adjustment quantity:", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(qtyText) || !decimal.TryParse(qtyText, out var qty))
        {
            await DisplayAlert("Invalid Quantity", "Please enter a valid number.", "OK");
            return;
        }

        var line = new DocumentLine
        {
            DraftId = _currentDraft?.Id ?? string.Empty,
            InventoryId = inventoryId,
            Qty = qty,
            Location = string.Empty,
            Note = NotesEntry.Text?.Trim() ?? string.Empty
        };

        await _dbService.SaveDocumentLineAsync(line);
        _lines.Insert(0, line);
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (_currentDraft == null || !_lines.Any())
        {
            await DisplayAlert("Nothing to Export", "Add lines to export.", "OK");
            return;
        }

        var csv = BuildCsv();
        var fileName = $"InventoryAdjustment_{_currentDraft.ReferenceNumber}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var path = await _exportService.SaveCsvAsync(fileName, csv);
        await _exportService.ShareFileAsync("Inventory Adjustment Export", path);
    }

    private async void OnPushClicked(object sender, EventArgs e)
    {
        if (_currentDraft == null || !_lines.Any())
        {
            await DisplayAlert("Nothing to Push", "Add lines before pushing.", "OK");
            return;
        }

        var endpoint = string.IsNullOrWhiteSpace(EndpointEntry.Text) ? "InventoryAdjustment" : EndpointEntry.Text.Trim();
        var result = await _workflowService.PushDocumentDraftAsync(_currentDraft, _lines.ToList(), endpoint);
        await DisplayAlert(result.Success ? "Success" : "Push Failed", result.Success ? result.Message : $"{result.Message}\n\n{result.ResponseBody}", "OK");
    }

    private string BuildCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("InventoryID,Qty,Warehouse,Location,CreatedAt");
        foreach (var line in _lines)
        {
            sb.AppendLine($"{line.InventoryId},{line.Qty},{WarehouseEntry.Text},{line.Location},{line.CreatedAt:o}");
        }
        return sb.ToString();
    }
}
