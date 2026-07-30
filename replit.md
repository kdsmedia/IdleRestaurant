# Idle Restaurant Game (Unity)

## Project Overview
A Unity mobile idle restaurant game for Android, originally developed by Altomedia. Players manage a restaurant, earn in-game currency, and progress through upgrades.

**Unity Version:** 2019.1.5f1  
**Target Platform:** Android (primary), iOS (assets present)  
**Bundle ID:** `com.altomedia.idlerestaurant`  
**Backend:** Firebase Analytics + Parse  
**Monetization:** Google AdMob (interstitial, banner, rewarded) + Unity IAP + Unity Ads

## Key Third-Party SDKs
| SDK | Version | Location |
|-----|---------|----------|
| Firebase SDK | 6.0.0 | `Assets/Firebase/` |
| Google Mobile Ads | — | `Assets/GoogleMobileAds/` |
| Unity IAP | — | `Assets/Plugins/` + `Assets/Resources/BillingMode.json` |
| Unity Ads | — | `Assets/UnityAds/` |
| Spine (animation) | — | `Assets/Spine/` |
| Parse | — | `Assets/Parse/` |

## Build Readiness Assessment

### ✅ What's in place
- Full C# game logic under `Assets/Scripts/`
- AdMob IDs hardcoded in `Assets/Scripts/AdsControl.cs` (Android)
- Firebase config exists in `Assets/StreamingAssets/google-services-desktop.json` and `Assets/Plugins/Android/Firebase/res/values/google-services.xml`
- Unity IAP billing config at `Assets/Resources/BillingMode.json`
- Spine skeleton/atlas assets for character animations

### ⚠️ Items to address before building
1. **`google-services.json` missing from project root** — required for standard Firebase Android build. Copy or recreate from the Firebase console for project `altomedia-8f793`.
2. **`GoogleService-Info.plist` missing from root** — needed for iOS builds.
3. **Deprecated `WWW` class** in `Assets/Scripts/GetFreeCoin.cs` — uses old Unity HTTP API and a hardcoded external URL (`http://mega.ikame.vn/index.php?index=get_time`). Should be replaced with `UnityWebRequest` and the URL verified/updated.
4. **Hardcoded AdMob IDs** in `AdsControl.cs` — fine for the original app; update if rebranding or creating a new AdMob property.

## Running / Building

### Build Otomatis via GitHub Actions
Setiap push ke branch `main` akan otomatis build APK Android dan membuat GitHub Release.
Workflow ada di `.github/workflows/build.yml`.

**Setup sekali (wajib sebelum build otomatis bisa berjalan):**

Game-ci v4 memerlukan lisensi Unity dari komputer lokal — tidak bisa lewat GitHub Actions.

1. **Aktifkan lisensi di Unity Hub (komputer lokal):**
   - Install [Unity Hub](https://unity.com/download)
   - Login dengan akun Unity Anda
   - Buka **Preferences → Licenses → Add → Get a free personal license**

2. **Temukan file `.ulf`:**
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - Mac: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`

3. **Tambahkan 3 secrets di GitHub repository:**
   Buka **Settings → Secrets and variables → Actions → New repository secret**

   | Secret Name | Isi |
   |---|---|
   | `UNITY_LICENSE` | Seluruh isi file `.ulf` (buka dengan text editor, copy semua) |
   | `UNITY_EMAIL` | Email akun Unity Anda |
   | `UNITY_PASSWORD` | Password akun Unity Anda |

Setelah 3 secrets tersimpan, push ke `main` → APK otomatis tersedia di tab **Releases**.

### Build Lokal
1. Open project di Unity 2019.1.5f1
2. Pastikan `Assets/google-services.json` ada (Android)
3. Set Bundle Identifier: `com.altomedia.idlerestaurant`
4. File → Build Settings → Android → Build

## User Preferences
- Setiap perubahan atau pengeditan wajib langsung di-commit dan di-push ke repository GitHub.
