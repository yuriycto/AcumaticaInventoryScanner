/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Application settings model for JSON persistence
 */

using System.Text.Json;

namespace AcuPower.AcumaticaInventoryScanner.Models;

public class AppSettings
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
    
    private static readonly string SettingsFileName = "appsettings.json";
    
    public static string GetSettingsFilePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
    }
    
    public static async Task<AppSettings> LoadAsync()
    {
        try
        {
            var path = GetSettingsFilePath();
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
        return new AppSettings();
    }
    
    public async Task SaveAsync()
    {
        try
        {
            var path = GetSettingsFilePath();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            await File.WriteAllTextAsync(path, json);
            System.Diagnostics.Debug.WriteLine($"Settings saved to: {path}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
    
    public static async Task ClearAsync()
    {
        try
        {
            var path = GetSettingsFilePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing settings: {ex.Message}");
        }
    }
}
