/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Model for Acumatica API endpoint discovery response
 */

using System.Text.Json.Serialization;

namespace AcuPower.AcumaticaInventoryScanner.Models;

/// <summary>
/// Response from /entity endpoint that lists available API versions
/// </summary>
public class AcumaticaEndpointsResponse
{
    [JsonPropertyName("version")]
    public AcumaticaVersion? Version { get; set; }

    [JsonPropertyName("endpoints")]
    public List<AcumaticaEndpoint>? Endpoints { get; set; }
}

public class AcumaticaVersion
{
    [JsonPropertyName("acumaticaBuildVersion")]
    public string? AcumaticaBuildVersion { get; set; }

    [JsonPropertyName("databaseVersion")]
    public string? DatabaseVersion { get; set; }
}

public class AcumaticaEndpoint
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("href")]
    public string? Href { get; set; }

    public override string ToString() => $"{Name} - {Version}";
}
