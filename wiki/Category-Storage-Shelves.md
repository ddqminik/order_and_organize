# Category Storage Shelves

Category Storage Shelves let you tag backroom storage shelves with a product category — Dairy, Drinks, Snacks, Fruits & Vegetables, and so on. Categories match the tabs you see in the ordering interface. Once a shelf is tagged, the game enforces where products can go, and your employees start putting stock in the right place automatically.

---

## What It Does

You assign a product category to any storage shelf in your backroom. The category you pick is the same one you see when browsing products on the ordering computer. Tagged shelves show a color-coded floating label, block products from other categories, and guide employee restocking behavior.

---

## Why Use It

Without category shelves, employees dump leftover products on whatever storage shelf has free space. Over time your backroom becomes a mixed-up mess — drinks next to dairy, snacks on the wrong aisle, and longer walks when restocking displays.

With category shelves:

- Products stay grouped by type in the backroom
- Employees look for the matching category shelf first when putting stock away
- You cannot accidentally place the wrong product on a tagged shelf
- Your layout stays organized without micromanaging every employee trip

---

## How to Assign a Category

1. Walk up to a **storage shelf** in your backroom and look directly at it. You must be within interaction range.
2. Press **G** (default hotkey) to open the category picker.
3. Use the **scroll wheel** or **arrow keys** to browse categories.
4. Press **Enter** to assign the highlighted category.
5. The shelf now shows a **color-coded floating label** with its category name.

---

## How to Change a Category

Open the picker (**G**) on the same shelf, select a different category with the scroll wheel or arrow keys, and press **Enter**. The new category replaces the old one immediately.

---

## How to Remove a Category

Open the picker (**G**) on the tagged shelf and press **Backspace** to clear the category. The shelf returns to normal — any product can be placed on it again, and the floating label disappears.

---

## How to Cancel

Press **G** again while the picker is open to close it without applying any changes.

---

## Controls Summary

| Key | Action |
|-----|--------|
| **G** | Open picker / Cancel |
| **Scroll wheel / Arrow keys** | Browse categories |
| **Enter** | Apply selected category |
| **Backspace** | Clear category |

---

## Placement Blocking

When a shelf has a category assigned, you **cannot** place products from a different category on it. The game blocks the placement before it happens. This keeps your organization intact even when you are stocking shelves yourself.

Only products that belong to the assigned category can go on that shelf. Untagged shelves accept any product as usual.

---

## Employee AI

Employees become **category-aware** when this feature is enabled. When putting away leftover products from displays or deliveries, they look for a storage shelf tagged with the **matching category first** before falling back to an untagged or general shelf.

This is the main benefit of category shelves — your backroom stays organized automatically without you standing there directing every restock run.

---

## Floating Labels

Color-coded labels float above every tagged shelf. The colors match the category colors from the ordering interface, so you can identify shelves at a glance.

Press **H** (default hotkey) to toggle labels on or off. Hiding labels can improve performance if you have dozens of tagged shelves across a large backroom.

You can also control label visibility in settings — see [Settings Reference](Settings-Reference.md#category-shelves-settings).

---

## Persistence

Category assignments save **automatically** and persist between game sessions. You do not need to re-tag shelves every time you launch the game.

Your category data is stored at:

```
BepInEx\config\OrderAndOrganize_CategoryShelves.json
```

---

## Backup & Recovery

The mod creates **automatic backups** of your category data. If assignments are lost — for example, due to a crash or an interrupted save — they are restored automatically the next time you load the game.

Your backup file is located at:

```
BepInEx\config\OrderAndOrganize_CategoryShelves.json.backup
```

If you notice missing assignments after a crash, restart the game once before reassigning everything manually. The restore usually happens on load.

---

## Tips for Organizing

- **Start broad** — Tag a few shelves with general categories (Drinks, Snacks, Dairy) when you are just starting out, then add more specific shelves as you unlock products.
- **Place shelves near displays** — Put category shelves close to the corresponding sales floor area so employees have shorter walks when restocking.
- **Use labels for overview, then hide them** — Press **H** to see your full layout at a glance. Once you have memorized where things go, hide labels to reduce on-screen clutter and improve performance.
- **Deleted shelves are cleaned up** — If you remove a shelf that had a category assigned, the orphaned assignment is cleaned up automatically over time. You do not need to manually delete entries from the config file.
- **Multiplayer note** — Category assignments are stored **locally per player**. Each player can organize their view of the backroom differently. See [Troubleshooting & FAQ](Troubleshooting-and-FAQ.md) for more multiplayer details.

---

## Related Pages

- [Settings Reference](Settings-Reference.md) — Hotkeys, label visibility, and other category shelf options
- [Troubleshooting & FAQ](Troubleshooting-and-FAQ.md) — Fixes for picker issues and lost assignments
- [Home](Home.md) — Overview of all mod features
