/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Cycle counting workflow page
 */

using System.Collections.ObjectModel;
using System.Text;
using AcuPower.AcumaticaInventoryScanner.Models;
using AcuPower.AcumaticaInventoryScanner.Services;

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class CycleCountPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly ScanService _scanService;
    private readonly WorkflowExportService _exportService;
    private readonly AcumaticaWorkflowService _workflowService;
    private readonly ObservableCollection<CountEntry> _entries = new();
    private List<CountSession> _sessions = new();
    private CountSession? _currentSession;

    public CycleCountPage(DatabaseService dbService, ScanService scanService, WorkflowExportService exportService, AcumaticaWorkflowService workflowService)
    {
        InitializeComponent();
        _dbService = dbService;
        _scanService = scanService;
        _exportService = exportService;
        _workflowService = workflowService;
        EntriesView.ItemsSource = _entries;
        EndpointEntry.Text = "PhysicalCount";

        FrequencyPicker.ItemsSource = new List<string> { "Daily", "Weekly", "Monthly", "Quarterly" };
        NextDueDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        _sessions = await _dbService.GetCountSessionsAsync(CountSessionType.Cycle);
        SessionPicker.ItemsSource = _sessions;
        SessionPicker.ItemDisplayBinding = new Binding(nameof(CountSession.Name));
    }

    private async void OnStartSessionClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SessionNameEntry.Text))
        {
            await DisplayAlert("Missing Info", "Please enter a session name.", "OK");
            return;
        }

        var session = new CountSession
        {
            SessionType = CountSessionType.Cycle,
            Name = SessionNameEntry.Text.Trim(),
            Warehouse = WarehouseEntry.Text?.Trim() ?? string.Empty,
            BinLocation = BinEntry.Text?.Trim() ?? string.Empty,
            Notes = NotesEntry.Text?.Trim() ?? string.Empty,
            Frequency = FrequencyPicker.SelectedItem?.ToString() ?? string.Empty,
            NextDueDate = NextDueDatePicker.Date
        };

        await _dbService.SaveCountSessionAsync(session);
        _currentSession = session;
        _entries.Clear();
        await LoadSessionsAsync();
        SessionPicker.SelectedItem = session;
    }

    private async void OnSessionSelected(object sender, EventArgs e)
    {
        if (SessionPicker.SelectedItem is not CountSession session) return;
        _currentSession = session;
        SessionNameEntry.Text = session.Name;
        WarehouseEntry.Text = session.Warehouse;
        BinEntry.Text = session.BinLocation;
        NotesEntry.Text = session.Notes;
        FrequencyPicker.SelectedItem = session.Frequency;
        NextDueDatePicker.Date = session.NextDueDate ?? DateTime.Today;
        await LoadEntriesAsync(session.Id);
    }

    private async Task LoadEntriesAsync(string sessionId)
    {
        _entries.Clear();
        var entries = await _dbService.GetCountEntriesAsync(sessionId);
        foreach (var entry in entries)
        {
            _entries.Add(entry);
        }
    }

    private async void OnScanItemClicked(object sender, EventArgs e)
    {
        if (_currentSession == null)
        {
            await DisplayAlert("No Session", "Please start or load a session first.", "OK");
            return;
        }

        var code = await _scanService.ScanAsync(Navigation);
        if (!string.IsNullOrWhiteSpace(code))
        {
            await AddEntryAsync(code.Trim());
        }
    }

    private async void OnManualAddClicked(object sender, EventArgs e)
    {
        if (_currentSession == null)
        {
            await DisplayAlert("No Session", "Please start or load a session first.", "OK");
            return;
        }

        var code = await DisplayPromptAsync("Manual Entry", "Enter Inventory ID:");
        if (!string.IsNullOrWhiteSpace(code))
        {
            await AddEntryAsync(code.Trim());
        }
    }

    private async Task AddEntryAsync(string inventoryId)
    {
        var qtyText = await DisplayPromptAsync("Quantity", "Enter counted quantity:", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(qtyText) || !decimal.TryParse(qtyText, out var qty))
        {
            await DisplayAlert("Invalid Quantity", "Please enter a valid number.", "OK");
            return;
        }

        var entry = new CountEntry
        {
            SessionId = _currentSession?.Id ?? string.Empty,
            InventoryId = inventoryId,
            QtyCounted = qty,
            Warehouse = WarehouseEntry.Text?.Trim() ?? string.Empty,
            BinLocation = BinEntry.Text?.Trim() ?? string.Empty
        };

        await _dbService.SaveCountEntryAsync(entry);
        _entries.Insert(0, entry);
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (_currentSession == null || !_entries.Any())
        {
            await DisplayAlert("Nothing to Export", "Add items to export.", "OK");
            return;
        }

        var csv = BuildCsv();
        var fileName = $"CycleCount_{_currentSession.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var path = await _exportService.SaveCsvAsync(fileName, csv);
        await _exportService.ShareFileAsync("Cycle Count Export", path);
    }

    private async void OnPushClicked(object sender, EventArgs e)
    {
        if (_currentSession == null || !_entries.Any())
        {
            await DisplayAlert("Nothing to Push", "Add items before pushing.", "OK");
            return;
        }

        var endpoint = string.IsNullOrWhiteSpace(EndpointEntry.Text) ? "PhysicalCount" : EndpointEntry.Text.Trim();
        var result = await _workflowService.PushCountAsync(_currentSession, _entries.ToList(), endpoint);
        await DisplayAlert(result.Success ? "Success" : "Push Failed", result.Success ? result.Message : $"{result.Message}\n\n{result.ResponseBody}", "OK");
    }

    private string BuildCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("InventoryID,QtyCounted,Warehouse,BinLocation,ScannedAt");
        foreach (var entry in _entries)
        {
            sb.AppendLine($"{entry.InventoryId},{entry.QtyCounted},{entry.Warehouse},{entry.BinLocation},{entry.ScannedAt:o}");
        }
        return sb.ToString();
    }
}
