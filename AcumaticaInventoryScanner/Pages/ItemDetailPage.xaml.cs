/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Demonstration of how to deal with barcode scanning via API
 * 
 * This page displays detailed inventory item information retrieved from Acumatica ERP
 * after barcode scanning, demonstrating API data retrieval and display.
 */

using AcuPower.AcumaticaInventoryScanner.Models;
using AcuPower.AcumaticaInventoryScanner.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AcuPower.AcumaticaInventoryScanner.Pages;

public partial class ItemDetailPage : ContentPage
{
    private InventoryItem? _item;
    private readonly AuthService _authService;
    private readonly DatabaseService _dbService;

    public ItemDetailPage(AuthService authService, DatabaseService dbService)
    {
        InitializeComponent();
        _authService = authService;
        _dbService = dbService;
    }

    public void SetItem(InventoryItem item)
    {
        _item = item;
        
        // Header Section
        IdLabel.Text = item.GetInventoryId();
        DescLabel.Text = !string.IsNullOrEmpty(item.GetDescription()) 
            ? item.GetDescription() 
            : "No Description";
        
        // Type and Status badges
        var itemType = item.GetItemType();
        TypeLabel.Text = !string.IsNullOrEmpty(itemType) ? itemType : "Stock Item";
        
        var itemStatus = item.GetItemStatus();
        StatusBadgeLabel.Text = !string.IsNullOrEmpty(itemStatus) ? itemStatus : "Unknown";
        
        // Price Summary
        var defaultPrice = item.GetDefaultPrice();
        var basePrice = item.GetBasePrice();
        var displayPrice = defaultPrice > 0 ? defaultPrice : basePrice;
        PriceLabel.Text = displayPrice > 0 ? $"${displayPrice:N2}" : "N/A";
        
        var baseUnit = item.GetBaseUnit();
        BaseUnitHeaderLabel.Text = !string.IsNullOrEmpty(baseUnit) ? baseUnit : "EA";
        UnitLabel.Text = !string.IsNullOrEmpty(baseUnit) ? baseUnit : "EA";
        
        // Warehouse Stock Section
        PopulateWarehouseStock(item, baseUnit);
        
        // Item Information Section
        var itemClass = item.GetItemClass();
        ItemClassLabel.Text = !string.IsNullOrEmpty(itemClass) ? itemClass : "—";
        
        var postingClass = item.GetPostingClass();
        PostingClassLabel.Text = !string.IsNullOrEmpty(postingClass) ? postingClass : "—";
        
        var taxCategory = item.GetTaxCategory();
        TaxCategoryLabel.Text = !string.IsNullOrEmpty(taxCategory) ? taxCategory : "—";
        
        var warehouse = item.GetDefaultWarehouse();
        WarehouseLabel.Text = !string.IsNullOrEmpty(warehouse) ? warehouse : "—";
        
        BaseUnitLabel.Text = !string.IsNullOrEmpty(baseUnit) ? baseUnit : "—";
        
        var planningMethod = item.GetPlanningMethod();
        PlanningMethodLabel.Text = !string.IsNullOrEmpty(planningMethod) ? planningMethod : "—";
        
        var valMethod = item.GetValMethod();
        ValMethodLabel.Text = !string.IsNullOrEmpty(valMethod) ? valMethod : "—";
        
        var lotSerialClass = item.GetLotSerialClass();
        LotSerialClassLabel.Text = !string.IsNullOrEmpty(lotSerialClass) ? lotSerialClass : "—";
        
        // Debug log the item data
        LogItemDetails(item);
    }

    private void PopulateWarehouseStock(InventoryItem item, string baseUnit)
    {
        var warehouseDetails = item.GetWarehouseDetails();
        var totalQty = item.GetQtyOnHand();
        
        System.Diagnostics.Debug.WriteLine($"=== Warehouse Details ===");
        System.Diagnostics.Debug.WriteLine($"Warehouse count: {warehouseDetails.Count}");
        System.Diagnostics.Debug.WriteLine($"Total Qty: {totalQty}");
        
        // Clear any existing warehouse items (except the placeholder)
        var childrenToRemove = WarehouseListContainer.Children
            .Where(c => c != NoWarehouseDataPlaceholder)
            .ToList();
        foreach (var child in childrenToRemove)
        {
            WarehouseListContainer.Children.Remove(child);
        }
        
        if (warehouseDetails.Count > 0)
        {
            // Hide placeholder, show warehouse breakdown
            NoWarehouseDataPlaceholder.IsVisible = false;
            TotalQtyContainer.IsVisible = false; // Hide total when we have breakdown
            
            // Sort: default warehouse first, then by name
            var sortedWarehouses = warehouseDetails
                .OrderByDescending(w => w.GetIsDefault())
                .ThenBy(w => w.GetWarehouseId())
                .ToList();
            
            int index = 0;
            foreach (var wh in sortedWarehouses)
            {
                var warehouseRow = CreateWarehouseRow(wh, baseUnit, index % 2 == 0);
                WarehouseListContainer.Children.Add(warehouseRow);
                index++;
                
                System.Diagnostics.Debug.WriteLine($"  Warehouse: {wh.GetWarehouseId()}, Qty: {wh.GetQtyOnHand()}, Available: {wh.GetQtyAvailable()}, IsDefault: {wh.GetIsDefault()}");
            }
            
            // Update total label
            TotalStockLabel.Text = $"Total: {totalQty:N0} {baseUnit}";
        }
        else
        {
            // Show placeholder and total qty fallback
            NoWarehouseDataPlaceholder.IsVisible = true;
            TotalQtyContainer.IsVisible = true;
            TotalQtyLabel.Text = totalQty > 0 ? totalQty.ToString("N0") : "0";
            TotalStockLabel.Text = "";
        }
    }

    private Border CreateWarehouseRow(WarehouseDetail warehouse, string baseUnit, bool isEvenRow)
    {
        var warehouseId = warehouse.GetWarehouseId();
        var qtyOnHand = warehouse.GetQtyOnHand();
        var qtyAvailable = warehouse.GetQtyAvailable();
        var isDefault = warehouse.GetIsDefault();
        
        var backgroundColor = isEvenRow ? Color.FromArgb("#263238") : Color.FromArgb("#2C3E50");
        
        var border = new Border
        {
            Stroke = Color.FromArgb("#37474F"),
            StrokeThickness = 1,
            Padding = new Thickness(15, 12),
            BackgroundColor = backgroundColor,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Margin = new Thickness(0, 0, 0, 1)
        };
        
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        
        // Left side: Warehouse name with default badge
        var leftStack = new VerticalStackLayout { Spacing = 3 };
        
        var warehouseNameStack = new HorizontalStackLayout { Spacing = 8 };
        warehouseNameStack.Children.Add(new Label
        {
            Text = "🏭",
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center
        });
        warehouseNameStack.Children.Add(new Label
        {
            Text = warehouseId,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        });
        
        if (isDefault)
        {
            var defaultBadge = new Border
            {
                Stroke = Color.FromArgb("#4CAF50"),
                StrokeThickness = 1,
                Padding = new Thickness(6, 2),
                BackgroundColor = Color.FromArgb("#1B5E20"),
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                VerticalOptions = LayoutOptions.Center
            };
            defaultBadge.Content = new Label
            {
                Text = "DEFAULT",
                FontSize = 9,
                TextColor = Color.FromArgb("#A5D6A7"),
                FontAttributes = FontAttributes.Bold
            };
            warehouseNameStack.Children.Add(defaultBadge);
        }
        
        leftStack.Children.Add(warehouseNameStack);
        
        // Show available qty if different from on-hand
        if (qtyAvailable > 0 && qtyAvailable != qtyOnHand)
        {
            leftStack.Children.Add(new Label
            {
                Text = $"Available: {qtyAvailable:N0}",
                FontSize = 11,
                TextColor = Color.FromArgb("#78909C")
            });
        }
        
        // Right side: Quantity
        var rightStack = new HorizontalStackLayout 
        { 
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        
        rightStack.Children.Add(new Label
        {
            Text = qtyOnHand.ToString("N0"),
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = qtyOnHand > 0 ? Color.FromArgb("#4DD0E1") : Color.FromArgb("#EF5350"),
            VerticalOptions = LayoutOptions.Center
        });
        rightStack.Children.Add(new Label
        {
            Text = baseUnit,
            FontSize = 12,
            TextColor = Color.FromArgb("#78909C"),
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 3)
        });
        
        grid.Children.Add(leftStack);
        Grid.SetColumn(leftStack, 0);
        
        grid.Children.Add(rightStack);
        Grid.SetColumn(rightStack, 1);
        
        border.Content = grid;
        
        return border;
    }

    private void LogItemDetails(InventoryItem item)
    {
        System.Diagnostics.Debug.WriteLine($"=== Item Details ===");
        System.Diagnostics.Debug.WriteLine($"ID: {item.GetInventoryId()}");
        System.Diagnostics.Debug.WriteLine($"Description: {item.GetDescription()}");
        System.Diagnostics.Debug.WriteLine($"Type: {item.GetItemType()}");
        System.Diagnostics.Debug.WriteLine($"Item Class: {item.GetItemClass()}");
        System.Diagnostics.Debug.WriteLine($"Posting Class: {item.GetPostingClass()}");
        System.Diagnostics.Debug.WriteLine($"Tax Category: {item.GetTaxCategory()}");
        System.Diagnostics.Debug.WriteLine($"Default Warehouse: {item.GetDefaultWarehouse()}");
        System.Diagnostics.Debug.WriteLine($"Base Unit: {item.GetBaseUnit()}");
        System.Diagnostics.Debug.WriteLine($"Default Price: {item.GetDefaultPrice()}");
        System.Diagnostics.Debug.WriteLine($"Base Price: {item.GetBasePrice()}");
        System.Diagnostics.Debug.WriteLine($"Status: {item.GetItemStatus()}");
        System.Diagnostics.Debug.WriteLine($"Planning Method: {item.GetPlanningMethod()}");
        System.Diagnostics.Debug.WriteLine($"Val Method: {item.GetValMethod()}");
        System.Diagnostics.Debug.WriteLine($"Lot/Serial Class: {item.GetLotSerialClass()}");
        System.Diagnostics.Debug.WriteLine($"Total Qty On Hand: {item.GetQtyOnHand()}");
        
        var warehouseDetails = item.GetWarehouseDetails();
        System.Diagnostics.Debug.WriteLine($"Warehouse Details Count: {warehouseDetails.Count}");
        foreach (var wh in warehouseDetails)
        {
            System.Diagnostics.Debug.WriteLine($"  - {wh.GetWarehouseId()}: {wh.GetQtyOnHand()} on hand, {wh.GetQtyAvailable()} available");
        }
    }

}
