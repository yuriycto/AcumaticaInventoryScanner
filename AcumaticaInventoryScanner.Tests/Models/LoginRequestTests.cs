/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for LoginRequest model serialization
 */

using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Models;

public class LoginRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;
}

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_SerializesCorrectly()
    {
        // Arrange
        var request = new LoginRequest
        {
            Name = "admin",
            Password = "secret123",
            Tenant = "CompanyA"
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("\"name\":\"admin\"", json);
        Assert.Contains("\"password\":\"secret123\"", json);
        Assert.Contains("\"tenant\":\"CompanyA\"", json);
    }

    [Fact]
    public void LoginRequest_DeserializesCorrectly()
    {
        // Arrange
        var json = """{"name":"testuser","password":"pass123","tenant":"TestTenant"}""";

        // Act
        var request = JsonSerializer.Deserialize<LoginRequest>(json);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("testuser", request.Name);
        Assert.Equal("pass123", request.Password);
        Assert.Equal("TestTenant", request.Tenant);
    }

    [Fact]
    public void LoginRequest_DefaultValues_AreEmpty()
    {
        // Arrange & Act
        var request = new LoginRequest();

        // Assert
        Assert.Equal(string.Empty, request.Name);
        Assert.Equal(string.Empty, request.Password);
        Assert.Equal(string.Empty, request.Tenant);
    }

    [Fact]
    public void LoginRequest_WithEmptyTenant_SerializesCorrectly()
    {
        // Arrange
        var request = new LoginRequest
        {
            Name = "admin",
            Password = "password",
            Tenant = ""
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("\"tenant\":\"\"", json);
    }

    [Fact]
    public void LoginRequest_IgnoresMissingProperties()
    {
        // Arrange - JSON missing tenant property
        var json = """{"name":"user","password":"pass"}""";

        // Act
        var request = JsonSerializer.Deserialize<LoginRequest>(json);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("user", request.Name);
        Assert.Equal("pass", request.Password);
        Assert.Equal(string.Empty, request.Tenant);
    }
}
