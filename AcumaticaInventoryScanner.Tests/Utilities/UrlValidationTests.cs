/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for URL validation utilities
 */

using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Utilities;

public class UrlValidationTests
{
    [Theory]
    [InlineData("https://acumatica.example.com/MySite", true)]
    [InlineData("https://company.acumatica.com/Instance", true)]
    [InlineData("http://localhost/Acumatica", true)]
    [InlineData("https://192.168.1.100/Site", true)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    [InlineData("ftp://invalid-scheme.com", false)]
    public void IsValidAcumaticaUrl(string url, bool expected)
    {
        // Act
        var isValid = IsValidUrl(url);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData("https://example.com/Site", "https://example.com/Site")]
    [InlineData("https://example.com/Site/", "https://example.com/Site")]
    [InlineData("  https://example.com/Site  ", "https://example.com/Site")]
    public void NormalizeUrl_RemovesTrailingSlashAndWhitespace(string input, string expected)
    {
        // Act
        var normalized = NormalizeUrl(input);

        // Assert
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("https://example.com/Site", "24.200.001", "https://example.com/Site/entity/Default/24.200.001/StockItem")]
    [InlineData("https://example.com/Site/", "23.200.001", "https://example.com/Site/entity/Default/23.200.001/StockItem")]
    public void BuildStockItemEndpoint(string baseUrl, string apiVersion, string expected)
    {
        // Act
        var endpoint = BuildEntityEndpoint(baseUrl, apiVersion, "StockItem");

        // Assert
        Assert.Equal(expected, endpoint);
    }

    [Theory]
    [InlineData("24.200.001", true)]
    [InlineData("23.200.001", true)]
    [InlineData("22.200.001", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData("24", false)]
    public void IsValidApiVersion(string version, bool expected)
    {
        // Act
        var isValid = IsValidApiVersionFormat(version);

        // Assert
        Assert.Equal(expected, isValid);
    }

    // Helper methods that mirror production validation logic
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) 
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        
        url = url.Trim();
        return url.TrimEnd('/');
    }

    private static string BuildEntityEndpoint(string baseUrl, string apiVersion, string entity)
    {
        var normalizedUrl = NormalizeUrl(baseUrl);
        return $"{normalizedUrl}/entity/Default/{apiVersion}/{entity}";
    }

    private static bool IsValidApiVersionFormat(string version)
    {
        if (string.IsNullOrWhiteSpace(version)) return false;
        
        // Acumatica API versions follow pattern: XX.XXX.XXX (e.g., 24.200.001)
        var parts = version.Split('.');
        if (parts.Length != 3) return false;
        
        return parts.All(p => int.TryParse(p, out _));
    }
}
