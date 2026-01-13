# Acumatica Inventory Scanner - Testing Guide

## Overview
This app connects to your Acumatica ERP instance to scan barcodes, query inventory items, and view item details. After successful login, you can test various features.

---

## Step 1: Login to Acumatica

### Login Process:
1. **Launch the app** - The login screen should appear automatically if you're not logged in
2. **Enter your credentials:**
   - **Instance URL:** `https://acupower-demo.co.uk/openAI25R1`
   - **Tenant:** Leave empty (or enter if you have a specific tenant)
   - **Username:** Your Acumatica username
   - **Password:** Your Acumatica password
   - **Client ID:** Leave empty (uses cookie-based auth)
   - **Client Secret:** Leave empty (uses cookie-based auth)
3. **Click "Login"** - Wait for the loading spinner
4. **Success:** You'll be redirected to the main scanning screen

### What Happens:
- The app authenticates with Acumatica using cookie-based authentication
- Your credentials are securely stored
- An API connection is established for querying inventory

---

## Step 2: Main Screen Features

After login, you'll see the **Scanner** screen with:

### Camera View:
- **Live camera feed** for barcode scanning
- Automatically detects barcodes/QR codes when you point at them
- Shows a loading spinner while querying Acumatica

### Buttons:
1. **Toggle AR Mode** - Opens AR view (experimental feature)
2. **Manual Entry** - Enter barcode/Inventory ID manually

---

## Step 3: Testing Barcode Scanning

### Method 1: Scan a Physical Barcode
1. **Point your camera** at a barcode or QR code
2. **Wait for detection** - The app will automatically detect the barcode
3. **Loading appears** - The app queries Acumatica for the item
4. **Results:**
   - **If found:** Navigates to Item Detail page
   - **If not found:** Shows "Not Found" alert

### Method 2: Manual Entry
1. **Tap "Manual Entry"** button
2. **Enter Inventory ID or Barcode:**
   - Example: `AALEGO500`
   - Or any valid Inventory ID from your Acumatica system
3. **Tap OK**
4. **Results:** Same as scanning - shows item details or "Not Found"

### What Gets Queried:
The app uses Acumatica's OData API:
- **Endpoint:** `/entity/Default/22.200.001/InventoryItem`
- **Filter:** `InventoryID eq 'YOUR_BARCODE'`
- **Returns:** Inventory item details (ID, Description, Price, etc.)

---

## Step 4: Item Detail Page

When an item is found, you'll see:

### Displayed Information:
- **Inventory ID:** The item's identifier
- **Description:** Item description from Acumatica
- **Price:** Base price (if available)
- **Stock:** Currently shows "N/A" (needs API field mapping)

### Actions:
- **Update Button:** Currently shows a notice (stock updates require Inventory Adjustment documents in Acumatica)

### Navigation:
- **Back button:** Returns to scanner screen
- Scanning resumes automatically after returning

---

## Step 5: Testing Different Scenarios

### ✅ Success Scenarios:

1. **Valid Inventory ID:**
   - Scan or enter a valid Inventory ID from your Acumatica system
   - Should display item details

2. **Multiple Scans:**
   - Scan different items one after another
   - Each scan should query and display the item

### ❌ Error Scenarios:

1. **Invalid/Non-existent Item:**
   - Scan a barcode that doesn't exist in Acumatica
   - Should show "Not Found" alert
   - Scanning resumes automatically

2. **Network Issues:**
   - Disable internet/WiFi
   - Try scanning
   - Should show error message
   - App should handle gracefully

3. **API Connection Lost:**
   - If token expires or connection fails
   - Should show "Not connected to API" error
   - May need to login again

---

## Step 6: AR Mode (Experimental)

1. **Tap "Toggle AR Mode"** button
2. **AR View Opens:**
   - Shows camera feed with AR overlay
   - Can scan barcodes in AR mode
   - Displays item information overlaid on camera

**Note:** AR features are basic and may need additional setup for full functionality.

---

## Step 7: Offline Caching

The app uses SQLite to cache scanned items:
- **Location:** App data directory
- **Purpose:** Store recent scans for offline access
- **Note:** Currently used for caching, full offline mode may need additional implementation

---

## Testing Checklist

### Basic Functionality:
- [ ] Login with valid credentials
- [ ] Camera permission is requested and granted
- [ ] Camera view appears and works
- [ ] Scan a valid barcode/Inventory ID
- [ ] Item details are displayed correctly
- [ ] Manual entry works
- [ ] "Not Found" message appears for invalid items
- [ ] Back navigation works

### Error Handling:
- [ ] Invalid credentials show error
- [ ] Network errors are handled gracefully
- [ ] Missing camera permission shows message
- [ ] API connection errors are displayed

### Data Validation:
- [ ] Inventory ID is displayed correctly
- [ ] Description is shown (if available)
- [ ] Price is displayed (if available)
- [ ] Empty/null fields are handled

---

## Known Limitations

1. **Stock Updates:**
   - Direct stock quantity updates require Inventory Adjustment documents
   - Current implementation shows a notice
   - Full implementation would need to create IN301000 documents

2. **API Version:**
   - Uses Acumatica API version 22.200.001
   - May need adjustment for different API versions
   - Endpoint: `/entity/Default/22.200.001/InventoryItem`

3. **Stock Quantity:**
   - Stock quantity display shows "N/A"
   - Requires mapping additional API fields (QtyOnHand, etc.)

4. **AR Features:**
   - Basic AR implementation
   - May need ARCore setup for full functionality

---

## Troubleshooting

### Camera Not Working:
- Check camera permissions in device settings
- Restart the app
- Ensure device has a working camera

### Login Fails:
- Verify URL is correct (include https://)
- Check username/password
- Ensure network connectivity
- Try cookie-based auth (leave Client ID/Secret empty)

### Items Not Found:
- Verify Inventory ID exists in Acumatica
- Check API endpoint version matches your Acumatica version
- Ensure user has permissions to view inventory items

### API Errors:
- Check Visual Studio Output window for detailed error messages
- Verify Acumatica instance is accessible
- Check if API version 22.200.001 is correct for your instance

---

## Next Steps for Full Testing

1. **Get Valid Inventory IDs:**
   - Log into Acumatica web interface
   - Go to Inventory → Items
   - Note some Inventory IDs to test with

2. **Test with Real Barcodes:**
   - Use physical products with barcodes
   - Ensure those barcodes match Inventory IDs in Acumatica

3. **Verify Data Accuracy:**
   - Compare app data with Acumatica web interface
   - Check that descriptions, prices match

4. **Test Network Scenarios:**
   - Test with WiFi
   - Test with mobile data
   - Test offline (should show cached data if implemented)

---

## Debugging Tips

### View Logs:
- Check Visual Studio **Output** window
- Look for debug messages starting with "Camera view", "Creating camera view", etc.
- API errors will show in exception messages

### Test API Directly:
You can test the Acumatica API directly using:
- Postman
- Browser (for GET requests)
- Acumatica's API documentation

Example API call:
```
GET https://acupower-demo.co.uk/openAI25R1/entity/Default/22.200.001/InventoryItem?$filter=InventoryID eq 'YOUR_ITEM_ID'
Headers: Authorization: Bearer YOUR_TOKEN
```

---

## Summary

The app provides:
✅ **Barcode Scanning** - Scan physical barcodes
✅ **Manual Entry** - Enter Inventory IDs manually  
✅ **Item Lookup** - Query Acumatica inventory
✅ **Item Details** - View item information
✅ **Offline Caching** - Store recent scans locally
✅ **AR Mode** - Basic AR overlay (experimental)

Ready to test! Start by logging in and scanning a valid Inventory ID from your Acumatica system.
