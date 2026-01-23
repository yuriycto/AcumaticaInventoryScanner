/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Export workflow data to CSV and share files
 */

using System.Text;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class WorkflowExportService
{
    public async Task<string> SaveCsvAsync(string fileName, string csvContent)
    {
        var safeName = fileName.Replace(":", "_").Replace("/", "_").Replace("\\", "_");
        var filePath = Path.Combine(FileSystem.CacheDirectory, safeName);
        await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
        return filePath;
    }

    public async Task ShareFileAsync(string title, string filePath)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(filePath)
        });
    }
}
