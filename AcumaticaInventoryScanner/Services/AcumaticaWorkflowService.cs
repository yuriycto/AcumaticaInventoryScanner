/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Push workflow drafts to Acumatica ERP endpoints
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AcuPower.AcumaticaInventoryScanner.Models;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class WorkflowPushResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
}

public class AcumaticaWorkflowService
{
    private readonly AuthService _authService;
    private readonly SettingsService _settingsService;

    public AcumaticaWorkflowService(AuthService authService, SettingsService settingsService)
    {
        _authService = authService;
        _settingsService = settingsService;
    }

    public async Task<WorkflowPushResult> PushCountAsync(CountSession session, List<CountEntry> entries, string endpointName)
    {
        var payload = new
        {
            Description = Value(session.Name),
            WarehouseID = Value(session.Warehouse),
            Details = entries.Select(e => new
            {
                InventoryID = Value(e.InventoryId),
                Qty = Value(e.QtyCounted),
                WarehouseID = string.IsNullOrWhiteSpace(e.Warehouse) ? null : Value(e.Warehouse),
                LocationID = string.IsNullOrWhiteSpace(e.BinLocation) ? null : Value(e.BinLocation),
                LotSerialNbr = string.IsNullOrWhiteSpace(e.LotSerial) ? null : Value(e.LotSerial)
            })
        };

        return await PostToEndpointAsync(endpointName, payload);
    }

    public async Task<WorkflowPushResult> PushDocumentDraftAsync(DocumentDraft draft, List<DocumentLine> lines, string endpointName)
    {
        var payload = new
        {
            Description = Value(draft.Notes),
            WarehouseID = Value(draft.Warehouse),
            ReferenceNbr = Value(draft.ReferenceNumber),
            Details = lines.Select(l => new
            {
                InventoryID = Value(l.InventoryId),
                Qty = Value(l.Qty),
                WarehouseID = string.IsNullOrWhiteSpace(draft.Warehouse) ? null : Value(draft.Warehouse),
                LocationID = string.IsNullOrWhiteSpace(l.Location) ? null : Value(l.Location),
                LotSerialNbr = string.IsNullOrWhiteSpace(l.LotSerial) ? null : Value(l.LotSerial),
                ReasonCode = string.IsNullOrWhiteSpace(l.Note) ? null : Value(l.Note)
            })
        };

        return await PostToEndpointAsync(endpointName, payload);
    }

    private async Task<WorkflowPushResult> PostToEndpointAsync(string endpointName, object payload)
    {
        try
        {
            var apiVersion = await _settingsService.GetApiVersionAsync() ?? "24.200.001";
            var client = await _authService.GetHttpClientAsync();
            if (client == null)
            {
                return new WorkflowPushResult { Success = false, Message = "Not authenticated. Please login first." };
            }

            var path = $"entity/Default/{apiVersion}/{endpointName}";
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            var response = await client.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new WorkflowPushResult
                {
                    Success = false,
                    Message = $"Acumatica returned {(int)response.StatusCode} {response.ReasonPhrase}",
                    ResponseBody = responseBody
                };
            }

            return new WorkflowPushResult { Success = true, Message = "Draft pushed to Acumatica successfully.", ResponseBody = responseBody };
        }
        catch (Exception ex)
        {
            return new WorkflowPushResult { Success = false, Message = ex.Message };
        }
    }

    private static object Value(object value) => new { value };
}
