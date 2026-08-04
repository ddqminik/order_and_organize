# Automatic Ordering

Automatic Ordering is an optional feature that periodically scans your inventory and purchases low-stock products for you — completely hands-free. It uses the same stock calculations as the [Smart Restock Button](Smart-Restock-Button.md), but instead of adding items to your shopping list for manual review, it buys them automatically.

---

## What It Does

When enabled, the mod runs on a timer (default: every **10 seconds**) and:

1. Scans all products in your store
2. Finds products where total stock is below the threshold (default: 40 units)
3. Adds one box of each low-stock product to the shopping list
4. **Immediately purchases** those boxes
5. Shows a notification with the number of boxes ordered and the total cost

You do not need to open the ordering computer or confirm anything. The mod handles the entire purchase cycle in the background while you manage your store.

---

## Disabled by Default

Automatic ordering is **OFF** when you first install the mod. This is intentional — unattended purchasing can spend your money quickly if you are not prepared.

You must **explicitly enable** automatic ordering before it will run. See [How to Enable](#how-to-enable) below.

---

## How to Enable

There are two ways to turn automatic ordering on:

### Toggle with the hotkey (recommended for first use)

1. Press **F8** (default hotkey) while in your store.
2. A notification confirms whether automatic ordering is now **ON** or **OFF**.
3. Press **F8** again anytime to toggle it off.

This lets you turn automation on and off quickly without opening menus.

### Auto-start at launch

If you want automatic ordering to run every time you play:

1. Press **F7** to open the settings window.
2. Enable **Auto-Order at Startup**.
3. Automatic ordering will turn on as soon as you load into your store.

You can still press **F8** to toggle it off during a session even with this setting enabled.

---

## How It Works

### Scan cycle

Every **X seconds** (default **10**, configurable from 2 to 300 seconds), the mod:

- Checks every product's total stock (Red + Green + Yellow — see [Understanding Stock Values](Smart-Restock-Button.md#understanding-stock-values))
- For each product below the threshold, adds one box and purchases it immediately
- Displays a notification: *"Ordered X boxes for $Y"*

### What gets skipped

The same skip rules as the Smart Restock Button apply:

- Products already on your shopping list (see [Shopping List Safety](#shopping-list-safety) below)
- Locked or unorderable products
- Products above the threshold
- Products recently ordered (see [Duplicate Protection](#duplicate-protection) below)
- Purchases that would drop your balance below the cash reserve

---

## Cash Reserve

The **Cash Reserve** setting defines a minimum balance the mod will never spend below.

| Setting | Default | Example |
|---------|---------|---------|
| Cash Reserve | 0 | Set to **5000** to always keep $5,000 available |

**Example:** If you have $12,000 and your cash reserve is $5,000, the mod will spend at most $7,000 per scan cycle. If a purchase would leave you below $5,000, it is skipped.

This protects you from going broke on automatic orders when you still need money for rent, licenses, store expansions, or other expenses.

---

## Shopping List Safety

If you have **any items** on your shopping list — even one box you added manually — automatic ordering **completely pauses**.

The mod waits until your shopping list is **completely empty** before resuming. This guarantees:

- Your manual orders are never modified or purchased without your intent
- Automatic and manual ordering never conflict
- You can add items to your list anytime without the mod interfering

**Tip:** Clear your shopping list before enabling automatic ordering for the first time, so it can start working immediately.

---

## Duplicate Protection

Without safeguards, the mod could order the same product every 10 seconds while waiting for a delivery to arrive. Duplicate protection prevents this.

The mod **remembers what it ordered** and will not re-order the same product until:

- The delivery arrives and stock increases, **or**
- The **Pending Order Timeout** expires (default: **120 seconds**)

This means you get one box per product per delivery cycle, not ten boxes of the same item stacked up.

---

## Host Only

In **multiplayer**, automatic ordering only runs for the **host player**.

| Role | Automatic Ordering |
|------|--------------------|
| Host | Works normally |
| Non-host (client) | Automatically disabled |

This prevents conflicts when multiple players have the mod installed. Non-host players can still use the Smart Restock Button and all other features — only the automatic purchasing is restricted.

---

## Best Practices

Follow these recommendations when setting up automatic ordering for the first time:

### Start with a higher threshold for busy stores

If your store sees heavy customer traffic, products sell out faster. A threshold of **50–60** catches shortages earlier than the default 40.

### Set a cash reserve

Always keep a buffer. A reserve of at least **$3,000–$5,000** is a good starting point for most stores. Increase it as your store grows and expenses rise.

### Increase the scan interval for fewer, larger orders

The default 10-second interval works well, but if you prefer less frequent ordering:

- Set the scan interval to **30–60 seconds**
- Each cycle may order more boxes at once (since more products drop below threshold between scans)
- Reduces notification spam and feels less "micro-managed"

### Clear your shopping list before enabling

Make sure your shopping list is empty when you first press **F8**, so automation is not stuck waiting for you to clear manual items.

### Watch the first few cycles

After enabling, pay attention to the notifications for the first few scan cycles. Confirm the mod is:

- Ordering sensible products (not things you are intentionally letting run low)
- Respecting your cash reserve
- Not ordering duplicates

Adjust the threshold, cash reserve, or scan interval in settings (F7) based on what you observe.

### Turn it off when experimenting

If you are reorganizing your store, testing new product layouts, or intentionally running certain items down, press **F8** to disable automatic ordering temporarily.

---

## Settings Overview

These settings control automatic ordering. All can be changed in-game with **F7**. See [Settings Reference](Settings-Reference.md) for full details.

| Setting | Default | Purpose |
|---------|---------|---------|
| Auto-Order at Startup | Off | Start automatic ordering when you load into your store |
| Toggle Hotkey | F8 | Turn automatic ordering on or off |
| Scan Interval | 10 seconds | How often inventory is checked |
| Cash Reserve | 0 | Minimum balance to keep untouched |
| Pending Order Timeout | 120 seconds | Wait time before re-ordering the same product |
| Show Notifications | On | Display purchase notifications in-game |
| Threshold | 40 | Stock level below which products are ordered (shared with Smart Restock Button) |

---

## Related Pages

- [Smart Restock Button](Smart-Restock-Button.md) — manual one-click restocking alternative
- [Settings Reference](Settings-Reference.md) — complete settings list with defaults
- [Troubleshooting & FAQ](Troubleshooting-and-FAQ.md) — fixes for when automatic ordering does not work
