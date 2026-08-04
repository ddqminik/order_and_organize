# Settings Reference

This page lists every setting in **Order & Organize** v0.5.0, its default value, allowed range, and what it does in plain terms.

---

## How to Change Settings

You can change settings in two ways:

### In-game settings window (recommended)

Press **F7** (default hotkey) to open the in-game config window. Changes take effect **immediately** — no restart required.

### Config file

Edit the config file directly at:

```
<GameDir>\BepInEx\config\com.ddqminik.supermarkettogether.orderandorganize.cfg
```

Replace `<GameDir>` with your Supermarket Together installation folder (the folder that contains `Supermarket Together.exe`).

Changes made to the config file require a **game restart** to take effect.

---

## General Settings

| Setting | Default | Range | Description |
|---------|---------|-------|-------------|
| **Enabled** | On | On/Off | Master switch. Turns the entire mod on or off. When off, none of the mod's features run. |
| **Threshold** | 40 | 0 – 9999 | Products with total stock below this number are considered low. Used by both the manual [Smart Restock Button](Smart-Restock-Button.md) and [Automatic Ordering](Automatic-Ordering.md). |
| **Button Text** | `"Add Low Stock"` | Any text | The label shown on the manual restock button in the ordering interface. Change this if you want different wording. |
| **Config Window Hotkey** | F7 | Any key | Key to open or close the in-game settings window. |

---

## Automation Settings

These settings control [Automatic Ordering](Automatic-Ordering.md) — the background system that scans inventory and purchases low-stock products for you.

| Setting | Default | Range | Description |
|---------|---------|-------|-------------|
| **Auto-Order at Startup** | Off | On/Off | When enabled, automatic ordering turns on as soon as you load into your store. You can still toggle it off with the hotkey during a session. |
| **Toggle Hotkey** | F8 | Any key | Key to toggle automatic ordering on or off. A notification confirms the current state. |
| **Scan Interval** | 10 seconds | 2 – 300 | How often the mod checks your inventory when automation is on. Lower values are more responsive but trigger orders more frequently. |
| **Cash Reserve** | 0 | 0 – 999,999 | Minimum money to always keep in your account. The mod never spends below this amount. Set this to avoid running completely dry. |
| **Pending Order Timeout** | 120 seconds | 10 – 600 | How long the mod remembers a product it already ordered before allowing a re-order. Prevents duplicate purchases for the same product. |
| **Show Notifications** | On | On/Off | Whether to display in-game notifications when automatic purchases happen (e.g. *"Ordered 5 boxes for $120"*). |

---

## Category Shelves Settings

These settings control [Category Storage Shelves](Category-Storage-Shelves.md) — backroom shelf tagging, placement blocking, and floating labels.

| Setting | Default | Range | Description |
|---------|---------|-------|-------------|
| **Enabled** | On | On/Off | Turns the category shelf feature on or off entirely. When off, the picker, labels, and placement blocking do not run. |
| **Hotkey** | G | Any key | Key to open the category picker when looking at a storage shelf in your backroom. |
| **Labels Visible** | On | On/Off | Whether to show floating category labels above tagged shelves. |
| **Labels Toggle Hotkey** | H | Any key | Key to toggle floating labels on or off during gameplay. Useful for performance with many tagged shelves. |

---

## Recommended Configurations

These are starting points — adjust based on how your store runs and how aggressively you want to spend.

### Small store (just starting)

Best when you have a limited product range and want to conserve cash early on.

| Setting | Recommended Value |
|---------|-------------------|
| Threshold | 20 |
| Scan Interval | 30 seconds |
| Cash Reserve | 2000 |

A lower threshold avoids over-ordering products you barely sell yet. A longer scan interval reduces how often the mod checks stock. A cash reserve of 2000 keeps a safety buffer for rent, upgrades, and unexpected expenses.

### Medium store

Good default balance for a growing store with a steady product mix.

| Setting | Recommended Value |
|---------|-------------------|
| Threshold | 40 (default) |
| Scan Interval | 10 seconds (default) |
| Cash Reserve | 5000 |

The mod's defaults work well here. Bump the cash reserve to 5000 once you are ordering regularly and want more protection against overspending.

### Large busy store

For high-traffic stores with many products and frequent stockouts.

| Setting | Recommended Value |
|---------|-------------------|
| Threshold | 60 – 80 |
| Scan Interval | 5 seconds |
| Cash Reserve | 10000 |

A higher threshold catches stock drops sooner before shelves go empty during rush periods. A 5-second scan interval keeps up with fast-moving inventory. A larger cash reserve protects your operating budget while automation runs frequently.

---

## Hotkey Reference

Hotkeys use Unity **KeyCode** names when editing the config file directly. Common examples:

| KeyCode Name | Key |
|--------------|-----|
| `F7`, `F8`, `F1`–`F12` | Function keys |
| `G`, `H` | Letter keys |
| `Alpha1`–`Alpha9` | Number row |

Press **F7** in-game to rebind hotkeys without editing the config file manually.

---

## Related Pages

- [Automatic Ordering](Automatic-Ordering.md) — How automation works and when to enable it
- [Category Storage Shelves](Category-Storage-Shelves.md) — How to use category shelves in your backroom
- [Troubleshooting & FAQ](Troubleshooting-and-FAQ.md) — Fixes when settings do not seem to apply
