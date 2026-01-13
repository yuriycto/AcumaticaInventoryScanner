/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This service handles authentication with Acumatica ERP using OAuth 2.0 and cookie-based
 * authentication methods for API access.
 */

using AcuPower.AcumaticaInventoryScanner.Models;
using Refit;
using System.Net.Http.Headers;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class AuthService
{
    private readonly SettingsService _settingsService;
    private IAcumaticaApi? _api;
    private HttpClient? _cookieAuthHttpClient; // Preserve HttpClient for cookie-based auth

    public AuthService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<bool> LoginAsync(string url, string tenant, string username, string password, string clientId, string clientSecret, string apiVersion = "24.200.001")
    {
        try
        {
            // Create client with the provided URL
            if (!url.EndsWith("/")) url += "/";
            
            System.Diagnostics.Debug.WriteLine($"Logging in to: {url} with API version: {apiVersion}");
            
            // Try OAuth first if Client ID and Secret are provided (RECOMMENDED)
            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Attempting OAuth login with Client ID: {clientId}");
                    _api = RestService.For<IAcumaticaApi>(url);

                    // Use Resource Owner Password Credentials Grant
                    // Note: scope "api" is required for Acumatica REST API access
                    var data = new Dictionary<string, object>
                    {
                        {"grant_type", "password"},
                        {"client_id", clientId},
                        {"client_secret", clientSecret},
                        {"username", username},
                        {"password", password},
                        {"scope", "api"}
                    };

                    var tokenResponse = await _api.GetTokenAsync(data);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        System.Diagnostics.Debug.WriteLine($"OAuth login SUCCESS! Token received, expires in: {tokenResponse.ExpiresIn}s");
                        await _settingsService.SaveCredentialsAsync(url, tenant, tokenResponse.AccessToken, apiVersion);
                        
                        // Also save OAuth credentials for token refresh
                        await _settingsService.SaveOAuthCredentialsAsync(clientId, clientSecret);
                        
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("OAuth login failed: No access token in response");
                    }
                }
                catch (Exception oauthEx)
                {
                    System.Diagnostics.Debug.WriteLine($"OAuth login failed: {oauthEx.Message}");
                    Console.WriteLine($"OAuth login failed: {oauthEx.Message}");
                    // Fall through to cookie-based auth
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OAuth credentials not provided, will attempt cookie-based auth");
            }
            
            // Fallback to cookie-based authentication
            try
            {
                // Dispose old HttpClient if exists
                _cookieAuthHttpClient?.Dispose();
                
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(url),
                    Timeout = TimeSpan.FromSeconds(30)
                };
                
                _api = RestService.For<IAcumaticaApi>(httpClient);
                
                var loginRequest = new LoginRequest
                {
                    Name = username,
                    Password = password,
                    Tenant = tenant
                };
                
                await _api.LoginAsync(loginRequest);
                
                // Store the HttpClient so cookies persist for cookie-based auth
                _cookieAuthHttpClient = httpClient;
                
                // For cookie auth, we'll use a dummy token to indicate successful login
                // The cookie will be maintained by HttpClient
                await _settingsService.SaveCredentialsAsync(url, tenant, "cookie-auth", apiVersion);
                
                return true;
            }
            catch (Exception cookieEx)
            {
                Console.WriteLine($"Cookie login failed: {cookieEx.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
        }
        return false;
    }

    public IAcumaticaApi? GetApi()
    {
        // Synchronous version - use GetApiAsync() instead to avoid deadlocks
        // This is kept for backward compatibility but should be avoided
        return GetApiAsync().GetAwaiter().GetResult();
    }

    public async Task<IAcumaticaApi?> GetApiAsync()
    {
        // Re-create API client using saved token/Url
        // NOTE: In a real app we'd cache this but also handle token expiration.
        // For this task, we assume valid token or re-login.
        
        // This method is a bit simplistic. In a real DI scenario we'd use a comprehensive factory.
        // But for now, we will rely on creating it on demand or caching it.
        
        var url = await _settingsService.GetUrlAsync();
        if (string.IsNullOrEmpty(url)) return null;
        
        var token = await _settingsService.GetTokenAsync();

        HttpClient httpClient;
        
        // For cookie-based auth, reuse the HttpClient that has the cookies
        if (token == "cookie-auth" && _cookieAuthHttpClient != null)
        {
            httpClient = _cookieAuthHttpClient;
        }
        else
        {
            // Create a new HttpClient for OAuth or new cookie auth
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(30) // Add timeout to prevent hanging
            };
            
            // Only set Bearer token if it's not cookie-based auth
            if (!string.IsNullOrEmpty(token) && token != "cookie-auth")
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            // Store HttpClient for cookie-based auth
            if (token == "cookie-auth")
            {
                _cookieAuthHttpClient = httpClient;
            }
        }

        _api = RestService.For<IAcumaticaApi>(httpClient);
        return _api;
    }

    public void ClearApiCache()
    {
        // Clear cached API client to force re-authentication
        _api = null;
        _cookieAuthHttpClient?.Dispose();
        _cookieAuthHttpClient = null;
    }
}
