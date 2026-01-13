/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Unit tests for TokenResponse model (OAuth token)
 */

using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AcuPower.AcumaticaInventoryScanner.Tests.Models;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}

public class TokenResponseTests
{
    [Fact]
    public void TokenResponse_DeserializesFromAcumaticaFormat()
    {
        // Arrange - Typical Acumatica OAuth response
        var json = """
        {
            "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test",
            "expires_in": 3600,
            "token_type": "Bearer"
        }
        """;

        // Act
        var token = JsonSerializer.Deserialize<TokenResponse>(json);

        // Assert
        Assert.NotNull(token);
        Assert.Equal("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test", token.AccessToken);
        Assert.Equal(3600, token.ExpiresIn);
        Assert.Equal("Bearer", token.TokenType);
    }

    [Fact]
    public void TokenResponse_DefaultValues()
    {
        // Arrange & Act
        var token = new TokenResponse();

        // Assert
        Assert.Equal(string.Empty, token.AccessToken);
        Assert.Equal(0, token.ExpiresIn);
        Assert.Equal(string.Empty, token.TokenType);
    }

    [Fact]
    public void TokenResponse_ExpiresIn_TypicalValue()
    {
        // Arrange
        var json = """{"access_token":"token123","expires_in":3600,"token_type":"Bearer"}""";

        // Act
        var token = JsonSerializer.Deserialize<TokenResponse>(json);

        // Assert
        Assert.NotNull(token);
        Assert.Equal(3600, token.ExpiresIn); // 1 hour in seconds
    }

    [Fact]
    public void TokenResponse_SerializesCorrectly()
    {
        // Arrange
        var token = new TokenResponse
        {
            AccessToken = "my-access-token",
            ExpiresIn = 7200,
            TokenType = "Bearer"
        };

        // Act
        var json = JsonSerializer.Serialize(token);

        // Assert
        Assert.Contains("\"access_token\":\"my-access-token\"", json);
        Assert.Contains("\"expires_in\":7200", json);
        Assert.Contains("\"token_type\":\"Bearer\"", json);
    }

    [Fact]
    public void TokenResponse_HandlesLongExpirations()
    {
        // Arrange - Refresh token might have longer expiration
        var json = """{"access_token":"token","expires_in":86400,"token_type":"Bearer"}""";

        // Act
        var token = JsonSerializer.Deserialize<TokenResponse>(json);

        // Assert
        Assert.NotNull(token);
        Assert.Equal(86400, token.ExpiresIn); // 24 hours
    }
}
