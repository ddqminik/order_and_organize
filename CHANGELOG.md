# Changelog

All notable changes to Order & Organize are documented in this file.

## [0.5.0] - 2026-08-03

### Fixed
- **Critical**: `PurgeStaleAssignments()` wiped all category shelf data during scene transitions (exit to menu / store switch). The purge ran while shelves were unloaded from memory, falsely detecting every assignment as stale.

### Added
- **Scene guard for purge**: Stale assignment cleanup now requires `GameData.Instance` to exist and a 120-second grace period after scene load, ensuring all props have spawned before any purge runs.
- **50% safety threshold**: If more than half of all assignments would be purged in a single pass, the purge aborts entirely and logs a warning. Prevents mass-wipe regardless of cause.
- **Backup before purge**: A backup file (`OrderAndOrganize_CategoryShelves.json.backup`) is created before any assignments are removed.
- **Auto-restore on load**: If the main file has 0 assignments but a backup exists, assignments are automatically restored from the backup.
- **Purge timer reset on scene unload**: The purge timer and scene-loaded timestamp reset when any scene unloads, preventing stale timers from carrying over.
- Removed broken `Data_Container.OnDestroy` Harmony patch that prevented all category storage patches from applying (method does not exist on `Data_Container`).

## [0.4.0] - 2026-08-03

### Added
- **In-game configuration window** (F7 hotkey): Scrollable IMGUI panel for real-time config changes without editing files.
- **Category storage shelves**: Tag storage shelves with product categories to restrict placement and direct employee restocking behavior.
  - G key opens scroll-wheel category picker while looking at a storage shelf.
  - Mouse wheel / arrow keys browse categories; Enter applies; Backspace clears; G cancels.
  - Floating color-coded labels above tagged shelves (toggleable via H key or config window).
  - Harmony patches enforce category restrictions on player placement and employee AI.
  - Assignments persist to `OrderAndOrganize_CategoryShelves.json` using grid-snapped position keys.
- **Category label toggle**: Configurable hotkey (default H) and config checkbox to show/hide floating labels.
- **Improved category picker readability**: Dark category colors are boosted to minimum luminance; selected items use white-on-color.
- **Prominent auto-purchase notifications**: Automation cycle notifications include box count and total spend amount.
- **Corrupt data purging**: Malformed position keys are detected and removed on load.

### Fixed
- Invisible "Add Low Stock" button (now clones native button style).
- Camera/movement not freezing during category picker (uses `FindFirstObjectByType` to locate `FirstPersonController`).
- Category assignments not persisting (fixed JSON parser for comma-containing keys).
- ESC key conflict with game menu when closing category picker (changed to G key).
- Config window clipping (added scroll view, widened to 420px).
- Input processing duplication causing scroll-by-2 and G-key not closing picker (consolidated to single `UpdateInput()` path).

## [0.3.0] - 2026-08-03

### Added
- Manual smart-list mode: "Add Low Stock" button on ordering interface.
- Automatic ordering mode with F8 toggle hotkey.
- Configurable stock threshold, scan interval, cash reserve.
- Pending order tracking with timeout-based deduplication.
- Money protection (cash reserve, fund verification before purchase).
- Shopping list safety (automation halts if manual items present).
- Host-only enforcement via `NetworkServer.active`.
- Localization bypass for custom notification text.
- Game API diagnostics logging on first store load.
- 45 unit tests covering core business logic.
- PowerShell scripts for build, deploy, save backup, and log collection.
