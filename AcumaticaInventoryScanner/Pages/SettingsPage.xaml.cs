/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Settings page for configuring Acumatica connection
 */

using AcuPower.AcumaticaInventoryScanner.Models;
using AcuPower.AcumaticaInventoryScanner.Services;

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly SettingsService _settingsService;
    private AppSettings? _settings;

    public SettingsPage(AuthService authService, SettingsService settingsService)
    {
        InitializeComponent();
        _authService = authService;
        _settingsService = settingsService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            _settings = await AppSettings.LoadAsync();
            
            // Populate form fields
            UrlEntry.Text = _settings.InstanceUrl;
            TenantEntry.Text = _settings.Tenant;
            UsernameEntry.Text = _settings.Username;
            ClientIdEntry.Text = _settings.ClientId;
            ClientSecretEntry.Text = _settings.ClientSecret;
            
            // Set API version picker
            var apiVersionIndex = ApiVersionPicker.ItemsSource?.Cast<string>()
                .ToList().IndexOf(_settings.ApiVersion) ?? 0;
            if (apiVersionIndex >= 0)
                ApiVersionPicker.SelectedIndex = apiVersionIndex;
            else
                ApiVersionPicker.SelectedIndex = 0;
            
            // App preferences
            SoundSwitch.IsToggled = _settings.PlaySoundOnScan;
            VibrateSwitch.IsToggled = _settings.VibrateOnScan;
            AutoSearchSwitch.IsToggled = _settings.AutoSearchOnScan;
            RememberSwitch.IsToggled = _settings.RememberCredentials;
            
            // Load password from secure storage if remembering
            if (_settings.RememberCredentials)
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
            ShowStatus($"Error loading settings: {ex.Message}", false);
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            SaveButton.IsEnabled = false;
            SaveButton.Text = "Saving...";
            
            _settings ??= new AppSettings();
            
            // Save form values
            _settings.InstanceUrl = UrlEntry.Text?.Trim() ?? string.Empty;
            _settings.Tenant = TenantEntry.Text?.Trim() ?? string.Empty;
            _settings.Username = UsernameEntry.Text?.Trim() ?? string.Empty;
            _settings.ClientId = ClientIdEntry.Text?.Trim() ?? string.Empty;
            _settings.ClientSecret = ClientSecretEntry.Text?.Trim() ?? string.Empty;
            _settings.ApiVersion = ApiVersionPicker.SelectedItem?.ToString() ?? "24.200.001";
            
            // App preferences
            _settings.PlaySoundOnScan = SoundSwitch.IsToggled;
            _settings.VibrateOnScan = VibrateSwitch.IsToggled;
            _settings.AutoSearchOnScan = AutoSearchSwitch.IsToggled;
            _settings.RememberCredentials = RememberSwitch.IsToggled;
            
            await _settings.SaveAsync();
            
            // Save password securely if remembering
            if (_settings.RememberCredentials && !string.IsNullOrEmpty(PasswordEntry.Text))
            {
                await SecureStorage.SetAsync("saved_password", PasswordEntry.Text);
            }
            else
            {
                SecureStorage.Remove("saved_password");
            }
            
            ShowStatus("✅ Settings saved successfully!", true);
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Error saving: {ex.Message}", false);
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "💾  Save Settings";
        }
    }

    private async void OnTestConnectionClicked(object? sender, EventArgs e)
    {
        try
        {
            TestButton.IsEnabled = false;
            TestButton.Text = "Testing...";
            ShowStatus("🔄 Testing connection...", true);
            
            var url = UrlEntry.Text?.Trim() ?? string.Empty;
            var username = UsernameEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;
            var clientId = ClientIdEntry.Text?.Trim() ?? string.Empty;
            var clientSecret = ClientSecretEntry.Text?.Trim() ?? string.Empty;
            var tenant = TenantEntry.Text?.Trim() ?? string.Empty;
            var apiVersion = ApiVersionPicker.SelectedItem?.ToString() ?? "24.200.001";
            
            if (string.IsNullOrEmpty(url))
            {
                ShowStatus("❌ Please enter the instance URL", false);
                return;
            }
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                ShowStatus("❌ OAuth credentials required. See README for setup.", false);
                return;
            }
            
            var success = await _authService.LoginAsync(url, tenant, username, password, clientId, clientSecret, apiVersion);
            
            if (success)
            {
                ShowStatus("✅ Connection successful! API is accessible.", true);
            }
            else
            {
                ShowStatus("❌ Connection failed. Check credentials and OAuth setup.", false);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Error: {ex.Message}", false);
        }
        finally
        {
            TestButton.IsEnabled = true;
            TestButton.Text = "🔌  Test Connection";
        }
    }

    private async void OnResetClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Reset Settings", 
            "This will clear all saved settings and credentials. You will need to configure the app again.\n\nAre you sure?", 
            "Yes, Reset", "Cancel");
        
        if (!confirm) return;
        
        try
        {
            // Clear settings file
            await AppSettings.ClearAsync();
            
            // Clear secure storage
            _settingsService.Clear();
            SecureStorage.Remove("saved_password");
            
            // Clear API cache
            _authService.ClearApiCache();
            
            // Clear form
            UrlEntry.Text = string.Empty;
            TenantEntry.Text = string.Empty;
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            ClientIdEntry.Text = string.Empty;
            ClientSecretEntry.Text = string.Empty;
            ApiVersionPicker.SelectedIndex = 0;
            SoundSwitch.IsToggled = true;
            VibrateSwitch.IsToggled = true;
            AutoSearchSwitch.IsToggled = true;
            RememberSwitch.IsToggled = true;
            
            ShowStatus("✅ All settings have been reset!", true);
            
            // Navigate to login page
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Error: {ex.Message}", false);
        }
    }

    private void ShowStatus(string message, bool isSuccess)
    {
        StatusFrame.IsVisible = true;
        StatusLabel.Text = message;
        StatusFrame.BackgroundColor = isSuccess ? Color.FromArgb("#1B5E20") : Color.FromArgb("#B71C1C");
        
        // Auto-hide after 5 seconds
        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(5), () =>
        {
            StatusFrame.IsVisible = false;
        });
    }
}
