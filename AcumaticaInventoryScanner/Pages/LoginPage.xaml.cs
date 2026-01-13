/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This page handles user authentication with Acumatica ERP, demonstrating
 * OAuth 2.0 and cookie-based authentication for API access.
 */

using System.Text.Json;
using AcuPower.AcumaticaInventoryScanner.Models;
using AcuPower.AcumaticaInventoryScanner.Services;

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private List<string> _availableVersions = new();
    private bool _endpointsFetched = false;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        
        // Load saved settings on startup
        LoadSavedSettingsAsync();
    }
    
    private async void LoadSavedSettingsAsync()
    {
        try
        {
            var settings = await AppSettings.LoadAsync();
            
            if (!string.IsNullOrEmpty(settings.InstanceUrl))
            {
                UrlEntry.Text = settings.InstanceUrl;
            }
            if (!string.IsNullOrEmpty(settings.Tenant))
            {
                TenantEntry.Text = settings.Tenant;
            }
            if (!string.IsNullOrEmpty(settings.Username))
            {
                UsernameEntry.Text = settings.Username;
            }
            if (!string.IsNullOrEmpty(settings.ClientId))
            {
                ClientIdEntry.Text = settings.ClientId;
            }
            if (!string.IsNullOrEmpty(settings.ClientSecret))
            {
                ClientSecretEntry.Text = settings.ClientSecret;
            }
            if (!string.IsNullOrEmpty(settings.ApiVersion))
            {
                ApiVersionEntry.Text = settings.ApiVersion;
            }
            
            // Load password from secure storage
            if (settings.RememberCredentials)
            {
                var savedPassword = await SecureStorage.GetAsync("saved_password");
                if (!string.IsNullOrEmpty(savedPassword))
                {
                    PasswordEntry.Text = savedPassword;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading saved settings: {ex.Message}");
        }
    }
    
    private async void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation to Settings failed: {ex.Message}");
            await DisplayAlert("Navigation Error", "Could not open Settings page.", "OK");
        }
    }

    private async void OnUrlEntryCompleted(object sender, EventArgs e)
    {
        // Auto-fetch endpoints when user finishes entering URL
        await FetchEndpointsAsync();
    }

    private async void OnUrlEntryUnfocused(object sender, FocusEventArgs e)
    {
        // Auto-fetch when user leaves the URL field (tabs to next field)
        if (!_endpointsFetched && !string.IsNullOrWhiteSpace(UrlEntry.Text) && UrlEntry.Text != "https://")
        {
            await FetchEndpointsAsync();
        }
    }

    private async void OnPasswordEntryUnfocused(object sender, FocusEventArgs e)
    {
        // Auto-fetch when user finishes entering password - ensures dropdown is ready before login
        if (!_endpointsFetched && !string.IsNullOrWhiteSpace(UrlEntry.Text) && UrlEntry.Text != "https://")
        {
            System.Diagnostics.Debug.WriteLine("Password field unfocused - triggering endpoint fetch");
            await FetchEndpointsAsync();
        }
    }

    private async void OnApiVersionPickerFocused(object sender, FocusEventArgs e)
    {
        // If picker is empty when focused, try to fetch endpoints
        if (!_endpointsFetched && (_availableVersions.Count == 0 || ApiVersionPicker.ItemsSource == null))
        {
            if (!string.IsNullOrWhiteSpace(UrlEntry.Text) && UrlEntry.Text != "https://")
            {
                await FetchEndpointsAsync();
            }
            else
            {
                await DisplayAlert("Enter URL First", "Please enter the Acumatica instance URL before selecting an API version", "OK");
            }
        }
    }

    private async void OnFetchEndpointsClicked(object sender, EventArgs e)
    {
        await FetchEndpointsAsync();
    }

    private async Task FetchEndpointsAsync()
    {
        var url = UrlEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(url) || url == "https://")
        {
            EndpointStatusLabel.Text = "❌ Please enter URL above first!";
            EndpointStatusLabel.TextColor = Colors.Red;
            await DisplayAlert("Enter URL", "Please enter the Acumatica instance URL first", "OK");
            return;
        }

        // Show loading state
        EndpointLoadingSpinner.IsRunning = true;
        EndpointLoadingSpinner.IsVisible = true;
        FetchEndpointsButton.IsEnabled = false;
        FetchEndpointsButton.Text = "⏳ Loading...";
        EndpointStatusLabel.Text = "🔄 Fetching available endpoints...";
        EndpointStatusLabel.TextColor = Colors.Gray;

        try
        {
            // Build the entity URL
            var entityUrl = url.TrimEnd('/') + "/entity";
            System.Diagnostics.Debug.WriteLine($"=== FETCHING ENDPOINTS ===");
            System.Diagnostics.Debug.WriteLine($"URL: {entityUrl}");

            var response = await _httpClient.GetAsync(entityUrl);
            
            System.Diagnostics.Debug.WriteLine($"Response status: {(int)response.StatusCode} {response.ReasonPhrase}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Server returned {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response length: {json.Length} chars");
            System.Diagnostics.Debug.WriteLine($"Response preview: {(json.Length > 200 ? json.Substring(0, 200) + "..." : json)}");

            var endpointsResponse = JsonSerializer.Deserialize<AcumaticaEndpointsResponse>(json);

            if (endpointsResponse?.Endpoints == null || endpointsResponse.Endpoints.Count == 0)
            {
                throw new Exception("No endpoints found in response");
            }

            System.Diagnostics.Debug.WriteLine($"Parsed {endpointsResponse.Endpoints.Count} total endpoints");

            // Filter to only "Default" endpoints and get unique versions, sorted descending
            _availableVersions = endpointsResponse.Endpoints
                .Where(e => e.Name == "Default" && !string.IsNullOrEmpty(e.Version))
                .Select(e => e.Version!)
                .Distinct()
                .OrderByDescending(v => v)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Filtered to {_availableVersions.Count} Default versions");

            if (_availableVersions.Count == 0)
            {
                // If no Default endpoints, show all unique versions
                _availableVersions = endpointsResponse.Endpoints
                    .Where(e => !string.IsNullOrEmpty(e.Version))
                    .Select(e => e.Version!)
                    .Distinct()
                    .OrderByDescending(v => v)
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"Using all {_availableVersions.Count} versions (no Default found)");
            }

            // Update the picker
            ApiVersionPicker.ItemsSource = _availableVersions;
            
            // Select the latest version by default
            if (_availableVersions.Count > 0)
            {
                ApiVersionPicker.SelectedIndex = 0;
                // Also update the manual entry field
                ApiVersionEntry.Text = _availableVersions[0];
            }

            // Update status
            var buildVersion = endpointsResponse.Version?.AcumaticaBuildVersion ?? "Unknown";
            EndpointStatusLabel.Text = $"✅ Found {_availableVersions.Count} versions! (Build: {buildVersion})";
            EndpointStatusLabel.TextColor = Color.FromArgb("#4CAF50"); // Green
            EndpointStatusLabel.FontAttributes = FontAttributes.Bold;
            _endpointsFetched = true;

            System.Diagnostics.Debug.WriteLine($"SUCCESS: Found {_availableVersions.Count} Default versions: {string.Join(", ", _availableVersions)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR FETCHING ENDPOINTS ===");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            
            EndpointStatusLabel.Text = $"❌ Error: {ex.Message}\n(Use manual entry below)";
            EndpointStatusLabel.TextColor = Colors.Red;

            _endpointsFetched = false;

            // Add some common defaults to picker anyway
            _availableVersions = new List<string> { "24.200.001", "23.200.001", "22.200.001", "20.200.001" };
            ApiVersionPicker.ItemsSource = _availableVersions;
            ApiVersionPicker.SelectedIndex = 0;
            
            // Make sure manual entry has a default
            if (string.IsNullOrWhiteSpace(ApiVersionEntry.Text))
            {
                ApiVersionEntry.Text = "24.200.001";
            }
        }
        finally
        {
            EndpointLoadingSpinner.IsRunning = false;
            EndpointLoadingSpinner.IsVisible = false;
            FetchEndpointsButton.IsEnabled = true;
            FetchEndpointsButton.Text = "Load";
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UrlEntry.Text) || string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in required fields (URL, Username, Password)", "OK");
            return;
        }

        // Get API version - prefer picker selection, fall back to manual entry
        string apiVersion;
        if (ApiVersionPicker.SelectedItem != null)
        {
            apiVersion = ApiVersionPicker.SelectedItem.ToString() ?? "24.200.001";
            System.Diagnostics.Debug.WriteLine($"Using API version from picker: {apiVersion}");
        }
        else if (!string.IsNullOrWhiteSpace(ApiVersionEntry.Text))
        {
            apiVersion = ApiVersionEntry.Text.Trim();
            System.Diagnostics.Debug.WriteLine($"Using API version from manual entry: {apiVersion}");
        }
        else
        {
            apiVersion = "24.200.001";
            System.Diagnostics.Debug.WriteLine($"Using default API version: {apiVersion}");
        }

        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        
        // Client ID and Secret are optional - if not provided, will use cookie-based auth
        string clientId = ClientIdEntry.Text?.Trim() ?? string.Empty;
        string clientSecret = ClientSecretEntry.Text?.Trim() ?? string.Empty;

        System.Diagnostics.Debug.WriteLine($"=== LOGIN ATTEMPT ===");
        System.Diagnostics.Debug.WriteLine($"URL: {UrlEntry.Text.Trim()}");
        System.Diagnostics.Debug.WriteLine($"Tenant: {TenantEntry.Text?.Trim() ?? "(empty)"}");
        System.Diagnostics.Debug.WriteLine($"Username: {UsernameEntry.Text.Trim()}");
        System.Diagnostics.Debug.WriteLine($"API Version: {apiVersion}");
        System.Diagnostics.Debug.WriteLine($"OAuth: {(!string.IsNullOrWhiteSpace(clientId) ? "Yes" : "No")}");

        bool success = await _authService.LoginAsync(UrlEntry.Text.Trim(), TenantEntry.Text?.Trim() ?? string.Empty, 
            UsernameEntry.Text.Trim(), PasswordEntry.Text, clientId, clientSecret, apiVersion);

        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;

        if (success)
        {
            System.Diagnostics.Debug.WriteLine($"=== LOGIN SUCCESS === API Version saved: {apiVersion}");
            
            // Save settings for future use
            var settings = await AppSettings.LoadAsync();
            settings.InstanceUrl = UrlEntry.Text.Trim();
            settings.Tenant = TenantEntry.Text?.Trim() ?? string.Empty;
            settings.Username = UsernameEntry.Text.Trim();
            settings.ClientId = clientId;
            settings.ClientSecret = clientSecret;
            settings.ApiVersion = apiVersion;
            settings.LastLoginDate = DateTime.Now;
            await settings.SaveAsync();
            
            // Save password securely if remembering
            if (settings.RememberCredentials)
            {
                await SecureStorage.SetAsync("saved_password", PasswordEntry.Text);
            }
            
            await DisplayAlert("Success", $"Login successful!\n\nAPI Version: {apiVersion}\n\nThis version will be used for all API calls.", "OK");
            await Navigation.PopModalAsync();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"=== LOGIN FAILED ===");
            await DisplayAlert("Error", "Login failed. Please check:\n- URL is correct\n- Username and password are correct\n- If using OAuth, Client ID and Secret are valid\n- You have network connectivity", "OK");
        }
    }
}
