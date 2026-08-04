using System.Collections;
using BepInEx.Logging;
using OrderAndOrganize.Models;
using UnityEngine;

namespace OrderAndOrganize.Game
{
    /// <summary>
    /// Handles purchasing through the game's native BuyCargo flow.
    /// Since BuyCargo() buys the entire shopping list, we must:
    /// 1. Verify the shopping list is empty (no manual entries)
    /// 2. Add exactly one product
    /// 3. Wait for price calculation
    /// 4. Call BuyCargo
    /// </summary>
    public class GamePurchaseAdapter
    {
        private readonly ManualLogSource _log;
        private readonly GameShoppingListAdapter _shoppingList;
        private readonly GameMoneyAdapter _money;

        public GamePurchaseAdapter(ManualLogSource log, GameShoppingListAdapter shoppingList, GameMoneyAdapter money)
        {
            _log = log;
            _shoppingList = shoppingList;
            _money = money;
        }

        public bool IsShoppingListEmpty(ManagerBlackboard blackboard)
        {
            return _shoppingList.GetShoppingListCount(blackboard) == 0;
        }

        /// <summary>
        /// Purchases a single product box through the native flow.
        /// Must be called from a coroutine context; yields to allow price calculation.
        /// Returns a PurchaseResult via the callback.
        /// </summary>
        public IEnumerator PurchaseSingleProduct(
            ManagerBlackboard blackboard,
            int productId,
            string productName,
            float boxPrice,
            float cashReserve,
            System.Action<PurchaseResult> onComplete)
        {
            float moneyBefore = _money.GetCurrentMoney();
            float spendable = moneyBefore - cashReserve;

            if (boxPrice > spendable)
            {
                onComplete?.Invoke(PurchaseResult.Failed(productId, productName, boxPrice,
                    $"Insufficient funds: need {boxPrice:F2}, have {spendable:F2} spendable (reserve={cashReserve:F2})"));
                yield break;
            }

            if (!IsShoppingListEmpty(blackboard))
            {
                onComplete?.Invoke(PurchaseResult.Failed(productId, productName, boxPrice,
                    "Shopping list contains manual entries; skipping to avoid unintended purchases."));
                yield break;
            }

            _shoppingList.AddToShoppingList(blackboard, productId, boxPrice);

            // Wait two frames for CalculateShoppingListTotal coroutine to complete
            yield return null;
            yield return new WaitForEndOfFrame();

            float moneyBeforeBuy = _money.GetCurrentMoney();
            spendable = moneyBeforeBuy - cashReserve;

            if (boxPrice > spendable)
            {
                // Funds changed between adding and buying; remove the item
                blackboard.RemoveShoppingListProduct(0);
                onComplete?.Invoke(PurchaseResult.Failed(productId, productName, boxPrice,
                    $"Funds changed during purchase: need {boxPrice:F2}, have {spendable:F2} spendable."));
                yield break;
            }

            blackboard.BuyCargo();

            // Wait a frame for the purchase to process
            yield return null;

            float moneyAfter = _money.GetCurrentMoney();
            bool moneyDeducted = moneyAfter < moneyBeforeBuy - 0.01f;

            if (moneyDeducted)
            {
                _log.LogInfo($"Purchased {productName} (ID={productId}) for ${boxPrice:F2}. Money: ${moneyBeforeBuy:F2} -> ${moneyAfter:F2}");
                onComplete?.Invoke(PurchaseResult.Succeeded(productId, productName, boxPrice, moneyBeforeBuy, moneyAfter));
            }
            else
            {
                _log.LogWarning($"Purchase of {productName} may have failed: money did not decrease. Before={moneyBeforeBuy:F2}, After={moneyAfter:F2}");
                onComplete?.Invoke(PurchaseResult.Failed(productId, productName, boxPrice,
                    "Money was not deducted; purchase may have been rejected by the game."));
            }
        }
    }
}
