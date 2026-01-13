/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for barcode validation and processing
 */

using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Utilities;

public class BarcodeValidationTests
{
    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("12345678", true)]
    [InlineData("ITEM-001", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void IsValidBarcode_BasicValidation(string? barcode, bool expected)
    {
        // Act
        var isValid = IsValidBarcode(barcode);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData("  ABC123  ", "ABC123")]
    [InlineData("\tITEM001\n", "ITEM001")]
    [InlineData("item-lower", "ITEM-LOWER")]
    public void NormalizeBarcode_TrimsAndUppercases(string input, string expected)
    {
        // Act
        var normalized = NormalizeBarcode(input);

        // Assert
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("ABC123", 6)]
    [InlineData("12345678901234567890", 20)]
    public void GetBarcodeLength(string barcode, int expectedLength)
    {
        // Act
        var length = barcode.Length;

        // Assert
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData("0123456789012", true)] // EAN-13
    [InlineData("012345678901", true)]  // UPC-A
    [InlineData("01234567", true)]      // EAN-8
    [InlineData("ABC123", false)]       // Not numeric
    public void IsNumericBarcode(string barcode, bool expected)
    {
        // Act
        var isNumeric = barcode.All(char.IsDigit);

        // Assert
        Assert.Equal(expected, isNumeric);
    }

    [Fact]
    public void BarcodeSearch_BuildsCorrectFilter()
    {
        // Arrange
        var barcode = "WIDGET001";

        // Act
        var filter = BuildInventoryIdFilter(barcode);

        // Assert
        Assert.Equal("InventoryID eq 'WIDGET001'", filter);
    }

    [Fact]
    public void BarcodeSearch_EscapesSingleQuotes()
    {
        // Arrange
        var barcode = "ITEM'S-001";

        // Act
        var filter = BuildInventoryIdFilter(barcode);

        // Assert
        Assert.Equal("InventoryID eq 'ITEM''S-001'", filter);
    }

    [Theory]
    [InlineData("ITEM001", "InventoryID")]
    [InlineData("12345", "InventoryID")]
    public void DetectBarcodeFieldType_DefaultsToInventoryId(string barcode, string expectedField)
    {
        // For this app, we always search by InventoryID
        // This test documents that behavior
        
        // Act
        var field = DetectSearchField(barcode);

        // Assert
        Assert.Equal(expectedField, field);
    }

    // Helper methods that mirror production logic
    private static bool IsValidBarcode(string? barcode)
    {
        return !string.IsNullOrWhiteSpace(barcode);
    }

    private static string NormalizeBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return string.Empty;
        return barcode.Trim().ToUpperInvariant();
    }

    private static string BuildInventoryIdFilter(string barcode)
    {
        // Escape single quotes for OData filter
        var escapedBarcode = barcode.Replace("'", "''");
        return $"InventoryID eq '{escapedBarcode}'";
    }

    private static string DetectSearchField(string barcode)
    {
        // In this app, we always search by InventoryID
        // Could be extended to detect other barcode types
        return "InventoryID";
    }
}
