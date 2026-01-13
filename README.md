# 📦 Acumatica Inventory Scanner

A modern mobile barcode scanning application for **Acumatica ERP** inventory management. Built with .NET MAUI for cross-platform deployment on Android and iOS devices.

> **Developed by [AcuPower LTD](https://acupowererp.com)** - Acumatica Implementation Experts

## ✨ Features

- 📷 **Real-time Barcode Scanning** - Fast camera-based barcode detection
- 🔍 **Inventory Lookup** - Instantly search and view stock item details
- 🔐 **OAuth 2.0 Authentication** - Secure API access to Acumatica
- 💾 **Settings Persistence** - Save credentials for quick re-login
- 🎨 **Modern Dark Theme** - Industrial-inspired UI design
- 📱 **Cross-Platform** - Works on Android and iOS

---

## 📋 Prerequisites

Before using this app, ensure you have:

1. **Acumatica ERP Instance** (version 20.2 or later)
2. **User Account** with API access permissions
3. **OAuth Connected Application** configured in Acumatica

---

## 🔧 Acumatica Configuration

### Step 1: Create an OAuth Connected Application

The app requires OAuth credentials for secure API access. Follow these steps:

1. **Navigate to Connected Applications**
   - In Acumatica, go to: **System → Integration → Connected Applications**
   - Or use the screen ID: `SM303010`

   ![Navigate to Connected Applications](docs/images/step1-navigation.png)

2. **Create New Application**
   - Click the **"+"** button to create a new record
   - Fill in the following fields:

   | Field | Value |
   |-------|-------|
   | **Client Name** | `InventoryScanner` (or your preferred name) |
   | **Active** | ✅ Checked |
   | **Flow** | `Resource Owner Password Credentials` |
   | **Plug-In** | `No Plug-In` |

   ![Create OAuth App](docs/images/step2-create-app.png)

3. **Add a Client Secret**
   - Click **"Add Shared Secret"** in the Secrets tab
   - Enter a description (e.g., "Mobile App Secret")
   - **⚠️ IMPORTANT**: Copy and save the generated secret value immediately!
     - The secret is only shown once and cannot be retrieved later
   - Click **OK** to add the secret

   ![Add Secret](docs/images/step3-add-secret.png)

4. **Save and Note Credentials**
   - Press **Ctrl+S** to save
   - Note the following values:
     - **Client ID** (e.g., `C6ECE655-8FE3-5C1F-C7C8-3309E724BA61@Company`)
     - **Client Secret** (the value you copied in step 3)

   ![Save Credentials](docs/images/step4-credentials.png)

---

### Step 2: Find Your API Version

The app needs to know which API version your Acumatica instance supports:

1. **Check Available Endpoints**
   - Open your browser and navigate to:
     ```
     https://your-instance.acumatica.com/YourSite/entity
     ```
   - This returns a JSON list of available API endpoints

2. **Note the Version**
   - Look for entries with `"name": "Default"`
   - Common versions: `24.200.001`, `23.200.001`, `22.200.001`
   - Use the latest version available for your instance

---

## 📱 App Configuration

### First Launch Setup

1. **Open the App**
   - Launch the Acumatica Inventory Scanner on your device

2. **Enter Connection Details**

   | Field | Description | Example |
   |-------|-------------|---------|
   | **Instance URL** | Your Acumatica site URL | `https://mycompany.acumatica.com/MySite` |
   | **Username** | Your Acumatica username | `admin` |
   | **Password** | Your Acumatica password | `****` |
   | **Tenant** | Optional - leave empty for single-tenant | |
   | **API Version** | From Step 2 above | `24.200.001` |
   | **Client ID** | OAuth Client ID from Step 1 | `GUID@Company` |
   | **Client Secret** | OAuth Secret from Step 1 | `your-secret-key` |

3. **Test Connection**
   - Tap **"Test Connection"** to verify settings
   - A green success message confirms API access

4. **Save Settings**
   - Tap **"Save Settings"** to persist for future use
   - Enable **"Remember credentials"** for auto-fill on next launch

---

## 🎯 Using the Scanner

### Scanning Barcodes

1. **Point camera at barcode** - Position within the scanning frame
2. **Hold steady** - The red scanning line indicates detection area
3. **Automatic detection** - Barcode is recognized and searched automatically

### Search Results

After scanning, the app displays:
- **Item ID** - Acumatica Inventory ID
- **Description** - Item description
- **Availability** - Current stock levels
- **Warehouse Location** - Where the item is stored

---

## ⚙️ Settings

Access settings anytime via the **Settings** tab:

### Connection Settings
- Edit Acumatica URL and credentials
- Change API version
- Update OAuth credentials

### App Preferences
- **Play sound on scan** - Audio feedback
- **Vibrate on scan** - Haptic feedback
- **Auto-search after scan** - Immediate lookup
- **Remember credentials** - Save login info

### Reset
- **Reset All Settings** - Clear all saved data and start fresh

---

## 🔒 Security Notes

1. **OAuth is Required** - Cookie-based auth is not supported for REST API
2. **Credentials Storage** - Passwords are stored in platform-secure storage
3. **Token Expiration** - OAuth tokens expire after 1 hour; re-login if needed
4. **HTTPS Only** - Always use secure connections

---

## 🐛 Troubleshooting

### "401 Unauthorized" Error
- OAuth credentials may be incorrect or expired
- Verify Client ID and Secret in Acumatica
- Check that the Connected Application is **Active**

### "404 Not Found" Error
- API version mismatch
- The endpoint uses `StockItem`, not `InventoryItem`
- Try a different API version from the `/entity` endpoint

### "Connection Failed"
- Check network connectivity
- Verify the instance URL is correct
- Ensure no VPN or firewall is blocking access

### Scanner Not Detecting
- Ensure camera permissions are granted
- Hold device steady with good lighting
- Barcode must be within the scanning frame

---

## 📞 Support

**Created by AcuPower LTD**

- 🌐 Website: [acupowererp.com](https://acupowererp.com)
- 📧 Email: support@acupowererp.com

---

## 📄 License

This project is provided as a demonstration of Acumatica API integration for inventory management. See LICENSE file for details.

---

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2024 | Initial release with barcode scanning |
| 1.1.0 | 2025 | Added OAuth support, Settings page |
| 1.2.0 | 2026 | Modern UI redesign, persistent settings |
