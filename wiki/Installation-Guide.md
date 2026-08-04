# Installation Guide

This guide walks you through installing BepInEx 5 and Order & Organize for Supermarket Together.

---

## Prerequisites

Before you begin, make sure you have:

- **Supermarket Together** installed on Steam
- **BepInEx 5** - you need the **5.x** release. **BepInEx 6.x is not compatible** with this mod.

If you already have BepInEx 5 installed and working, skip to [Installing Order & Organize](#installing-order--organize).

---

## Installing BepInEx 5

1. **Download BepInEx 5 (x64)** from the [BepInEx releases page on GitHub](https://github.com/BepInEx/BepInEx/releases).
   - Choose the latest **5.x** release (for example, `BepInEx_x64_5.x.x.x.zip`).
   - Do **not** download a 6.x release.

2. **Find your game folder.** In Steam, right-click Supermarket Together → **Manage** → **Browse local files**. This opens the folder containing `Supermarket Together.exe`.

3. **Extract the BepInEx archive** into that game folder. After extraction, you should see files like `winhttp.dll` alongside `Supermarket Together.exe`.

4. **Run the game once**, then close it completely. BepInEx creates its folder structure on first launch. After this step, you should have a `BepInEx` folder with `plugins`, `config`, and `core` subfolders inside your game directory.

---

## Installing Order & Organize

1. **Download** `OrderAndOrganize.dll` from your preferred mod source (Nexus Mods, Thunderstore, GitHub releases, etc.).

2. **Create the plugin folder:**
   ```
   <GameDir>\BepInEx\plugins\OrderAndOrganize\
   ```
   Replace `<GameDir>` with your Supermarket Together installation path.

3. **Place** `OrderAndOrganize.dll` inside the `OrderAndOrganize` folder. The full path should look like:
   ```
   <GameDir>\BepInEx\plugins\OrderAndOrganize\OrderAndOrganize.dll
   ```

4. **Launch the game.** On first launch, the mod creates its configuration file automatically at:
   ```
   BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg
   ```

---

## Verifying Installation

After launching the game, confirm the mod loaded correctly:

1. **Check the BepInEx console** (a separate window that may appear when the game starts) or open the log file at:
   ```
   <GameDir>\BepInEx\LogOutput.log
   ```
   Look for a line similar to:
   ```
   Order & Organize v0.5.0 loaded successfully
   ```

2. **Press F7 in-game** to open the mod's settings window. If the window appears, the mod is installed and running.

3. **Open the ordering computer** in your store. You should see an **Add Low Stock** button on the ordering interface.

If any of these checks fail, see [Troubleshooting & FAQ](Troubleshooting-and-FAQ.md).

---

## Upgrading from Smart Restock

If you previously used this mod under its old name **Smart Restock**, follow these steps:

1. **Install Order & Organize** as described above (you can keep your existing BepInEx installation).

2. **Delete the old plugin manually.** Remove `SmartRestock.dll` from your `BepInEx\plugins\` folder (or wherever you placed it). Do not run both mods at the same time.

3. **Launch the game.** Your settings and category shelf data are **migrated automatically** on first launch. You do not need to reconfigure anything.

The new mod reads your old configuration and category assignments and converts them to the new format. Your backup files are preserved during migration.

---

## Updating the Mod

To update to a newer version of Order & Organize:

1. **Close the game** completely.
2. **Delete** the old `OrderAndOrganize.dll` from `BepInEx\plugins\OrderAndOrganize\`.
3. **Copy** the new `OrderAndOrganize.dll` into the same folder.
4. **Launch the game.**

Your settings are preserved between updates. You do not need to delete your config file unless you want to reset everything to defaults.

---

## Uninstallation

To remove Order & Organize from your game:

1. **Delete the plugin folder:**
   ```
   <GameDir>\BepInEx\plugins\OrderAndOrganize\
   ```

2. **Optionally delete mod data files** (only if you want a clean removal):
   - Config file: `BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg`
   - Category shelf data: `BepInEx\config\OrderAndOrganize_CategoryShelves.json`
   - Category shelf backup: `BepInEx\config\OrderAndOrganize_CategoryShelves.json.backup`

Removing the mod has **zero effect on your save files**. Your store, money, inventory, and progress remain exactly as they were. BepInEx itself stays installed unless you remove it separately.

---

## Next Steps

- Learn how the restock button works: [Smart Restock Button](Smart-Restock-Button.md)
- Set up hands-free ordering: [Automatic Ordering](Automatic-Ordering.md)
- Organize your backroom: [Category Storage Shelves](Category-Storage-Shelves.md)
