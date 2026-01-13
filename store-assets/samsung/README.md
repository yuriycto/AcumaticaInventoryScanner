# Samsung Galaxy Store Submission Assets

This folder contains all files needed to submit the Acumatica Inventory Scanner to Samsung Galaxy Store (seller.samsungapps.com).

## 📦 Files Included

### APK File
| File | Size | Description |
|------|------|-------------|
| `AcumaticaInventoryScanner-v1.0.0.apk` | ~30 MB | Signed release APK |

### Documentation
| File | Description |
|------|-------------|
| `store-listing.md` | Full store listing text (descriptions, keywords, etc.) |
| `PRIVACY_POLICY.md` | Privacy policy for app submission |
| `generate_icons.py` | Script to generate icons from SVG |

---

## 🎨 Required Assets (You Need to Create)

### 1. App Icon (REQUIRED)
- **Size:** 512x512 PNG
- **Format:** PNG with transparency
- **Source:** Use `generate_icons.py` or manually export from:
  ```
  AcumaticaInventoryScanner/Resources/Images/app_icon.svg
  ```

To generate icons automatically:
```bash
pip install cairosvg pillow
python generate_icons.py
```

Or use online converter: https://cloudconvert.com/svg-to-png

### 2. Screenshots (REQUIRED - minimum 4)

Take screenshots on a Samsung device or emulator:

| Screenshot | Suggested Content |
|------------|------------------|
| 1 | Login page with fields |
| 2 | Main scanner screen with camera viewfinder |
| 3 | Item details page showing inventory info |
| 4 | Settings page |

**Sizes:**
- Phone: 1080x1920 or 1440x2560 (portrait)
- Tablet: 2560x1600 (landscape, optional)

**Tips:**
- Use Samsung Galaxy emulator in Android Studio
- Or use a real Samsung device
- Enable "Show taps" in Developer Options for better demos

### 3. Feature Graphic (OPTIONAL but recommended)
- **Size:** 1024x500 PNG or JPEG
- Promotional banner shown in store
- Should include app name and key visual

---

## 📝 Submission Checklist

### Samsung Seller Portal Setup
1. [ ] Create account at seller.samsungapps.com
2. [ ] Complete seller profile
3. [ ] Verify email and phone

### App Submission
1. [ ] Upload APK file
2. [ ] Upload app icon (512x512)
3. [ ] Upload at least 4 screenshots
4. [ ] Fill in app name: "Acumatica Inventory Scanner"
5. [ ] Select category: Business > Productivity
6. [ ] Copy short description from `store-listing.md`
7. [ ] Copy full description from `store-listing.md`
8. [ ] Add privacy policy URL (host `PRIVACY_POLICY.md` online)
9. [ ] Complete age rating questionnaire
10. [ ] Set price: Free
11. [ ] Select countries for distribution
12. [ ] Submit for review

---

## 🔗 Privacy Policy Hosting

You need to host the privacy policy online. Options:

1. **GitHub Pages** (Free)
   - Push to your repo, enable Pages
   - URL: `https://yourusername.github.io/AcumaticaInventoryScanner/PRIVACY_POLICY.html`

2. **Your website**
   - Upload to: `https://acupowererp.com/privacy/inventory-scanner`

3. **Google Docs** (Quick option)
   - Upload as public Google Doc
   - Use the shareable link

---

## ⚙️ App Details for Samsung Store

| Field | Value |
|-------|-------|
| App Name | Acumatica Inventory Scanner |
| Package | com.acupower.acumaticainventoryscanner |
| Version | 1.0.0 |
| Min Android | 5.0 (API 21) |
| Target Android | 14 (API 34) |
| Category | Business |
| Price | Free |
| In-App Purchases | No |
| Ads | No |
| Age Rating | Everyone |

---

## 📱 Permissions Explanation

When prompted by Samsung, explain each permission:

| Permission | Explanation |
|------------|-------------|
| CAMERA | Required for scanning barcodes on inventory items |
| INTERNET | Required to connect to user's Acumatica ERP system |
| ACCESS_NETWORK_STATE | To check if device has internet connectivity |

---

## 📞 Support Information

| Field | Value |
|-------|-------|
| Developer | AcuPower LTD |
| Website | https://acupowererp.com |
| Support Email | support@acupowererp.com |
| Privacy Policy | (your hosted URL) |

---

## 🕐 Review Timeline

Samsung Galaxy Store review typically takes:
- **Initial review:** 3-5 business days
- **Updates:** 1-3 business days

You'll receive email notifications about the review status.
