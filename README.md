# Order & Organize - BepInEx 5 Mod for Supermarket Together

Automatically tracks product stock levels and restocks low-inventory items in Supermarket Together.

## Features

### Manual Mode ("Add Low Stock" Button)
A button on the ordering interface that scans all products and adds one box of each product below the configured threshold to the shopping list. It:
- Skips products already on your shopping list
- Skips locked/unorderable products
- Prioritizes by lowest stock, then greatest shortage, then product ID
- Never buys anything automatically -- only adds to your list
- Shows a notification with the count of added and skipped products

### Automatic Mode (Optional)
An optional system that periodically purchases products below the threshold without manual intervention. It:
- Is **disabled by default** -- enable via hotkey (F8) or config
- Only runs when you are the host (singleplayer or multiplayer host)
- Only purchases when the shopping list is empty (never interferes with manual orders)
- Respects a configurable cash reserve (never spends below your reserve)
- Tracks pending orders in memory to prevent duplicate purchases
- Shows notifications with box count and total spend per cycle

### Category Storage Shelves
Tag storage shelves with product categories to organize your backroom:
- Press **G** while looking at a storage shelf to open the category picker
- Scroll or use arrow keys to browse categories, **Enter** to apply, **Backspace** to clear, **G** to cancel
- Tagged shelves only accept products from that category (player placement is blocked otherwise)
- Employees prioritize category-matched storage when putting away leftovers
- Color-coded floating labels above tagged shelves (toggle with **H** key)
- Assignments persist across sessions and are backed up automatically

### In-Game Config Window (F7)
A scrollable configuration panel for adjusting all mod settings in real-time without editing config files.

## Stock Value Meanings

The ordering interface displays three stock values per product:

| Color | Meaning | Source |
|-------|---------|--------|
| Red area | **On Shelves** | Products displayed on store shelves |
| Green area | **In Storage** | Products in storage containers |
| Yellow area | **In Boxes/Movement** | Boxes on ground + carried by employees + carried by players |

**CombinedStock** = OnShelves + InStorage + InBoxes/Movement

Products with CombinedStock below the threshold (default: 40 units) qualify for restocking.

## Installation

### Prerequisites
- [Supermarket Together](https://store.steampowered.com/app/2590510/Supermarket_Together/) installed
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) installed in the game directory

### Install the Mod
1. Download `OrderAndOrganize.dll` from the releases (or build from source)
2. Copy to `<GameDir>\BepInEx\plugins\OrderAndOrganize\OrderAndOrganize.dll`
3. Launch the game

## Building from Source

### Requirements
- .NET SDK 8.0
- Supermarket Together with BepInEx 5 installed

### Setup
1. Clone this repository
2. Copy `Directory.Build.props.example` to `Directory.Build.props`
3. Edit `Directory.Build.props` and set `GameDir` to your game installation path

### Build
```powershell
.\scripts\build.ps1
```

### Deploy
```powershell
.\scripts\deploy.ps1
```
This builds and copies the DLL to the game's plugin directory. The game must not be running.

### Backup Saves
```powershell
.\scripts\backup-save.ps1
```
Creates a timestamped backup of your save data in `Documents\Supermarket Together Backups\`.

### Collect Logs
```powershell
.\scripts\collect-logs.ps1
```
Copies BepInEx logs to a timestamped file in the `logs/` directory with Order & Organize entries highlighted.

## Configuration

After first launch, a config file is created at `<GameDir>\BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg`.

| Setting | Default | Description |
|---------|---------|-------------|
| **General/Enabled** | true | Enable or disable the mod |
| **General/ThresholdUnits** | 40 | Stock level below which products qualify for restocking |
| **General/ButtonText** | "Add Low Stock" | Text on the manual restock button |
| **General/VerboseLogging** | false | Log per-product decisions for debugging |
| **General/ConfigWindowHotkey** | F7 | Key to toggle the in-game config window |
| **Automation/AutoOrderAtStartup** | false | Start automation enabled when game loads |
| **Automation/ToggleHotkey** | F8 | Key to toggle automatic ordering |
| **Automation/ScanIntervalSeconds** | 10 | Seconds between automatic scans (2-300) |
| **Automation/CashReserve** | 0 | Minimum money to keep in reserve |
| **Automation/PendingOrderTimeoutSeconds** | 120 | Seconds before stale pending orders are re-evaluated |
| **Automation/ShowNotifications** | true | Show in-game notifications for automation events |
| **CategoryShelves/Enabled** | true | Enable the category storage shelf feature |
| **CategoryShelves/Hotkey** | G | Key to open the category picker on a storage shelf |
| **CategoryShelves/LabelsVisible** | true | Show floating category labels above tagged shelves |
| **CategoryShelves/LabelsToggleHotkey** | H | Key to toggle floating labels on/off |

## Safety & Protection

- **No save file modification**: All actions use the game's native APIs
- **No direct money editing**: Money is deducted through the game's `BuyCargo()` / `CmdAlterFunds` flow
- **Shopping list safety**: Automation halts if manual items are detected on the shopping list
- **Cash reserve**: Configure a minimum balance that automation will never spend below
- **Pending order tracking**: In-memory tracking prevents duplicate orders before the game reflects deliveries
- **Host-only automation**: Automatic purchases only execute when you are the host/server
- **Scene cleanup**: All pending order records are cleared when leaving a game session
- **Category data protection**: Three-layer safety for shelf assignments -- scene guard (120s grace period), 50% purge threshold, and automatic backup/restore

## Uninstallation

1. Delete `<GameDir>\BepInEx\plugins\OrderAndOrganize\`
2. Optionally delete `<GameDir>\BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg`
3. Optionally delete `<GameDir>\BepInEx\config\OrderAndOrganize_CategoryShelves.json` (and `.backup`)

## Multiplayer

This mod is designed for **singleplayer and host use**. When playing as a multiplayer host, all features work normally. Non-host clients can use the manual restock button and category picker, but purchases are host-authoritative and category assignments are local-only. Automatic ordering is automatically disabled for non-host players.

## Compatibility

- **Target Framework**: .NET Framework 4.7.2
- **BepInEx**: 5.x
- **Game API**: Uses reflection and Harmony patches to access game internals
- **Risk**: Game updates may break the mod if internal APIs change. The adapter pattern isolates all game access for easier updates.

## Distribution

### Thunderstore
```powershell
.\scripts\package-thunderstore.ps1
```
Creates a ready-to-upload zip in `dist/`. Upload at [thunderstore.io/package/create](https://thunderstore.io/package/create/).

### NexusMods
Upload the DLL from `src\OrderAndOrganize\bin\Release\OrderAndOrganize.dll` along with the README through the NexusMods web interface.

## Project Structure

```
OrderAndOrganize/
├── src/OrderAndOrganize/           # Main mod source
│   ├── Plugin.cs               # BepInEx entry point + Harmony patches
│   ├── Configuration/          # BepInEx config bindings
│   ├── Game/                   # Game API adapters (compatibility layer)
│   ├── Models/                 # Data models (no Unity dependency)
│   ├── Services/               # Business logic
│   ├── UI/                     # Ordering UI, config window, category picker
│   └── Diagnostics/            # Verbose logging & API verification
├── tests/OrderAndOrganize.Tests/   # Unit tests (xUnit)
├── scripts/                    # Build, deploy, backup, log collection
├── docs/                       # Architecture, API findings, test matrix
└── README.md
```

## Running Tests

```powershell
dotnet test tests\OrderAndOrganize.Tests\OrderAndOrganize.Tests.csproj
```

45 unit tests cover threshold logic, sorting, cash reserve calculations, pending order tracking, and boundary conditions.
