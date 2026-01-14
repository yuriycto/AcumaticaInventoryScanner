/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Source-generated JSON serialization context for AOT/trimming compatibility
 * 
 * This ensures JSON serialization works correctly in Release mode builds.
 */

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcuPower.AcumaticaInventoryScanner.Models;

/// <summary>
/// Source-generated JSON serializer context for all model types.
/// This is required for Release builds with trimming enabled.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(InventoryItem))]
[JsonSerializable(typeof(List<InventoryItem>))]
[JsonSerializable(typeof(ODataResponse<InventoryItem>))]
[JsonSerializable(typeof(CustomField))]
[JsonSerializable(typeof(WarehouseDetail))]
[JsonSerializable(typeof(List<WarehouseDetail>))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(AcumaticaEndpointsResponse))]
[JsonSerializable(typeof(AcumaticaEndpoint))]
[JsonSerializable(typeof(List<AcumaticaEndpoint>))]
[JsonSerializable(typeof(AcumaticaVersion))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class AppJsonContext : JsonSerializerContext
{
}
