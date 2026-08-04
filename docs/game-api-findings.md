# Game API Findings

## Game Version
Assembly-CSharp.dll from `C:\Program Files (x86)\Steam\steamapps\common\Supermarket Together`, inspected 2026-08-03 using ilspycmd 8.2.0.7535.

## Core Types

### ManagerBlackboard : NetworkBehaviour
Found via `Object.FindFirstObjectByType<ManagerBlackboard>()`. Lives on the same GameObject as `ProductListing` and `GameData`.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `GetProductsExistences(int productID)` | **private** | `int[] (3 elements)` | Must use reflection. Returns `[OnShelves, InStorage, InBoxes/Movement]` |
| `AddShoppingListProduct(int productID, float boxPrice)` | public | `void` | Instantiates a UI list item |
| `RemoveShoppingListProduct(int index)` | public | `void` | Removes by child index |
| `RemoveAllShoppingList()` | public | `void` | Clears entire shopping list |
| `BuyCargo()` | public | `void` | Buys ENTIRE shopping list, calls CmdAddProductToSpawnList per item, deducts total via CmdAlterFunds |
| `shoppingListParent` | public | `GameObject` | Parent of shopping list UI items |
| `shoppingTotalCharge` | public | `float` | Total cost of current shopping list |
| `tabsOBJ` | public | `GameObject` | Ordering interface tabs container |
| `dummyArrayExistences` | public | `GameObject[]` | [0]=shelves parent, [1]=storage parent, [2]=boxes parent |
| `isSpawning` | private | `bool` | Cargo spawn in progress |
| `CmdAddProductToSpawnList(int)` | private (Command) | `void` | Mirror network command |

### ProductListing : NetworkBehaviour
Singleton via `ProductListing.Instance`.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `Instance` | public static | `ProductListing` | Singleton |
| `productsData` | public | `ProductData[]` | Full product catalog |
| `availableProducts` | public | `List<int>` | Currently available (unlocked) product IDs |
| `unlockedProductTiers` | public [SyncVar] | `bool[]` | Indexed by tier |
| `tierInflation` | public [SyncVar] | `float[]` | Price multipliers by tier |
| `tiers` | public | `string[]` | Tier ranges as "start-end" strings |
| `productGroups` | public | `int[]` | Group index per tier |

### ProductListing.ProductData (nested class)

| Field | Type | Notes |
|-------|------|-------|
| `productID` | int | |
| `productPrefab` | GameObject | |
| `productSprite` | Sprite | |
| `maxItemsPerBox` | int | Default 25 |
| `basePricePerUnit` | float | Default 1.0 |
| `productBrand` | string | |
| `productTier` | int | |
| `isStackable` | bool | |
| `productContainerClass` | int | |
| `boxClass` | int | |

### GameData : NetworkBehaviour
Singleton via `GameData.Instance`.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `Instance` | public static | `GameData` | Singleton |
| `gameFunds` | public [SyncVar] | `float` | Player money, hook="UpdateUIFunds" |
| `CmdAlterFunds(float)` | public [Command] | `void` | Adds/subtracts from gameFunds. Clamped to [0, 2.14e9] |
| `CmdmoneySpentOnProducts(float)` | public [Command] | `void` | Tracks daily spending for reports |
| `isSupermarketOpen` | public [SyncVar] | `bool` | |

### GameCanvas : MonoBehaviour
Singleton via `GameCanvas.Instance`.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `Instance` | public static | `GameCanvas` | Singleton |
| `CreateCanvasNotification(string hash)` | public | `void` | Looks up localization key. Bypassed with backtick prefix via Harmony patch |
| `CreateImportantNotification(string hash)` | public | `void` | Similar, uses different prefab |
| `inCooldown` | private | `bool` | Set true for 0.5s after each notification |

### LocalizationManager
Singleton via `LocalizationManager.instance`.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `instance` | public static | `LocalizationManager` | Singleton |
| `GetLocalizationString(string key)` | public | `string` | Product names: "product" + id |

### NPC_Manager
Singleton via `NPC_Manager.Instance`. Used internally by GetProductsExistences to count employee-carried products.

### Mirror Networking
- `NetworkServer.active` returns `true` when the local instance is the server (host). Used for authority detection.

### FirstPersonController (StarterAssets)
Fully qualified name: `StarterAssets.FirstPersonController`. Controls player movement and camera rotation.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `enabled` | public | `bool` (inherited from MonoBehaviour) | Disabling freezes all player input (movement + camera) |

**Important**: This class has NO static `Instance` property or field. Must be found at runtime via `Object.FindObjectOfType(typeof(StarterAssets.FirstPersonController))`. Attempting `AccessTools.Property("Instance")` fails silently with HarmonyX warnings. The game's own menus likely disable this component to freeze player input.

## Stock Value Mapping (Red/Green/Yellow)

Confirmed from `ManagerBlackboard.FixedUpdate()`:

```
int[] productsExistences = GetProductsExistences(productID);
// [0] -> "InShelvesBCK/ShelvesQuantity" = On Shelves (displayed in RED area)
// [1] -> "InStorageBCK/StorageQuantity" = In Storage (displayed in GREEN area)
// [2] -> "InBoxesBCK/BoxesQuantity"     = In Boxes/Movement (displayed in YELLOW area)
```

- **Index 0 = OnShelves**: Products currently on display shelves. Iterated from `dummyArrayExistences[0]` children using `Data_Container.productInfoArray`.
- **Index 1 = InStorage**: Products in storage containers. Iterated from `dummyArrayExistences[1]` children using `Data_Container.productInfoArray`.
- **Index 2 = InBoxes/Movement**: Sum of:
  - Boxes on the ground: `dummyArrayExistences[2]` children via `BoxData.productID` / `BoxData.numberOfProducts`
  - Employee-carried: `NPC_Manager.Instance.employeeParentOBJ` children via `NPC_Info.boxProductID` / `NPC_Info.boxNumberOfProducts`
  - Player-carried: `CustomNetworkManager.GamePlayers` via `PlayerSyncCharacter.syncedProductID` / `PlayerSyncCharacter.syncedNumberOfProducts`

## Box Price Formula

Confirmed from `ManagerBlackboard.CreateUIShopItem`:

```csharp
float pricePerUnit = basePricePerUnit * tierInflation[productTier];
pricePerUnit = Mathf.Round(pricePerUnit * 100f) / 100f;
float boxPrice = pricePerUnit * maxItemsPerBox;
boxPrice = Mathf.Round(boxPrice * 100f) / 100f;
```

## Purchase Mechanism

**No per-product purchase method exists.** `BuyCargo()` buys the entire shopping list:

1. Validates `shoppingListParent.childCount > 0` and `shoppingTotalCharge > 0`
2. Validates `gameFunds >= shoppingTotalCharge`
3. For each shopping list item: `CmdAddProductToSpawnList(productID)` (Mirror Command -> server)
4. Destroys all shopping list UI items
5. `CmdAlterFunds(-shoppingTotalCharge)` to deduct money
6. `CmdmoneySpentOnProducts(shoppingTotalCharge)` for daily reports

**Automation strategy**: Add one product to empty shopping list, wait for price calculation, call BuyCargo. Skip if manual entries exist.

## Shopping List Item Structure

Each shopping list item has:
- `InteractableData.thisSkillIndex` = productID
- `BoxPrice` child with `TextMeshProUGUI.text` = " $XX.XX"

## Data_Container

Represents shelves and storage units in the game world.

| Member | Visibility | Type/Signature | Notes |
|--------|-----------|---------------|-------|
| `productInfoArray` | public | `int[]` | Products stored on this container |
| `containerClass` | public | `int` | Type identifier. `69` = storage shelf |
| `GetStorageBox(int)` | public | `void` | Places a product on the shelf |

**Important**: `Data_Container` has **no** `OnDestroy` method. Attempting to patch `OnDestroy` via Harmony causes all patches in the class to fail silently. Use `Physics.OverlapSphere()` at known positions to detect if a container still exists.

## Save System

The game uses **Easy Save 3 (ES3)** with AES encryption (password: `g#asojrtg@omos)^yq`).

### Key Save Methods

| Class | Method | Purpose |
|-------|--------|---------|
| `GameData` | `SaveFromQuitButton()` | Public. Triggers full autosave on quit |
| `GameData` | `Autosave(bool)` | Private coroutine. Full mid-day save (7 steps) |
| `GameData` | `AutosaveControl()` | Private coroutine. Timer loop, interval = `autosaveFactor` minutes |
| `SaveBehaviour` | `SavePersistentValues()` | Public. Core values (funds, upgrades, pricing) |
| `NetworkSpawner` | `SaveProps(bool)` | Public. All placed furniture/shelves/props |

### Save Files

| File | Purpose |
|------|---------|
| `StoreFile{N}.es3` | Main store save slots |
| `StoreFile{N}Day{D}.es3` | End-of-day snapshot backups |
| `Autosaves/Autosave001.es3` | Mid-session crash-recovery autosave |
| `Settings/GameOptions.es3` | Game settings (language, autosave interval, etc.) |

Save path: `%USERPROFILE%\AppData\LocalLow\DDTNL\Supermarket Together\`

## Notification Localization Bypass

`GameCanvas.CreateCanvasNotification` passes the hash through `LocalizationManager.GetLocalizationString`. We patch this with Harmony to check for backtick prefix: if present, return the string directly (minus the backtick) without localization lookup.
