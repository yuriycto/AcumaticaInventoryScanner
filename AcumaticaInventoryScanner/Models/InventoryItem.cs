/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This model represents inventory item data structures returned from Acumatica ERP API,
 * demonstrating data modeling for barcode scanning workflows.
 */

using System.Text.Json;
using System.Text.Json.Serialization;
using SQLite;

namespace AcuPower.AcumaticaInventoryScanner.Models;

/// <summary>
/// Represents an inventory item (StockItem) from Acumatica ERP API.
/// Acumatica wraps field values in objects with a "value" property.
/// </summary>
public class InventoryItem
{
    [PrimaryKey]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    // JSON properties for API deserialization - marked as Ignore for SQLite
    [Ignore]
    [JsonPropertyName("InventoryID")]
    public CustomField? InventoryID { get; set; }

    [Ignore]
    [JsonPropertyName("Description")]
    public CustomField? Description { get; set; }

    [Ignore]
    [JsonPropertyName("Type")]
    public CustomField? Type { get; set; }

    [Ignore]
    [JsonPropertyName("ItemClass")]
    public CustomField? ItemClass { get; set; }

    [Ignore]
    [JsonPropertyName("PostingClass")]
    public CustomField? PostingClass { get; set; }

    [Ignore]
    [JsonPropertyName("TaxCategory")]
    public CustomField? TaxCategory { get; set; }

    [Ignore]
    [JsonPropertyName("DefaultWarehouse")]
    public CustomField? DefaultWarehouse { get; set; }

    [Ignore]
    [JsonPropertyName("BaseUnit")]
    public CustomField? BaseUnit { get; set; }

    [Ignore]
    [JsonPropertyName("DefaultPrice")]
    public CustomField? DefaultPrice { get; set; }

    [Ignore]
    [JsonPropertyName("BasePrice")]
    public CustomField? BasePrice { get; set; }

    [Ignore]
    [JsonPropertyName("ItemStatus")]
    public CustomField? ItemStatus { get; set; }

    [Ignore]
    [JsonPropertyName("PlanningMethod")]
    public CustomField? PlanningMethod { get; set; }

    [Ignore]
    [JsonPropertyName("QtyOnHand")]
    public CustomField? QtyOnHand { get; set; }

    [Ignore]
    [JsonPropertyName("ValMethod")]
    public CustomField? ValMethod { get; set; }

    [Ignore]
    [JsonPropertyName("LotSerialClass")]
    public CustomField? LotSerialClass { get; set; }

    // Warehouse Details - per-warehouse inventory quantities
    [Ignore]
    [JsonPropertyName("WarehouseDetails")]
    public List<WarehouseDetail>? WarehouseDetails { get; set; }

    // SQLite-friendly properties (simple types for storage)
    [JsonIgnore]
    public string InventoryIDValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string DescriptionValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string TypeValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string ItemClassValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string PostingClassValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string TaxCategoryValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string DefaultWarehouseValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string BaseUnitValue { get; set; } = string.Empty;

    [JsonIgnore]
    public decimal DefaultPriceValue { get; set; }

    [JsonIgnore]
    public decimal BasePriceValue { get; set; }

    [JsonIgnore]
    public string ItemStatusValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string PlanningMethodValue { get; set; } = string.Empty;

    [JsonIgnore]
    public decimal QtyOnHandValue { get; set; }

    [JsonIgnore]
    public string ValMethodValue { get; set; } = string.Empty;

    [JsonIgnore]
    public string LotSerialClassValue { get; set; } = string.Empty;

    // Serialized warehouse details for SQLite storage
    [JsonIgnore]
    public string WarehouseDetailsJson { get; set; } = string.Empty;

    // Helper methods to get values (prefer cached values, fallback to CustomField)
    public string GetInventoryId() => !string.IsNullOrEmpty(InventoryIDValue) 
        ? InventoryIDValue 
        : InventoryID?.GetStringValue() ?? Id;
    
    public string GetDescription() => !string.IsNullOrEmpty(DescriptionValue) 
        ? DescriptionValue 
        : Description?.GetStringValue() ?? string.Empty;
    
    public new string GetType() => !string.IsNullOrEmpty(TypeValue) 
        ? TypeValue 
        : Type?.GetStringValue() ?? string.Empty;

    /// <summary>
    /// Gets the item type (alias for GetType to avoid confusion with object.GetType)
    /// </summary>
    public string GetItemType() => GetType();
    
    public string GetItemClass() => !string.IsNullOrEmpty(ItemClassValue) 
        ? ItemClassValue 
        : ItemClass?.GetStringValue() ?? string.Empty;
    
    public string GetPostingClass() => !string.IsNullOrEmpty(PostingClassValue) 
        ? PostingClassValue 
        : PostingClass?.GetStringValue() ?? string.Empty;
    
    public string GetTaxCategory() => !string.IsNullOrEmpty(TaxCategoryValue) 
        ? TaxCategoryValue 
        : TaxCategory?.GetStringValue() ?? string.Empty;
    
    public string GetDefaultWarehouse() => !string.IsNullOrEmpty(DefaultWarehouseValue) 
        ? DefaultWarehouseValue 
        : DefaultWarehouse?.GetStringValue() ?? string.Empty;
    
    public string GetBaseUnit() => !string.IsNullOrEmpty(BaseUnitValue) 
        ? BaseUnitValue 
        : BaseUnit?.GetStringValue() ?? string.Empty;
    
    public decimal GetDefaultPrice() => DefaultPriceValue != 0 
        ? DefaultPriceValue 
        : DefaultPrice?.GetDecimalValue() ?? 0m;
    
    public decimal GetBasePrice() => BasePriceValue != 0 
        ? BasePriceValue 
        : BasePrice?.GetDecimalValue() ?? GetDefaultPrice();
    
    public string GetItemStatus() => !string.IsNullOrEmpty(ItemStatusValue) 
        ? ItemStatusValue 
        : ItemStatus?.GetStringValue() ?? string.Empty;
    
    public string GetPlanningMethod() => !string.IsNullOrEmpty(PlanningMethodValue) 
        ? PlanningMethodValue 
        : PlanningMethod?.GetStringValue() ?? string.Empty;
    
    /// <summary>
    /// Gets the total quantity on hand across all warehouses
    /// </summary>
    public decimal GetQtyOnHand()
    {
        // First try to get from warehouse details
        if (WarehouseDetails != null && WarehouseDetails.Count > 0)
        {
            return WarehouseDetails.Sum(w => w.GetQtyOnHand());
        }
        
        // Fallback to cached value or direct field
        return QtyOnHandValue != 0 
            ? QtyOnHandValue 
            : QtyOnHand?.GetDecimalValue() ?? 0m;
    }

    /// <summary>
    /// Gets warehouse details list, deserializing from JSON if needed
    /// </summary>
    public List<WarehouseDetail> GetWarehouseDetails()
    {
        if (WarehouseDetails != null && WarehouseDetails.Count > 0)
        {
            return WarehouseDetails;
        }
        
        // Try to deserialize from stored JSON
        if (!string.IsNullOrEmpty(WarehouseDetailsJson))
        {
            try
            {
                var details = JsonSerializer.Deserialize<List<WarehouseDetail>>(WarehouseDetailsJson);
                return details ?? new List<WarehouseDetail>();
            }
            catch
            {
                return new List<WarehouseDetail>();
            }
        }
        
        return new List<WarehouseDetail>();
    }

    public string GetValMethod() => !string.IsNullOrEmpty(ValMethodValue) 
        ? ValMethodValue 
        : ValMethod?.GetStringValue() ?? string.Empty;

    public string GetLotSerialClass() => !string.IsNullOrEmpty(LotSerialClassValue) 
        ? LotSerialClassValue 
        : LotSerialClass?.GetStringValue() ?? string.Empty;

    /// <summary>
    /// Populates the SQLite-friendly properties from the CustomField properties.
    /// Call this before saving to SQLite.
    /// </summary>
    public void PrepareForStorage()
    {
        InventoryIDValue = InventoryID?.GetStringValue() ?? string.Empty;
        DescriptionValue = Description?.GetStringValue() ?? string.Empty;
        TypeValue = Type?.GetStringValue() ?? string.Empty;
        ItemClassValue = ItemClass?.GetStringValue() ?? string.Empty;
        PostingClassValue = PostingClass?.GetStringValue() ?? string.Empty;
        TaxCategoryValue = TaxCategory?.GetStringValue() ?? string.Empty;
        DefaultWarehouseValue = DefaultWarehouse?.GetStringValue() ?? string.Empty;
        BaseUnitValue = BaseUnit?.GetStringValue() ?? string.Empty;
        DefaultPriceValue = DefaultPrice?.GetDecimalValue() ?? 0m;
        BasePriceValue = BasePrice?.GetDecimalValue() ?? 0m;
        ItemStatusValue = ItemStatus?.GetStringValue() ?? string.Empty;
        PlanningMethodValue = PlanningMethod?.GetStringValue() ?? string.Empty;
        QtyOnHandValue = QtyOnHand?.GetDecimalValue() ?? 0m;
        ValMethodValue = ValMethod?.GetStringValue() ?? string.Empty;
        LotSerialClassValue = LotSerialClass?.GetStringValue() ?? string.Empty;
        
        // Serialize warehouse details for storage
        if (WarehouseDetails != null && WarehouseDetails.Count > 0)
        {
            // Prepare each warehouse detail for storage
            foreach (var wd in WarehouseDetails)
            {
                wd.PrepareForStorage();
            }
            
            try
            {
                WarehouseDetailsJson = JsonSerializer.Serialize(WarehouseDetails.Select(w => new 
                {
                    WarehouseID = w.GetWarehouseId(),
                    QtyOnHand = w.GetQtyOnHand(),
                    QtyAvailable = w.GetQtyAvailable(),
                    IsDefault = w.GetIsDefault()
                }));
            }
            catch
            {
                WarehouseDetailsJson = string.Empty;
            }
        }
    }
}

/// <summary>
/// Represents warehouse-specific inventory details from Acumatica.
/// This is a nested collection within StockItem.
/// </summary>
public class WarehouseDetail
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("WarehouseID")]
    public CustomField? WarehouseID { get; set; }

    [JsonPropertyName("QtyOnHand")]
    public CustomField? QtyOnHand { get; set; }

    [JsonPropertyName("QtyAvailable")]
    public CustomField? QtyAvailable { get; set; }

    [JsonPropertyName("QtyNotAvailable")]
    public CustomField? QtyNotAvailable { get; set; }

    [JsonPropertyName("QtyOnPOOrders")]
    public CustomField? QtyOnPOOrders { get; set; }

    [JsonPropertyName("QtyOnSOOrders")]
    public CustomField? QtyOnSOOrders { get; set; }

    [JsonPropertyName("IsDefault")]
    public CustomField? IsDefault { get; set; }

    [JsonPropertyName("DefaultReceiptLocationID")]
    public CustomField? DefaultReceiptLocationID { get; set; }

    [JsonPropertyName("DefaultShipmentLocationID")]
    public CustomField? DefaultShipmentLocationID { get; set; }

    // Cached values for storage/display
    [JsonIgnore]
    public string WarehouseIDValue { get; set; } = string.Empty;
    
    [JsonIgnore]
    public decimal QtyOnHandValue { get; set; }
    
    [JsonIgnore]
    public decimal QtyAvailableValue { get; set; }
    
    [JsonIgnore]
    public bool IsDefaultValue { get; set; }

    public string GetWarehouseId() => !string.IsNullOrEmpty(WarehouseIDValue) 
        ? WarehouseIDValue 
        : WarehouseID?.GetStringValue() ?? string.Empty;

    public decimal GetQtyOnHand() => QtyOnHandValue != 0 
        ? QtyOnHandValue 
        : QtyOnHand?.GetDecimalValue() ?? 0m;

    public decimal GetQtyAvailable() => QtyAvailableValue != 0 
        ? QtyAvailableValue 
        : QtyAvailable?.GetDecimalValue() ?? 0m;

    public decimal GetQtyNotAvailable() => QtyNotAvailable?.GetDecimalValue() ?? 0m;

    public decimal GetQtyOnPOOrders() => QtyOnPOOrders?.GetDecimalValue() ?? 0m;

    public decimal GetQtyOnSOOrders() => QtyOnSOOrders?.GetDecimalValue() ?? 0m;

    public bool GetIsDefault()
    {
        if (IsDefaultValue) return true;
        var val = IsDefault?.GetStringValue()?.ToLowerInvariant();
        return val == "true" || val == "1";
    }

    public void PrepareForStorage()
    {
        WarehouseIDValue = WarehouseID?.GetStringValue() ?? string.Empty;
        QtyOnHandValue = QtyOnHand?.GetDecimalValue() ?? 0m;
        QtyAvailableValue = QtyAvailable?.GetDecimalValue() ?? 0m;
        IsDefaultValue = GetIsDefault();
    }
}

/// <summary>
/// Represents a field value from Acumatica API.
/// Acumatica wraps field values in objects like: { "value": "ABC" }
/// </summary>
public class CustomField
{
    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    /// <summary>
    /// Gets the value as a string.
    /// </summary>
    public string GetStringValue()
    {
        if (Value == null) return string.Empty;
        
        try
        {
            return Value.Value.ValueKind switch
            {
                JsonValueKind.String => Value.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => Value.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => string.Empty,
                _ => Value.Value.GetRawText()
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the value as a decimal.
    /// </summary>
    public decimal GetDecimalValue()
    {
        if (Value == null) return 0m;
        
        try
        {
            return Value.Value.ValueKind switch
            {
                JsonValueKind.Number => Value.Value.GetDecimal(),
                JsonValueKind.String => decimal.TryParse(Value.Value.GetString(), out var d) ? d : 0m,
                _ => 0m
            };
        }
        catch
        {
            return 0m;
        }
    }

    /// <summary>
    /// Gets the value as an integer.
    /// </summary>
    public int GetIntValue()
    {
        if (Value == null) return 0;
        
        try
        {
            return Value.Value.ValueKind switch
            {
                JsonValueKind.Number => Value.Value.GetInt32(),
                JsonValueKind.String => int.TryParse(Value.Value.GetString(), out var i) ? i : 0,
                _ => 0
            };
        }
        catch
        {
            return 0;
        }
    }

    public override string ToString() => GetStringValue();
}
