# Architecture

## Overview

OrderAndOrganize follows a layered architecture with clear separation between game API access, business logic, and UI.

```
Plugin.cs (Entry Point)
├── Configuration/ModConfiguration.cs   - BepInEx config bindings
├── Game/                               - Adapter layer (game API wrappers)
│   ├── GameInventoryAdapter            - GetProductsExistences (reflection)
│   ├── GameProductCatalogAdapter       - ProductListing / product data
│   ├── GameShoppingListAdapter         - Shopping list inspection & manipulation
│   ├── GamePurchaseAdapter             - BuyCargo flow orchestration
│   ├── GameMoneyAdapter                - GameData.gameFunds reading
│   ├── GameNotificationAdapter         - Canvas notifications with rate limiting
│   ├── GameAuthorityAdapter            - NetworkServer.active host detection
│   ├── GameUiAdapter                   - Ordering UI state queries
│   └── GameCompatibilityException      - Structured error type
├── Models/                             - Data structures (no Unity dependency)
│   ├── ProductStockSnapshot            - Full stock state for one product
│   ├── ProductOrderCandidate           - Filtered candidate for ordering
│   ├── PendingAutomatedOrder           - In-memory purchase tracking record
│   ├── AutomationCycleResult           - Summary of one automation cycle
│   └── PurchaseResult                  - Single purchase outcome
├── Services/                           - Business logic
│   ├── InventoryScanner                - Scans all products, builds snapshots
│   ├── RestockPlanner                  - Filters & sorts candidates (pure logic)
│   ├── ShoppingListService             - Adds candidates to shopping list
│   ├── PurchaseService                 - Executes purchases with money protection
│   ├── PendingOrderTracker             - Deduplication via in-memory order records
│   ├── AutomationController            - Hotkey toggle, coroutine timer, lifecycle
│   ├── NotificationService             - User-facing notification formatting
│   └── CoroutineRunner                 - Persistent MonoBehaviour for coroutines
├── UI/
│   ├── OrderButtonController    - "Add Low Stock" button in ordering UI
│   ├── ConfigWindow                    - IMGUI in-game config panel (F7)
│   └── CategoryPickerUI               - Scroll-wheel category picker + floating labels
├── Patches/CategoryStoragePatches      - Harmony patches for category shelf enforcement
└── Diagnostics/GameApiDiagnostics      - Verbose logging & API verification
```

## Key Design Decisions

### Adapter Pattern
All game API access is centralized in `Game/` adapters. Each adapter wraps exactly one game subsystem (inventory, money, notifications, etc.). If a game update changes an API, only one adapter needs updating.

### Reflection for Private Methods
`ManagerBlackboard.GetProductsExistences` is private. The `GameInventoryAdapter` resolves it via reflection once on first use, caches the `MethodInfo`, and validates the return type. Failure to resolve logs an error and disables scanning.

### Localization Bypass via Harmony
Game notifications pass through `LocalizationManager.GetLocalizationString`. A Harmony prefix patch checks for a backtick (`\``) prefix and returns the raw string, bypassing localization lookup. This lets us display arbitrary mod messages in the native notification system.

### Coroutine Orchestration
`BuyCargo()` relies on `CalculateShoppingListTotal`, which is a coroutine that runs at end-of-frame. The `GamePurchaseAdapter` yields between adding a product and calling BuyCargo to ensure the shopping total is calculated.

### Pending Order Deduplication
After each automated purchase, `PendingOrderTracker` records the product ID, timestamp, and expected stock. On subsequent scans, it checks if `InMovement` has increased (reflecting delivery). Orders time out after a configurable period to prevent permanent blocking.

## Data Flow

### Manual Mode
1. User clicks "Add Low Stock" button
2. `InventoryScanner.ScanAll()` builds `ProductStockSnapshot` list
3. `RestockPlanner.PlanManualRestock()` filters by CombinedStock < threshold, excludes already-listed
4. `ShoppingListService.AddCandidatesToShoppingList()` calls `AddShoppingListProduct` for each
5. Notification shows count added/skipped

### Automatic Mode
1. `AutomationController` coroutine fires every N seconds
2. Checks authority (host only), store loaded, shopping list empty
3. `InventoryScanner.ScanAll()` + `PendingOrderTracker.Reconcile()`
4. `RestockPlanner.PlanAutoRestock()` uses EffectiveCombinedStock, excludes pending
5. `PurchaseService.ExecutePurchases()` processes candidates one-by-one:
   - Verify funds (including cash reserve)
   - Verify shopping list still empty
   - Add product → wait frame → BuyCargo → record pending order
6. Notification shows cycle summary

## Category Storage Shelves

Storage shelves (containerClass 69) can be tagged with a product category (ordering-tab group). This restricts what can be placed on the shelf and makes employees prioritize category-matched storage.

### Category Picker (Scroll-Wheel UI)
The picker uses a keyboard/scroll-wheel interaction model to avoid IMGUI click-through issues:
- **G key** opens the picker while looking at a storage shelf
- **Mouse wheel / Arrow keys** browse categories
- **Enter** applies the selected category
- **G** cancels/closes, **Backspace** clears the assignment
- `Input.ResetInputAxes()` blocks all game input while the picker is open
- `FirstPersonController` is disabled via `FindObjectOfType` (no static Instance exists) to freeze camera/movement

### Floating Labels
Assigned categories are shown as color-coded labels floating above tagged shelves. Toggleable via H key or the config window. Labels scale with distance and fade at `LabelMaxDistance` (20 units).

### Persistence
Assignments are stored as `{positionKey: groupIndex}` in `BepInEx/config/OrderAndOrganize_CategoryShelves.json`. Position keys use grid-snapped format `"x,y,z"` (integers, 0.1 unit precision). On load, keys are validated against the `"int,int,int"` format and corrupt entries are purged.

### Harmony Patches
- `Data_Container.GetStorageBox` prefix: blocks player placement of wrong-category products
- `NPC_Manager.GetFreeStorageContainer` prefix: employees prefer category-matched storage, then untagged, skipping wrong-category shelves

### Stale Assignment Cleanup
Deleted shelves leave orphaned entries in the JSON file. A periodic `PurgeStaleAssignments()` method uses `Physics.OverlapSphere()` to verify each assignment's shelf still exists. Protected by three safety layers (see below).

### Backup and Recovery
Before any purge removes entries, `CreateBackup()` copies the current JSON to `OrderAndOrganize_CategoryShelves.json.backup`. On load, if the main file has 0 entries but a backup exists, `TryRestoreFromBackup()` automatically recovers the data.

## Safety Mechanisms

- **Money protection**: Never spends below configurable cash reserve
- **Shopping list safety**: Auto-mode halts if manual entries are detected
- **Reentrancy guard**: `_isRunningCycle` prevents overlapping automation cycles
- **Host-only automation**: Purchases only execute when `NetworkServer.active`
- **Scene cleanup**: Pending orders cleared on scene unload
- **No save modification**: All actions use native game APIs
- **No direct money editing**: Uses CmdAlterFunds through BuyCargo
- **Category data protection** (three layers):
  1. **Scene guard**: Purge only runs when `GameData.Instance` exists and 120 seconds have passed since scene load
  2. **50% threshold**: Purge aborts if more than half of all assignments would be removed
  3. **Backup before purge**: Automatic backup created before any entries are deleted, with auto-restore on load
