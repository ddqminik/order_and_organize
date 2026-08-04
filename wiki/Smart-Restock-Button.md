# Smart Restock Button

The Smart Restock Button adds a one-click **Add Low Stock** button to the ordering computer in your store. Instead of scrolling through hundreds of products and adding each one manually, you can fill your shopping list with every low-stock item in a single click.

---

## What It Does

When you open the ordering computer, a button labeled **Add Low Stock** appears on the interface (the label can be customized in settings). Clicking it:

1. Scans **every product** in your store
2. Identifies products where total stock is below your configured threshold
3. Adds **one box** of each low-stock product to your shopping list
4. Shows a notification summarizing what was added and what was skipped

This is a **manual** feature — it only adds items to your shopping list. You still review the list and confirm the purchase yourself, just like any normal order.

---

## How to Use

1. **Open the ordering computer** at your store (the same terminal you use to order products normally).
2. **Click Add Low Stock** on the ordering interface.
3. **Review your shopping list** — the mod adds one box per low-stock product. Remove anything you do not want before purchasing.
4. **Confirm the purchase** as you normally would.

That is the entire workflow. Use the button whenever you want a quick restock run without hunting through the product catalog.

---

## Understanding Stock Values

The ordering interface shows **three colored stock numbers** for each product. The mod uses all three to calculate whether a product needs restocking.

| Color | Meaning |
|-------|---------|
| **Red** | Units currently on display shelves (what customers can buy) |
| **Green** | Units in storage containers in your backroom |
| **Yellow** | Units in unopened boxes on the ground, being carried by employees, or being carried by you |

**Total Stock = Red + Green + Yellow**

A product is considered **low stock** when its total stock is **below the threshold**. The default threshold is **40 units**, but you can change this in settings (press **F7**).

### Example

If a product shows Red `12`, Green `8`, Yellow `5`:

- Total Stock = 12 + 8 + 5 = **25**
- With the default threshold of 40, this product is **low stock** and would be added by the button

---

## What Gets Skipped

The button is smart about what it adds. It will **not** add a product if:

| Reason | Explanation |
|--------|-------------|
| **Already on shopping list** | Avoids duplicate entries — if you already added it manually, the button leaves it alone |
| **Locked or unorderable** | Products you have not unlocked or cannot order yet are ignored |
| **Above the threshold** | Products with enough total stock are not considered low-stock |

After clicking the button, a **notification** tells you exactly how many products were added and how many were skipped (along with the reasons).

---

## Prioritization

When multiple products are low on stock, the mod adds them in a sensible order:

1. **Lowest total stock first** — the most critically understocked products are prioritized
2. **Greatest shortage second** — among products with similar stock, those furthest below the threshold come first
3. **Product ID last** — used as a tiebreaker for consistent, predictable ordering every time

This means the products that need attention most urgently appear at the top of your shopping list.

---

## Notification

After you click **Add Low Stock**, an in-game notification appears showing:

- How many products were **added** to your shopping list
- How many were **skipped**, and why (already on list, locked, above threshold, etc.)

Use this feedback to understand what the mod did at a glance. If the number seems low, check whether many products are already on your list or above the threshold.

---

## Tips

### Adjust the threshold for your store size

The default threshold of **40** works well for medium-sized stores. Consider adjusting it based on how much inventory you typically carry:

| Store Size | Suggested Threshold |
|------------|---------------------|
| Small store, few product types | 20–30 |
| Medium store | 40 (default) |
| Large store, high traffic | 60+ |

Open settings with **F7** and change the **Threshold** value to match your needs. A lower threshold means fewer products trigger restocking; a higher threshold means the button catches shortages earlier.

### Customize the button text

If **Add Low Stock** does not fit your workflow, change the **Button Text** setting in the config window (F7). For example, you could rename it to **Restock All** or **Fill Low Items**.

### Combine with automatic ordering

The Smart Restock Button and Automatic Ordering use the same threshold and stock calculations. If you prefer reviewing orders before buying, use the button. If you want fully hands-free restocking, see [Automatic Ordering](Automatic-Ordering.md).

### Clear your shopping list first

The button skips products already on your list. If you want a clean restock run, clear your shopping list before clicking **Add Low Stock** so nothing is skipped for being a duplicate.

---

## Related Pages

- [Automatic Ordering](Automatic-Ordering.md) — hands-free alternative that purchases automatically
- [Settings Reference](Settings-Reference.md) — full list of threshold, button text, and other settings
- [Troubleshooting & FAQ](Troubleshooting-and-FAQ.md) — what to do if the button does not appear
