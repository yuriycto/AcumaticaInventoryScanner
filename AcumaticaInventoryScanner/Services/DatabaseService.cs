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
        await _database.CreateTableAsync<CountSession>();
        await _database.CreateTableAsync<CountEntry>();
        await _database.CreateTableAsync<DocumentDraft>();
        await _database.CreateTableAsync<DocumentLine>();
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

    public async Task<List<CountSession>> GetCountSessionsAsync(CountSessionType type)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.Table<CountSession>()
            .Where(s => s.SessionType == type)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveCountSessionAsync(CountSession session)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.InsertOrReplaceAsync(session);
    }

    public async Task<List<CountEntry>> GetCountEntriesAsync(string sessionId)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.Table<CountEntry>()
            .Where(e => e.SessionId == sessionId)
            .OrderByDescending(e => e.ScannedAt)
            .ToListAsync();
    }

    public async Task<int> SaveCountEntryAsync(CountEntry entry)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.InsertOrReplaceAsync(entry);
    }

    public async Task<List<DocumentDraft>> GetDocumentDraftsAsync(DocumentDraftType type)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.Table<DocumentDraft>()
            .Where(d => d.DraftType == type)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveDocumentDraftAsync(DocumentDraft draft)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.InsertOrReplaceAsync(draft);
    }

    public async Task<List<DocumentLine>> GetDocumentLinesAsync(string draftId)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.Table<DocumentLine>()
            .Where(l => l.DraftId == draftId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveDocumentLineAsync(DocumentLine line)
    {
        await InitAsync();
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        return await _database.InsertOrReplaceAsync(line);
    }
}
