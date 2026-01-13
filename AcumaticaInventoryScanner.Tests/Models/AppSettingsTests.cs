/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for AppSettings model
 */

using System.Text.Json;
using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Models;

/// <summary>
/// Simplified AppSettings for testing (without file system dependencies)
/// </summary>
public class AppSettingsTestModel
{
    public string InstanceUrl { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "24.200.001";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool RememberCredentials { get; set; } = true;
    public DateTime? LastLoginDate { get; set; }
    public string LastScannedBarcode { get; set; } = string.Empty;
    
    // App preferences
    public bool PlaySoundOnScan { get; set; } = true;
    public bool VibrateOnScan { get; set; } = true;
    public bool AutoSearchOnScan { get; set; } = true;
}

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_DefaultValues()
    {
        // Arrange & Act
        var settings = new AppSettingsTestModel();

        // Assert
        Assert.Equal(string.Empty, settings.InstanceUrl);
        Assert.Equal(string.Empty, settings.Tenant);
        Assert.Equal(string.Empty, settings.Username);
        Assert.Equal("24.200.001", settings.ApiVersion);
        Assert.Equal(string.Empty, settings.ClientId);
        Assert.Equal(string.Empty, settings.ClientSecret);
        Assert.True(settings.RememberCredentials);
        Assert.Null(settings.LastLoginDate);
        Assert.True(settings.PlaySoundOnScan);
        Assert.True(settings.VibrateOnScan);
        Assert.True(settings.AutoSearchOnScan);
    }

    [Fact]
    public void AppSettings_SerializesCorrectly()
    {
        // Arrange
        var settings = new AppSettingsTestModel
        {
            InstanceUrl = "https://acumatica.example.com/MyInstance",
            Tenant = "MyCompany",
            Username = "admin",
            ApiVersion = "24.200.001",
            ClientId = "client-id-123",
            ClientSecret = "secret-456",
            RememberCredentials = true,
            LastLoginDate = new DateTime(2024, 1, 15, 10, 30, 0),
            LastScannedBarcode = "ABC123",
            PlaySoundOnScan = false,
            VibrateOnScan = true,
            AutoSearchOnScan = true
        };

        // Act
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

        // Assert
        Assert.Contains("\"InstanceUrl\": \"https://acumatica.example.com/MyInstance\"", json);
        Assert.Contains("\"Tenant\": \"MyCompany\"", json);
        Assert.Contains("\"Username\": \"admin\"", json);
        Assert.Contains("\"ApiVersion\": \"24.200.001\"", json);
        Assert.Contains("\"ClientId\": \"client-id-123\"", json);
        Assert.Contains("\"RememberCredentials\": true", json);
        Assert.Contains("\"PlaySoundOnScan\": false", json);
    }

    [Fact]
    public void AppSettings_DeserializesCorrectly()
    {
        // Arrange
        var json = """
        {
            "InstanceUrl": "https://test.acumatica.com/Site",
            "Tenant": "TestTenant",
            "Username": "testuser",
            "ApiVersion": "23.200.001",
            "ClientId": "test-client",
            "ClientSecret": "test-secret",
            "RememberCredentials": false,
            "PlaySoundOnScan": true,
            "VibrateOnScan": false,
            "AutoSearchOnScan": true
        }
        """;

        // Act
        var settings = JsonSerializer.Deserialize<AppSettingsTestModel>(json);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("https://test.acumatica.com/Site", settings.InstanceUrl);
        Assert.Equal("TestTenant", settings.Tenant);
        Assert.Equal("testuser", settings.Username);
        Assert.Equal("23.200.001", settings.ApiVersion);
        Assert.Equal("test-client", settings.ClientId);
        Assert.Equal("test-secret", settings.ClientSecret);
        Assert.False(settings.RememberCredentials);
        Assert.True(settings.PlaySoundOnScan);
        Assert.False(settings.VibrateOnScan);
    }

    [Fact]
    public void AppSettings_HandlesMissingProperties()
    {
        // Arrange - Minimal JSON
        var json = """{"InstanceUrl": "https://example.com"}""";

        // Act
        var settings = JsonSerializer.Deserialize<AppSettingsTestModel>(json);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("https://example.com", settings.InstanceUrl);
        Assert.Equal(string.Empty, settings.Tenant);
        Assert.Equal("24.200.001", settings.ApiVersion); // Default
        Assert.True(settings.RememberCredentials); // Default
    }

    [Fact]
    public void AppSettings_LastLoginDate_Serialization()
    {
        // Arrange
        var settings = new AppSettingsTestModel
        {
            LastLoginDate = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<AppSettingsTestModel>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.LastLoginDate);
        Assert.Equal(2024, deserialized.LastLoginDate.Value.Year);
        Assert.Equal(6, deserialized.LastLoginDate.Value.Month);
        Assert.Equal(15, deserialized.LastLoginDate.Value.Day);
    }

    [Fact]
    public void AppSettings_ApiVersion_CommonValues()
    {
        // Arrange
        var versions = new[] { "24.200.001", "23.200.001", "22.200.001", "21.200.001" };

        foreach (var version in versions)
        {
            // Act
            var settings = new AppSettingsTestModel { ApiVersion = version };
            var json = JsonSerializer.Serialize(settings);
            var deserialized = JsonSerializer.Deserialize<AppSettingsTestModel>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(version, deserialized.ApiVersion);
        }
    }

    [Fact]
    public void AppSettings_InstanceUrl_WithTrailingSlash()
    {
        // Arrange
        var settings = new AppSettingsTestModel
        {
            InstanceUrl = "https://acumatica.com/MySite/"
        };

        // Act
        var json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<AppSettingsTestModel>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("https://acumatica.com/MySite/", deserialized.InstanceUrl);
    }
}
