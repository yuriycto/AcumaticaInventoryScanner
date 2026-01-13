/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This service handles local SQLite database caching of scanned inventory items
 * for offline access and demonstration purposes.
 */

using SQLite;
using AcuPower.AcumaticaInventoryScanner.Models;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "AcumaticaScanner.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<InventoryItem>();
    }

    public async Task<List<InventoryItem>> GetItemsAsync()
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.Table<InventoryItem>().ToListAsync();
    }

    public async Task<int> SaveItemAsync(InventoryItem item)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        
        // Prepare the item for SQLite storage by copying CustomField values to simple properties
        item.PrepareForStorage();
        
        // Ensure the item has an Id for the primary key
        if (string.IsNullOrEmpty(item.Id))
        {
            // Use InventoryID as the Id if not set
            item.Id = item.GetInventoryId();
        }
        
        return await _database.InsertOrReplaceAsync(item);
    }
}
