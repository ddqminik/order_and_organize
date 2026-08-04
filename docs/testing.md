# Testing

## Unit Tests

All business logic is covered by xUnit tests in `tests/OrderAndOrganize.Tests/`.

Run with:
```powershell
dotnet test tests\OrderAndOrganize.Tests\OrderAndOrganize.Tests.csproj
```

### Test Coverage (45 tests)

#### Model Tests
| Test | Description | Status |
|------|-------------|--------|
| CombinedStock_SumsAllThreeValues (x4) | Verifies OnShelves+InStorage+InMovement | PASS |
| EffectiveCombinedStock_IncludesPending (x3) | CombinedStock + PendingUnreflectedUnits | PASS |
| Shortage_IsThresholdMinusCombined | threshold - combinedStock | PASS |
| PurchaseResult_SucceededFactory | Static factory for success | PASS |
| PurchaseResult_FailedFactory | Static factory for failure | PASS |
| AutomationCycleResult_HasPurchases | ProductsPurchased > 0 | PASS |
| AutomationCycleResult_HasSkips | Skip count checks | PASS |

#### Cash Reserve Tests
| Test | Description | Status |
|------|-------------|--------|
| CashReserve_AffordabilityCheck (x6) | money - reserve >= price | PASS |
| MoneyAfterPurchase_NeverBelowReserve (x2) | remaining >= reserve | PASS |

#### Candidate Sorting Tests
| Test | Description | Status |
|------|-------------|--------|
| MultipleCandidates_PurchaseOrder | Sorted by stock asc | PASS |
| MultipleCandidates_BudgetConstraint | Budget-limited purchasing | PASS |

#### RestockPlanner Tests
| Test | Description | Status |
|------|-------------|--------|
| CombinedStock_ThresholdBoundary (x6) | 37<40=include, 40<40=exclude, boundary cases | PASS |
| ManualRestock_ExcludesLockedProducts | unlocked=false filtered out | PASS |
| ManualRestock_ExcludesUnorderableProducts | orderable=false filtered out | PASS |
| ManualRestock_ExcludesProductsAlreadyOnShoppingList | onList=true filtered out | PASS |
| ManualRestock_SortsByLowestStockFirst | stock 2 < 18 < 31 | PASS |
| ManualRestock_TieBreaksByShortageDescendingThenProductIdAscending | Same stock -> by ID | PASS |
| ManualRestock_EmptySnapshotCollection | No crash on empty input | PASS |
| ManualRestock_ThresholdZero_NothingQualifies | Edge case: threshold=0 | PASS |
| Shortage_CalculatedCorrectly | 40 - (10+5+2) = 23 | PASS |
| EffectiveCombinedStock_IncludesPendingUnits | 37 + 30 = 67 | PASS |
| AutoRestock_UsesEffectiveCombinedStock | effective=67 > threshold=40 excluded | PASS |
| AutoRestock_ExcludesProductsWithPendingOrders | pending order blocks re-order | PASS |

#### PendingOrderTracker Tests
| Test | Description | Status |
|------|-------------|--------|
| RecordOrder_CreatesEntry | All fields populated correctly | PASS |
| HasPendingOrder_ReturnsFalseWhenNone | No false positives | PASS |
| Reconcile_ResolvesWhenInMovementReflects | InMovement increase resolves | PASS |
| Reconcile_KeepsPendingWhenNotReflected | No premature resolution | PASS |
| ClearAll_RemovesEverything | Full clear on scene unload | PASS |
| PendingOrder_IsTimedOut | Timeout detection | PASS |

## Manual Test Matrix

These tests require launching the game. Document results after each test run.

| # | Scenario | Expected Result | Status | Notes |
|---|----------|----------------|--------|-------|
| 1 | Install mod, launch game | BepInEx log shows "Order & Organize v0.1.0 loaded" | PENDING | |
| 2 | Load singleplayer store | API diagnostics logged | PENDING | |
| 3 | Open ordering interface | "Add Low Stock" button visible | PENDING | |
| 4 | Click "Add Low Stock" with empty shelves | Products added to shopping list | PENDING | |
| 5 | Click "Add Low Stock" with full shelves | Notification: "nothing below threshold" | PENDING | |
| 6 | Click "Add Low Stock" with products already on list | Already-listed products skipped | PENDING | |
| 7 | Press F8 to enable automation | Notification: "automation enabled" | PENDING | |
| 8 | Press F8 to disable automation | Notification: "automation disabled" | PENDING | |
| 9 | Automation with empty shopping list | Products auto-purchased | PENDING | |
| 10 | Automation with manual items on list | Notification: "manual list not empty" | PENDING | |
| 11 | Automation with low money | Products skipped, notification shown | PENDING | |
| 12 | Automation with cash reserve | Spending stops at reserve | PENDING | |
| 13 | Change threshold in config | Behavior updates on next scan | PENDING | |
| 14 | Close/reopen ordering UI | Button not duplicated | PENDING | |
| 15 | Return to main menu | Pending orders cleared | PENDING | |
| 16 | Multiplayer as client (not host) | Automation disabled with notification | PENDING | |
| 17 | Press G while looking at storage shelf | Scroll-wheel category picker appears, camera/movement frozen | PENDING | |
| 18 | Scroll through categories, press G/Enter | Category applied, picker closes, notification shown | PENDING | |
| 19 | Press G to cancel category picker | Picker closes without applying | PENDING | |
| 20 | Press Backspace in category picker | Category cleared from shelf, picker closes | PENDING | |
| 21 | Tag shelf, exit game, reload | Category assignment persists across sessions | PENDING | |
| 22 | Press H to toggle floating labels | Labels toggle on/off with notification | PENDING | |
| 23 | Toggle labels in config window (F7) | Checkbox syncs with floating label visibility | PENDING | |
| 24 | Place wrong-category product on tagged shelf | Placement blocked with notification | PENDING | |
| 25 | Employee puts away product near tagged shelf | Employee prefers category-matched storage | PENDING | |
| 26 | Load save with corrupt category data | Corrupt entries purged, valid entries preserved | PENDING | |
| 27 | Delete tagged shelf, wait 120s+ | Stale assignment purged from JSON (log confirms) | PENDING | |
| 28 | Exit to menu while shelves are tagged | Purge does NOT run during scene transition (log shows no purge) | PENDING | |
| 29 | Delete >50% of tagged shelves at once | Purge aborts with "ABORTED" warning in log | PENDING | |
| 30 | Delete category JSON, keep backup | On next load, auto-restore from backup (log confirms) | PENDING | |
| 31 | Open F7 config window | Scrollable, all settings visible including category shelf options | PENDING | |
| 32 | Automation purchases items | Notification shows box count and total spend | PENDING | |
