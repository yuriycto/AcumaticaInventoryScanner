/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This model represents the OData response wrapper that Acumatica API returns.
 */

using System.Text.Json.Serialization;

namespace AcuPower.AcumaticaInventoryScanner.Models;

/// <summary>
/// OData response wrapper - Acumatica returns data in format: { "value": [...] }
/// </summary>
public class ODataResponse<T>
{
    [JsonPropertyName("value")]
    public List<T>? Value { get; set; }
    
    [JsonPropertyName("@odata.context")]
    public string? Context { get; set; }
    
    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }
}
