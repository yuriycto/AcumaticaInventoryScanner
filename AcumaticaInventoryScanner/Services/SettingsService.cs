/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This service manages secure storage of Acumatica credentials and settings
 * for the inventory scanner application.
 */

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class SettingsService
{
    private const string KeyUrl = "acumatica_url";
    private const string KeyTenant = "acumatica_tenant";
    private const string KeyToken = "access_token";
    private const string KeyApiVersion = "api_version";
    private const string KeyClientId = "oauth_client_id";
    private const string KeyClientSecret = "oauth_client_secret";

    public async Task SaveCredentialsAsync(string url, string tenant, string token, string apiVersion = "24.200.001")
    {
        await SecureStorage.SetAsync(KeyUrl, url);
        await SecureStorage.SetAsync(KeyTenant, tenant);
        await SecureStorage.SetAsync(KeyToken, token);
        await SecureStorage.SetAsync(KeyApiVersion, apiVersion);
    }

    public async Task SaveOAuthCredentialsAsync(string clientId, string clientSecret)
    {
        await SecureStorage.SetAsync(KeyClientId, clientId);
        await SecureStorage.SetAsync(KeyClientSecret, clientSecret);
    }

    public async Task<string?> GetUrlAsync() => await SecureStorage.GetAsync(KeyUrl);
    public async Task<string?> GetTenantAsync() => await SecureStorage.GetAsync(KeyTenant);
    public async Task<string?> GetTokenAsync() => await SecureStorage.GetAsync(KeyToken);
    public async Task<string?> GetApiVersionAsync() => await SecureStorage.GetAsync(KeyApiVersion) ?? "24.200.001";
    public async Task<string?> GetClientIdAsync() => await SecureStorage.GetAsync(KeyClientId);
    public async Task<string?> GetClientSecretAsync() => await SecureStorage.GetAsync(KeyClientSecret);

    public void Clear()
    {
        SecureStorage.Remove(KeyUrl);
        SecureStorage.Remove(KeyTenant);
        SecureStorage.Remove(KeyToken);
        SecureStorage.Remove(KeyApiVersion);
        SecureStorage.Remove(KeyClientId);
        SecureStorage.Remove(KeyClientSecret);
    }
}
