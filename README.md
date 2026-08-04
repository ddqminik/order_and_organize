# Order & Organize

**A BepInEx 5 mod for [Supermarket Together](https://store.steampowered.com/app/2590510/Supermarket_Together/) that automates inventory restocking and lets you organize your storage shelves by product category.**

**Version:** 0.5.0
**Author:** ddqminik
**Requires:** [BepInEx 5](https://github.com/BepInEx/BepInEx/releases)

---

## What Does This Mod Do?

Order & Organize adds three major features to Supermarket Together:

1. **Smart Restock Button** -- A one-click button that scans your entire inventory and adds low-stock products to your shopping list.
2. **Automatic Ordering** -- An optional system that periodically checks stock levels and purchases products for you, completely hands-free.
3. **Category Storage Shelves** -- Tag your storage shelves with product categories (Dairy, Drinks, Snacks, etc.) so only matching products can be placed there, and employees prioritize the right shelf when putting items away.

All features work through the game's native systems -- the mod never edits your save file or manipulates your money directly.

---

## Features in Detail

### Smart Restock Button

When you open the ordering computer in your store, you'll see an **"Add Low Stock"** button. Clicking it:

- Scans every product in your store
- Finds products where your total stock (shelves + storage + boxes) is below a configurable threshold (default: 40 units)
- Adds one box of each low-stock product to your shopping list
- Skips products that are already on your shopping list
- Skips locked or unorderable products
- Prioritizes the most understocked products first
- Shows a notification telling you how many products were added and how many were skipped

This is purely manual -- it only adds items to your shopping list. You still review and confirm the purchase yourself.

### Automatic Ordering

For a fully hands-free experience, you can enable automatic ordering. This is **disabled by default** and must be turned on intentionally.

**How to use it:**

1. Press **F8** (default hotkey) to toggle automatic ordering on or off
2. When enabled, the mod scans your inventory every 10 seconds (configurable)
3. If it finds products below the threshold, it purchases them automatically
4. A notification appears showing how many boxes were ordered and the total cost

**Safety features of automatic ordering:**

- **Cash reserve**: Set a minimum balance (e.g. $5,000) that the mod will never spend below. You'll always have money for rent, licenses, and other expenses.
- **No interference with manual orders**: If you have items on your shopping list, automation pauses entirely until the list is clear. Your manual orders are never touched.
- **Duplicate protection**: The mod tracks what it has already ordered and won't re-order the same product until the previous delivery arrives or a timeout expires.
- **Host-only**: In multiplayer, automatic ordering only works for the host. This prevents conflicts when multiple players have the mod installed.

### Category Storage Shelves

Tired of employees dumping products on random storage shelves? This feature lets you designate shelves for specific product categories.

**How to use it:**

1. Look directly at a storage shelf in your backroom
2. Press **G** (default hotkey) to open the category picker
3. Use the **scroll wheel** or **arrow keys** to browse available categories (these match the tabs in the ordering interface -- Dairy, Drinks, Fruits & Vegetables, etc.)
4. Press **Enter** to assign the selected category
5. Press **Backspace** to remove an existing category assignment
6. Press **G** again to cancel without making changes

**What happens after you assign a category:**

- A color-coded floating label appears above the shelf showing its assigned category
- **Player placement is restricted**: If you try to place a product from a different category on a tagged shelf, it will be blocked
- **Employee AI is category-aware**: When employees put away leftover products, they'll look for a shelf tagged with the matching category first
- Assignments are saved automatically and persist when you close and reopen the game

**Floating labels:**

- Colored labels float above every tagged shelf so you can see your organization at a glance
- Press **H** (default hotkey) to toggle labels on or off
- You can also toggle labels from the in-game config window
- Hiding labels can help with performance if you have many tagged shelves

### In-Game Configuration

Press **F7** (default hotkey) to open a scrollable settings window where you can adjust every mod setting in real-time without closing the game or editing files.

---

## Installation

### Step 1: Install BepInEx 5

If you don't already have BepInEx installed:

1. Download [BepInEx 5 (x64)](https://github.com/BepInEx/BepInEx/releases) -- get the latest **5.x** release, not 6.x
2. Extract the contents into your Supermarket Together game folder (where `Supermarket Together.exe` is located)
3. Run the game once and close it -- BepInEx will create its folder structure

Your game folder should now have a `BepInEx` folder with `plugins`, `config`, and `core` subfolders.

### Step 2: Install Order & Organize

1. Download `OrderAndOrganize.dll`
2. Create a folder: `<GameDir>\BepInEx\plugins\OrderAndOrganize\`
3. Place `OrderAndOrganize.dll` inside that folder
4. Launch the game

On first launch, the mod creates its config file automatically.

---

## Controls & Hotkeys

| Key | Action | When |
|-----|--------|------|
| **F7** | Open/close the settings window | Anytime in-game |
| **F8** | Toggle automatic ordering on/off | Anytime in-game |
| **G** | Open category picker / Cancel picker | While looking at a storage shelf / While picker is open |
| **H** | Toggle floating category labels | Anytime in-game |
| **Enter** | Apply selected category | While category picker is open |
| **Backspace** | Clear category from shelf | While category picker is open |
| **Scroll wheel / Arrow keys** | Browse categories | While category picker is open |

All hotkeys are configurable in the settings window (F7) or the config file.

---

## Understanding Stock Values

The ordering interface shows three colored stock values for each product:

| Color | What It Means |
|-------|---------------|
| **Red** | Units currently on display shelves |
| **Green** | Units in storage containers |
| **Yellow** | Units in unopened boxes, being carried by employees, or being carried by you |

**Total Stock** = Red + Green + Yellow

The mod considers a product "low stock" when its total stock is below the threshold (default: 40 units). You can change this threshold in the settings.

---

## Settings Reference

All settings can be changed in-game by pressing **F7**, or by editing the config file at:
`<GameDir>\BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg`

### General

| Setting | Default | What It Does |
|---------|---------|--------------|
| Enabled | On | Master switch -- turn off to completely disable the mod |
| Threshold | 40 | Products with total stock below this number are considered low-stock |
| Button Text | "Add Low Stock" | Text shown on the manual restock button |
| Config Window Hotkey | F7 | Key to open/close the settings window |

### Automatic Ordering

| Setting | Default | What It Does |
|---------|---------|--------------|
| Auto-Order at Startup | Off | When enabled, automatic ordering turns on as soon as you load into your store |
| Toggle Hotkey | F8 | Key to turn automatic ordering on or off |
| Scan Interval | 10 seconds | How often the mod checks your inventory (range: 2-300 seconds) |
| Cash Reserve | 0 | The mod will never spend your balance below this amount |
| Pending Order Timeout | 120 seconds | How long to wait before allowing a re-order of the same product |
| Show Notifications | On | Display in-game notifications when automatic purchases are made |

### Category Shelves

| Setting | Default | What It Does |
|---------|---------|--------------|
| Enabled | On | Turn the category shelf feature on or off |
| Hotkey | G | Key to open the category picker on a storage shelf |
| Labels Visible | On | Show floating category labels above tagged shelves |
| Labels Toggle Hotkey | H | Key to show/hide floating labels |

---

## Multiplayer

Order & Organize is designed primarily for **singleplayer and host use**.

| Feature | As Host | As Client (Non-Host) |
|---------|---------|---------------------|
| Smart Restock Button | Works fully | Adds to shopping list (purchases are host-authoritative) |
| Automatic Ordering | Works fully | Automatically disabled |
| Category Shelves (picker + labels) | Works fully | Works, but assignments are stored locally on your machine |
| Category Placement Blocking | Works fully | Works fully |
| Employee Category AI | Works fully | Not applicable (host controls employees) |
| In-Game Config Window | Works fully | Works fully |

**Important notes for multiplayer:**
- Only the **host** needs the mod for automatic ordering to work
- Category shelf assignments are **not shared** between players -- each player has their own local assignments
- There is no conflict if some players have the mod and others don't
- The mod does not affect other players' gameplay or UI

---

## Compatibility

- **Game version**: Tested with Supermarket Together (latest Steam version as of August 2026)
- **BepInEx version**: Requires BepInEx 5.x (not compatible with BepInEx 6.x)
- **Other mods**: Order & Organize should work alongside most other BepInEx mods. It patches a small number of game methods (employee restocking and shelf placement) and avoids conflicts by checking for existing patches. If you experience issues with another mod, try disabling one at a time to isolate the conflict.
- **Game updates**: Major game updates may break the mod if the developers change internal code. The mod logs clear error messages if it can't find expected game APIs, so check the BepInEx log if something stops working after a game update.

---

## Troubleshooting

### The mod isn't loading
- Make sure you have **BepInEx 5** installed (not 6.x)
- Check that `OrderAndOrganize.dll` is in `BepInEx\plugins\OrderAndOrganize\`
- Look at `BepInEx\LogOutput.log` for error messages

### The "Add Low Stock" button doesn't appear
- Open the ordering computer and wait a moment -- the button appears once the ordering UI is fully loaded
- Check that the mod is enabled (press F7 and verify "Enabled" is checked)

### Category picker doesn't open
- Make sure you're looking directly at a **storage shelf** (not a display shelf)
- The shelf must be within interaction range
- Check that Category Shelves are enabled in settings (F7)

### My category assignments disappeared
- The mod creates automatic backups. If assignments are lost, they should be restored automatically on next load
- Your backup file is at: `BepInEx\config\OrderAndOrganize_CategoryShelves.json.backup`

### Automatic ordering isn't working
- Press F8 to make sure it's enabled (a notification will confirm)
- Check that your shopping list is empty -- automation pauses if there are manual items on the list
- In multiplayer, automatic ordering only works for the host
- Make sure you have enough money (above your cash reserve setting)

### Performance issues
- If you have many tagged shelves, try hiding floating labels (press H)
- Increase the scan interval in settings to reduce how often inventory is checked

---

## Uninstallation

1. Delete the folder: `<GameDir>\BepInEx\plugins\OrderAndOrganize\`
2. Optionally delete the config file: `<GameDir>\BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg`
3. Optionally delete shelf data: `<GameDir>\BepInEx\config\OrderAndOrganize_CategoryShelves.json` (and `.backup`)

Removing the mod has no effect on your save file. Your store will continue to work normally.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a full history of changes.

---

## Credits

Created by **ddqminik**.

Built with [BepInEx](https://github.com/BepInEx/BepInEx) and [HarmonyX](https://github.com/BepInEx/HarmonyX).
