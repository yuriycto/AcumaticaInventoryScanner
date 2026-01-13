/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for OData response wrapper
 */

using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Models;

public class ODataResponse<T>
{
    [JsonPropertyName("value")]
    public List<T>? Value { get; set; }
    
    [JsonPropertyName("@odata.context")]
    public string? Context { get; set; }
    
    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }
}

// Simple test model
public class TestItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ODataResponseTests
{
    [Fact]
    public void ODataResponse_DeserializesValueArray()
    {
        // Arrange
        var json = """
        {
            "value": [
                {"id": "1", "name": "Item1"},
                {"id": "2", "name": "Item2"}
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ODataResponse<TestItem>>(json);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Value);
        Assert.Equal(2, response.Value.Count);
        Assert.Equal("1", response.Value[0].Id);
        Assert.Equal("Item1", response.Value[0].Name);
        Assert.Equal("2", response.Value[1].Id);
        Assert.Equal("Item2", response.Value[1].Name);
    }

    [Fact]
    public void ODataResponse_DeserializesWithContext()
    {
        // Arrange
        var json = """
        {
            "@odata.context": "https://example.com/$metadata#StockItem",
            "value": []
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ODataResponse<TestItem>>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("https://example.com/$metadata#StockItem", response.Context);
    }

    [Fact]
    public void ODataResponse_DeserializesWithCount()
    {
        // Arrange
        var json = """
        {
            "@odata.count": 100,
            "value": [{"id": "1", "name": "Test"}]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ODataResponse<TestItem>>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(100, response.Count);
    }

    [Fact]
    public void ODataResponse_EmptyValue_ReturnsEmptyList()
    {
        // Arrange
        var json = """{"value": []}""";

        // Act
        var response = JsonSerializer.Deserialize<ODataResponse<TestItem>>(json);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Value);
        Assert.Empty(response.Value);
    }

    [Fact]
    public void ODataResponse_NullValue_IsNull()
    {
        // Arrange
        var json = """{"value": null}""";

        // Act
        var response = JsonSerializer.Deserialize<ODataResponse<TestItem>>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Value);
    }

    [Fact]
    public void ODataResponse_FullAcumaticaFormat()
    {
        // Arrange - Full Acumatica-style response
        var json = """
        {
            "@odata.context": "https://acumatica.example.com/entity/Default/24.200.001/$metadata#StockItem",
            "@odata.count": 2,
            "value": [
                {"id": "abc-123", "name": "Widget A"},
                {"id": "def-456", "name": "Widget B"}
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ODataResponse<TestItem>>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("StockItem", response.Context);
        Assert.Equal(2, response.Count);
        Assert.Equal(2, response.Value?.Count);
    }

    [Fact]
    public void ODataResponse_SerializesCorrectly()
    {
        // Arrange
        var response = new ODataResponse<TestItem>
        {
            Context = "https://test.com/$metadata",
            Count = 1,
            Value = new List<TestItem>
            {
                new TestItem { Id = "test-1", Name = "Test Item" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("\"@odata.context\":\"https://test.com/$metadata\"", json);
        Assert.Contains("\"@odata.count\":1", json);
        Assert.Contains("\"value\":", json);
    }
}
