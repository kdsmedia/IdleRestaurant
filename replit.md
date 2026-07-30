# Idle Restaurant Game (Unity)

## Project Overview

A Unity-based **idle restaurant mobile game** written in C#. Players manage a restaurant with kitchens, waiters, transporters, elevators, managers, and boosts — typical idle/incremental gameplay loop.

### Stack
- **Engine:** Unity (C#)
- **Animations:** Spine (skeletal 2D animation)
- **Backend / Auth:** Firebase
- **Ads:** Google Mobile Ads + Unity Ads
- **Analytics / Tracking:** Firebase Analytics
- **Notifications:** Local push notifications
- **Data:** JSON via MiniJSON, Parse (legacy backend)

### Key Source Files (`Assets/Scripts/`)

| File | Purpose |
|---|---|
| `GameManager.cs` | Central game loop and initialization |
| `GameProcess.cs` | Core game process logic |
| `DataManager.cs` | Save/load and data persistence |
| `Database.cs` | Firebase database integration |
| `RestaurantController.cs` | Restaurant-level management |
| `KitchenController.cs` | Kitchen unit logic |
| `WaiterController.cs` | Waiter NPC behaviour |
| `TransporterController.cs` | Transporter NPC behaviour |
| `ElevatorController.cs` | Elevator system |
| `ManagerController.cs` | Manager hire/skill system |
| `BoostManager.cs` / `BoostController.cs` | Boost system |
| `AdsControl.cs` | Ad integration (rewarded / interstitial) |
| `ShopManager.cs` | In-app purchase shop |
| `Tracking.cs` | Analytics events |
| `Singleton.cs` | Generic singleton base class |
| `ObjectPool.cs` | Object pooling utility |
| `Constant.cs` | Game-wide constants |
| `Configuration.cs` | Runtime configuration |

### Asset Folders

| Folder | Contents |
|---|---|
| `Assets/Scripts/` | All C# game logic |
| `Assets/Prefab/` | Unity prefabs (Kitchen, Waiter, Transporter, etc.) |
| `Assets/AnimationClip/` | Animation clips |
| `Assets/AnimatorController/` | Animator state machines |
| `Assets/MonoBehaviour/` | Spine skeleton data & atlases |
| `Assets/Images/` | Sprites and textures |
| `Assets/Sounds/` | Audio assets |
| `Assets/Firebase/` | Firebase SDK |
| `Assets/GoogleMobileAds/` | AdMob SDK |
| `Assets/Resources/` | Runtime-loaded assets (JSON configs, prefabs) |

## Important Notes

- **Cannot be built/run on Replit** — Unity projects require the Unity Editor or Unity Cloud Build.
- To build: open this project folder in **Unity 2019+** (check `ProjectSettings/ProjectVersion.txt` for the exact version).
- Firebase config files: `Assets/google-services.json` (Android) and `Assets/GoogleService-Info.plist` (iOS) are present.

## User Preferences

- **Push on every change:** After every edit or file change, immediately commit and push to the GitHub repository — no exceptions.
