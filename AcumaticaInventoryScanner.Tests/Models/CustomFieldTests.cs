/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for CustomField model - Acumatica field value wrapper
 */

using System.Text.Json;
using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Models;

/// <summary>
/// Represents a field value from Acumatica API (simplified for testing).
/// </summary>
public class CustomField
{
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
                JsonValueKind.Null => string.Empty,
                _ => Value.Value.GetRawText()
            };
        }
        catch
        {
            return string.Empty;
        }
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
        catch
        {
            return 0m;
        }
    }

    public int GetIntValue()
    {
        if (Value == null) return 0;
        
        try
        {
            return Value.Value.ValueKind switch
            {
                JsonValueKind.Number => Value.Value.GetInt32(),
                JsonValueKind.String => int.TryParse(Value.Value.GetString(), out var i) ? i : 0,
                _ => 0
            };
        }
        catch
        {
            return 0;
        }
    }
}

public class CustomFieldTests
{
    [Fact]
    public void GetStringValue_WithStringValue_ReturnsString()
    {
        // Arrange
        var json = """{"value": "TestValue"}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetStringValue();

        // Assert
        Assert.Equal("TestValue", result);
    }

    [Fact]
    public void GetStringValue_WithNumberValue_ReturnsNumberAsString()
    {
        // Arrange
        var json = """{"value": 42}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetStringValue();

        // Assert
        Assert.Equal("42", result);
    }

    [Fact]
    public void GetStringValue_WithNullValue_ReturnsEmptyString()
    {
        // Arrange
        var field = new CustomField { Value = null };

        // Act
        var result = field.GetStringValue();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetStringValue_WithBooleanTrue_ReturnsTrue()
    {
        // Arrange
        var json = """{"value": true}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetStringValue();

        // Assert
        Assert.Equal("true", result);
    }

    [Fact]
    public void GetStringValue_WithBooleanFalse_ReturnsFalse()
    {
        // Arrange
        var json = """{"value": false}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetStringValue();

        // Assert
        Assert.Equal("false", result);
    }

    [Fact]
    public void GetDecimalValue_WithDecimalNumber_ReturnsDecimal()
    {
        // Arrange
        var json = """{"value": 123.45}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetDecimalValue();

        // Assert
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void GetDecimalValue_WithIntegerNumber_ReturnsDecimal()
    {
        // Arrange
        var json = """{"value": 100}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetDecimalValue();

        // Assert
        Assert.Equal(100m, result);
    }

    [Fact]
    public void GetDecimalValue_WithStringNumber_ParsesAndReturnsDecimal()
    {
        // Arrange
        var json = """{"value": "99.99"}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetDecimalValue();

        // Assert
        Assert.Equal(99.99m, result);
    }

    [Fact]
    public void GetDecimalValue_WithNullValue_ReturnsZero()
    {
        // Arrange
        var field = new CustomField { Value = null };

        // Act
        var result = field.GetDecimalValue();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetDecimalValue_WithInvalidString_ReturnsZero()
    {
        // Arrange
        var json = """{"value": "not-a-number"}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetDecimalValue();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetIntValue_WithInteger_ReturnsInt()
    {
        // Arrange
        var json = """{"value": 42}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetIntValue();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetIntValue_WithStringInteger_ParsesAndReturnsInt()
    {
        // Arrange
        var json = """{"value": "123"}""";
        var doc = JsonDocument.Parse(json);
        var field = new CustomField { Value = doc.RootElement.GetProperty("value") };

        // Act
        var result = field.GetIntValue();

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void GetIntValue_WithNullValue_ReturnsZero()
    {
        // Arrange
        var field = new CustomField { Value = null };

        // Act
        var result = field.GetIntValue();

        // Assert
        Assert.Equal(0, result);
    }
}
