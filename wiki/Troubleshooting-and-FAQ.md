# Troubleshooting & FAQ

Common problems, fixes, and frequently asked questions for **Order & Organize** v0.5.0.

---

## Troubleshooting

### The mod isn't loading at all

1. **Verify BepInEx 5 is installed** — The mod requires BepInEx **5.x**, not 6.x. Check that `BepInEx\core\BepInEx.dll` exists in your game folder.
2. **Check the plugin location** — `OrderAndOrganize.dll` must be in `BepInEx\plugins\OrderAndOrganize\`, not directly in `BepInEx\plugins\`.
3. **Read the log** — Open `BepInEx\LogOutput.log` and search for `OrderAndOrganize` or `Order & Organize`. Error messages usually explain what went wrong.
4. **Remove the old mod** — If you previously used Smart Restock, delete `SmartRestock.dll` from your plugins folder. Having both installed can cause conflicts.

---

### The "Add Low Stock" button doesn't appear

1. **Open the ordering computer** — The button only appears on the ordering interface. Open the computer and wait a moment for the UI to load.
2. **Check that the mod is enabled** — Press **F7** and confirm **Enabled** is turned on under General Settings.
3. **Check the log** — Look in `BepInEx\LogOutput.log` for errors related to the ordering UI. A game update or mod conflict can prevent the button from being injected.

See [Smart Restock Button](Smart-Restock-Button.md) for how the button is supposed to work.

---

### Category picker doesn't open when I press G

1. **Look at a storage shelf** — You must be looking directly at a **storage shelf** in the backroom, not a display shelf or other furniture.
2. **Move closer** — You need to be within interaction range. Try standing closer and looking at the shelf center.
3. **Check settings** — Press **F7** and confirm **Category Shelves → Enabled** is turned on.
4. **Verify the hotkey** — The default is **G**, but you may have changed it in settings. Check **Category Shelves → Hotkey** in the F7 window.

See [Category Storage Shelves](Category-Storage-Shelves.md) for full usage instructions.

---

### My category assignments disappeared

1. **Restart the game** — The mod has automatic backup and restore. Assignments are often recovered on the next load after a crash or interrupted session.
2. **Check the backup file** — Look for `BepInEx\config\OrderAndOrganize_CategoryShelves.json.backup`. The mod uses this file to restore lost data.
3. **Reassign if needed** — If both the main file and the backup are empty or corrupted, assignments may have been permanently lost. You will need to tag your shelves again.

---

### Automatic ordering isn't purchasing anything

1. **Confirm it is enabled** — Press **F8** and check the notification. It should say automatic ordering is **ON**.
2. **Empty your shopping list** — Automation only runs when your shopping list is completely empty. Clear any pending items first.
3. **Check your balance** — Make sure you have enough money above your [Cash Reserve](Settings-Reference.md#automation-settings). The mod will not spend below that amount.
4. **Multiplayer: host only** — In multiplayer, only the **host** can use automatic ordering. Non-host players cannot trigger background purchases.

See [Automatic Ordering](Automatic-Ordering.md) for full behavior details.

---

### The game crashes

1. **Test without the mod** — Remove or disable the mod temporarily and see if the crash still happens. Some crashes are base game bugs unrelated to mods.
2. **Check BepInEx log** — Open `BepInEx\LogOutput.log` for errors near the time of the crash.
3. **Check Unity player log** — Open `%APPDATA%\..\LocalLow\DDTNL\Supermarket Together\Player.log` for additional crash details.
4. **Known engine issue** — If the crash log mentions `ucrtbase.dll` or `STATUS_STACK_BUFFER_OVERRUN`, this is a Unity engine issue, not caused by the mod. Updating graphics drivers or verifying game files may help.

---

### Performance issues or lag

1. **Hide floating labels** — Press **H** to toggle category labels off. With dozens of tagged shelves, labels add rendering overhead.
2. **Increase scan interval** — In settings (**F7**), raise **Scan Interval** to 30 or 60 seconds instead of the default 10. Less frequent inventory checks reduce background work.
3. **Disable verbose logging** — If you enabled verbose or debug logging for troubleshooting, turn it off once you are done. Excessive log writes can cause minor stutter.

---

### Settings aren't saving

1. **In-game changes save immediately** — Settings changed through the **F7** window are written to the config file right away and apply without a restart.
2. **Config file edits need a restart** — If you edited `com.ddqminik.supermarkettogether.orderandorganize.cfg` manually, save the file and **restart the game** for changes to load.
3. **Check file permissions** — Make sure the game folder is not read-only and you have write access to `BepInEx\config\`.

---

## FAQ

### Does this mod work in multiplayer?

**Yes.** As the **host**, all features work fully — automatic ordering, the restock button, and category shelves.

Non-host players can use the manual restock button and category picker, but:

- **Automatic ordering is host-only**
- **Category assignments are stored locally per player** — each player maintains their own shelf tags

---

### Do all players need the mod?

**No.** Only the host needs the mod installed for automatic ordering to work. Players without the mod are completely unaffected — they see and play the normal game.

For category shelves and the restock button on non-host clients, those players need the mod installed locally to use those features themselves.

---

### Can the mod break my save?

**No.** The mod never modifies your save file. All purchases and placements go through the game's normal systems. Uninstalling the mod leaves your save completely intact — you simply lose the mod's extra features.

---

### Does it work with other mods?

**It should.** Order & Organize patches only a small number of game methods and is designed to coexist with most BepInEx mods.

If you experience issues, try disabling other mods one at a time to identify a conflict. Check `BepInEx\LogOutput.log` for Harmony patch errors.

---

### Will it break after a game update?

**Possibly.** If the game developers change internal code that the mod relies on, features may stop working until the mod is updated. The mod logs clear error messages when this happens — search the log for `OrderAndOrganize` after a major game patch.

Check for mod updates after significant Supermarket Together updates.

---

### How do I change the hotkeys?

Press **F7** to open settings and change any hotkey field directly.

Alternatively, edit the config file at `BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg`. Hotkeys use Unity **KeyCode** names (e.g. `F7`, `F8`, `G`, `H`, `Alpha1`). Restart the game after editing the file.

See [Settings Reference](Settings-Reference.md#hotkey-reference) for common KeyCode names.

---

### What happens if I run out of money with automation on?

The mod **stops purchasing** and waits until you have enough money again. It does not put you into debt or force failed transactions.

If you have a **Cash Reserve** set, the mod stops even earlier — when a purchase would drop your balance below that reserve. Automation resumes automatically once your balance is sufficient.

---

### Can I use the manual button and automation at the same time?

**Yes**, but automation **pauses while your shopping list has items on it**.

Use the manual **Add Low Stock** button to fill your list, complete the purchase at the ordering computer, and automation resumes on its own once the list is empty again.

---

## Related Pages

- [Installation Guide](Installation-Guide.md) — Setup and verification steps
- [Settings Reference](Settings-Reference.md) — Every setting explained
- [Home](Home.md) — Feature overview and quick start
