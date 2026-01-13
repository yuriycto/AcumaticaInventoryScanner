/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This model represents authentication request data for Acumatica ERP API login.
 */

using System.Text.Json.Serialization;

namespace AcuPower.AcumaticaInventoryScanner.Models;

public class LoginRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;
}
