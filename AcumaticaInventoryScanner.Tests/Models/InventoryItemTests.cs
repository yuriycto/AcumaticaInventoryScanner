/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for InventoryItem model - main inventory data structure
 */

using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Models;

/// <summary>
/// CustomField wrapper for Acumatica's {"value": x} format
/// </summary>
public class TestCustomField
{
    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    public string GetStringValue()
    {
        if (Value == null) return string.Empty;
        try
        {
            return Value.Value.ValueKind switch
            {
                JsonValueKind.String => Value.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => Value.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => Value.Value.GetRawText()
            };
        }
        catch { return string.Empty; }
    }

    public decimal GetDecimalValue()
    {
        if (Value == null) return 0m;
        try
        {
            return Value.Value.ValueKind switch
            {
                JsonValueKind.Number => Value.Value.GetDecimal(),
                JsonValueKind.String => decimal.TryParse(Value.Value.GetString(), out var d) ? d : 0m,
                _ => 0m
            };
        }
        catch { return 0m; }
    }
}

/// <summary>
/// Simplified InventoryItem for testing (without SQLite dependencies)
/// </summary>
public class InventoryItemTestModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("InventoryID")]
    public TestCustomField? InventoryID { get; set; }

    [JsonPropertyName("Description")]
    public TestCustomField? Description { get; set; }

    [JsonPropertyName("DefaultPrice")]
    public TestCustomField? DefaultPrice { get; set; }

    [JsonPropertyName("QtyOnHand")]
    public TestCustomField? QtyOnHand { get; set; }

    [JsonPropertyName("BaseUnit")]
    public TestCustomField? BaseUnit { get; set; }

    [JsonPropertyName("ItemStatus")]
    public TestCustomField? ItemStatus { get; set; }

    [JsonPropertyName("WarehouseDetails")]
    public List<WarehouseDetailTestModel>? WarehouseDetails { get; set; }

    // Helper methods
    public string GetInventoryId() => InventoryID?.GetStringValue() ?? Id;
    public string GetDescription() => Description?.GetStringValue() ?? string.Empty;
    public decimal GetDefaultPrice() => DefaultPrice?.GetDecimalValue() ?? 0m;
    public decimal GetQtyOnHand() => QtyOnHand?.GetDecimalValue() ?? 0m;
    public string GetBaseUnit() => BaseUnit?.GetStringValue() ?? string.Empty;
    public string GetItemStatus() => ItemStatus?.GetStringValue() ?? string.Empty;
}

public class WarehouseDetailTestModel
{
    [JsonPropertyName("WarehouseID")]
    public TestCustomField? WarehouseID { get; set; }

    [JsonPropertyName("QtyOnHand")]
    public TestCustomField? QtyOnHand { get; set; }

    [JsonPropertyName("QtyAvailable")]
    public TestCustomField? QtyAvailable { get; set; }

    [JsonPropertyName("IsDefault")]
    public TestCustomField? IsDefault { get; set; }

    public string GetWarehouseId() => WarehouseID?.GetStringValue() ?? string.Empty;
    public decimal GetQtyOnHand() => QtyOnHand?.GetDecimalValue() ?? 0m;
    public decimal GetQtyAvailable() => QtyAvailable?.GetDecimalValue() ?? 0m;
    
    public bool GetIsDefault()
    {
        var val = IsDefault?.GetStringValue()?.ToLowerInvariant();
        return val == "true" || val == "1";
    }
}

public class InventoryItemTests
{
    [Fact]
    public void InventoryItem_DeserializesFromAcumaticaFormat()
    {
        // Arrange - Acumatica wraps values in {"value": x} format
        var json = """
        {
            "id": "abc-123-def",
            "InventoryID": {"value": "WIDGET001"},
            "Description": {"value": "Test Widget"},
            "DefaultPrice": {"value": 29.99},
            "QtyOnHand": {"value": 100},
            "BaseUnit": {"value": "EA"},
            "ItemStatus": {"value": "Active"}
        }
        """;

        // Act
        var item = JsonSerializer.Deserialize<InventoryItemTestModel>(json);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("abc-123-def", item.Id);
        Assert.Equal("WIDGET001", item.GetInventoryId());
        Assert.Equal("Test Widget", item.GetDescription());
        Assert.Equal(29.99m, item.GetDefaultPrice());
        Assert.Equal(100m, item.GetQtyOnHand());
        Assert.Equal("EA", item.GetBaseUnit());
        Assert.Equal("Active", item.GetItemStatus());
    }

    [Fact]
    public void InventoryItem_HandlesNullFields()
    {
        // Arrange
        var json = """
        {
            "id": "test-123",
            "InventoryID": {"value": "ITEM001"}
        }
        """;

        // Act
        var item = JsonSerializer.Deserialize<InventoryItemTestModel>(json);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("ITEM001", item.GetInventoryId());
        Assert.Equal(string.Empty, item.GetDescription());
        Assert.Equal(0m, item.GetDefaultPrice());
        Assert.Equal(0m, item.GetQtyOnHand());
    }

    [Fact]
    public void InventoryItem_FallsBackToIdWhenNoInventoryID()
    {
        // Arrange
        var json = """{"id": "fallback-id"}""";

        // Act
        var item = JsonSerializer.Deserialize<InventoryItemTestModel>(json);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("fallback-id", item.GetInventoryId());
    }

    [Fact]
    public void InventoryItem_WithWarehouseDetails()
    {
        // Arrange
        var json = """
        {
            "id": "item-1",
            "InventoryID": {"value": "STOCK001"},
            "WarehouseDetails": [
                {
                    "WarehouseID": {"value": "MAIN"},
                    "QtyOnHand": {"value": 50},
                    "QtyAvailable": {"value": 45},
                    "IsDefault": {"value": true}
                },
                {
                    "WarehouseID": {"value": "BACKUP"},
                    "QtyOnHand": {"value": 25},
                    "QtyAvailable": {"value": 25},
                    "IsDefault": {"value": false}
                }
            ]
        }
        """;

        // Act
        var item = JsonSerializer.Deserialize<InventoryItemTestModel>(json);

        // Assert
        Assert.NotNull(item);
        Assert.NotNull(item.WarehouseDetails);
        Assert.Equal(2, item.WarehouseDetails.Count);
        
        var mainWarehouse = item.WarehouseDetails[0];
        Assert.Equal("MAIN", mainWarehouse.GetWarehouseId());
        Assert.Equal(50m, mainWarehouse.GetQtyOnHand());
        Assert.Equal(45m, mainWarehouse.GetQtyAvailable());
        Assert.True(mainWarehouse.GetIsDefault());
        
        var backupWarehouse = item.WarehouseDetails[1];
        Assert.Equal("BACKUP", backupWarehouse.GetWarehouseId());
        Assert.Equal(25m, backupWarehouse.GetQtyOnHand());
        Assert.False(backupWarehouse.GetIsDefault());
    }

    [Fact]
    public void InventoryItem_EmptyWarehouseDetails()
    {
        // Arrange
        var json = """
        {
            "id": "item-no-wh",
            "WarehouseDetails": []
        }
        """;

        // Act
        var item = JsonSerializer.Deserialize<InventoryItemTestModel>(json);

        // Assert
        Assert.NotNull(item);
        Assert.NotNull(item.WarehouseDetails);
        Assert.Empty(item.WarehouseDetails);
    }

    [Fact]
    public void InventoryItem_PriceAsStringValue()
    {
        // Arrange - Sometimes prices come as strings
        var json = """
        {
            "id": "item-string-price",
            "DefaultPrice": {"value": "19.99"}
        }
        """;

        // Act
        var item = JsonSerializer.Deserialize<InventoryItemTestModel>(json);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(19.99m, item.GetDefaultPrice());
    }

    [Fact]
    public void WarehouseDetail_IsDefault_HandlesStringTrue()
    {
        // Arrange
        var json = """
        {
            "WarehouseID": {"value": "WH1"},
            "IsDefault": {"value": "true"}
        }
        """;

        // Act
        var warehouse = JsonSerializer.Deserialize<WarehouseDetailTestModel>(json);

        // Assert
        Assert.NotNull(warehouse);
        Assert.True(warehouse.GetIsDefault());
    }

    [Fact]
    public void WarehouseDetail_IsDefault_HandlesString1()
    {
        // Arrange
        var json = """
        {
            "WarehouseID": {"value": "WH1"},
            "IsDefault": {"value": "1"}
        }
        """;

        // Act
        var warehouse = JsonSerializer.Deserialize<WarehouseDetailTestModel>(json);

        // Assert
        Assert.NotNull(warehouse);
        Assert.True(warehouse.GetIsDefault());
    }

    [Fact]
    public void WarehouseDetail_IsDefault_HandlesFalse()
    {
        // Arrange
        var json = """
        {
            "WarehouseID": {"value": "WH1"},
            "IsDefault": {"value": "false"}
        }
        """;

        // Act
        var warehouse = JsonSerializer.Deserialize<WarehouseDetailTestModel>(json);

        // Assert
        Assert.NotNull(warehouse);
        Assert.False(warehouse.GetIsDefault());
    }
}
