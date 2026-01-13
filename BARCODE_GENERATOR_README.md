# Barcode Generator - WPF Application

## Overview
This WPF application generates various types of barcodes for testing and demonstration purposes related to Acumatica ERP integration and barcode scanning via API.

**Created by:** AcuPower  
**Website:** acupowererp.com  
**Purpose:** Demonstration of how to deal with barcode scanning via API

## Features

- **Multiple Barcode Formats:**
  - Code 128
  - EAN-13
  - EAN-8
  - UPC-A
  - UPC-E
  - QR Code
  - Data Matrix
  - PDF417
  - ITF
  - Codabar

- **Customizable Dimensions:**
  - Adjustable width and height
  - Real-time preview

- **Save Functionality:**
  - Save barcode as PNG, JPEG, or BMP
  - Custom file naming

## Usage

1. **Enter Text/Inventory ID:**
   - Type the text or Inventory ID you want to encode
   - Example: "AALEGO500" or any alphanumeric string

2. **Select Barcode Type:**
   - Choose from the dropdown menu
   - Code 128 is recommended for alphanumeric Inventory IDs

3. **Adjust Dimensions (Optional):**
   - Set width and height in pixels
   - Default: 300x150

4. **Generate:**
   - Click "Generate Barcode" button
   - Barcode appears in the preview area

5. **Save:**
   - Click "Save Barcode Image" to save to file
   - Choose format: PNG, JPEG, or BMP

## Use Cases

- **Testing Barcode Scanning:**
  - Generate barcodes with Inventory IDs from Acumatica
  - Test scanning with the Acumatica Inventory Scanner mobile app
  - Verify API integration

- **Demo Purposes:**
  - Create sample barcodes for demonstrations
  - Generate QR codes for quick access

## Technical Details

- **Framework:** .NET 8.0 WPF
- **Barcode Library:** ZXing.Net 0.16.11
- **Platform:** Windows

## Integration with Acumatica Inventory Scanner

This tool complements the Acumatica Inventory Scanner mobile app:
1. Generate barcodes with Inventory IDs from your Acumatica system
2. Print or display the barcodes
3. Scan them with the mobile app to test the integration
4. Verify that the app correctly queries Acumatica API

## Notes

- QR Codes can encode longer text strings
- Code 128 is ideal for Inventory IDs (alphanumeric)
- EAN/UPC formats require specific digit counts
- Some formats may not support all characters
