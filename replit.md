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
This project must be opened and built in **Unity 2019.1.5f1** (or a compatible 2019.x version). Replit does not include the Unity Editor — building must be done on a local machine or Unity Cloud Build.

Steps to build locally:
1. Open project in Unity 2019.1.5f1
2. Place `google-services.json` in `Assets/` (Android) or `GoogleService-Info.plist` in `Assets/` (iOS)
3. Set Bundle Identifier to `com.altomedia.idlerestaurant` (or your new ID)
4. File → Build Settings → Android → Build

## User Preferences
<!-- Add preferences here as needed -->
