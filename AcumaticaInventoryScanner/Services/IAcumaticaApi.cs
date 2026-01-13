/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This interface defines the Acumatica ERP REST API endpoints used for authentication
 * and inventory item queries, demonstrating API integration for barcode scanning workflows.
 */

using AcuPower.AcumaticaInventoryScanner.Models;
using Refit;

namespace AcuPower.AcumaticaInventoryScanner.Services;

public interface IAcumaticaApi
{
    [Post("/entity/auth/login")]
    Task LoginAsync([Body] LoginRequest request);

    [Post("/entity/auth/logout")]
    Task LogoutAsync();

    [Post("/identity/connect/token")]
    Task<TokenResponse> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    // Example OData filter: ?$filter=InventoryID eq 'ABC'
    // Note: Filter is optional - if null/empty, returns all items (with pagination)
    // Acumatica OData API typically returns: { "value": [ {...}, {...} ] }
    // Dynamic endpoint - version is passed as path parameter
    // IMPORTANT: Acumatica uses "StockItem" not "InventoryItem" for the REST API endpoint
    // Use $expand=WarehouseDetails to get per-warehouse quantities
    [Get("/entity/Default/{version}/StockItem")]
    Task<ODataResponse<InventoryItem>> GetInventoryItemsAsync(
        string version, 
        [AliasAs("$filter")] string? filter = null,
        [AliasAs("$expand")] string? expand = null);
    
    // Alternative: Direct list (if API doesn't wrap in value)
    [Get("/entity/Default/{version}/StockItem")]
    Task<List<InventoryItem>> GetInventoryItemsListAsync(
        string version, 
        [AliasAs("$filter")] string? filter = null,
        [AliasAs("$expand")] string? expand = null);
    
    [Put("/entity/Default/{version}/StockItem")]
    Task UpdateInventoryItemAsync(string version, [Body] InventoryItem item);
}
