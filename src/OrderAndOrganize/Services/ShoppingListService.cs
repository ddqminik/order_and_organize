using System.Collections.Generic;
using BepInEx.Logging;
using OrderAndOrganize.Game;
using OrderAndOrganize.Models;

namespace OrderAndOrganize.Services
{
    public class ShoppingListService
    {
        private readonly ManualLogSource _log;
        private readonly GameShoppingListAdapter _shoppingList;
        private readonly GameProductCatalogAdapter _catalog;

        public ShoppingListService(
            ManualLogSource log,
            GameShoppingListAdapter shoppingList,
            GameProductCatalogAdapter catalog)
        {
            _log = log;
            _shoppingList = shoppingList;
            _catalog = catalog;
        }

        public (int added, int skipped) AddCandidatesToShoppingList(
            IReadOnlyList<ProductOrderCandidate> candidates)
        {
            var blackboard = _catalog.GetManagerBlackboard();
            if (blackboard == null)
            {
                _log.LogError("Cannot add to shopping list: ManagerBlackboard not found.");
                return (0, 0);
            }

            int added = 0;
            int skipped = 0;

            foreach (var candidate in candidates)
            {
                if (_shoppingList.IsProductOnShoppingList(blackboard, candidate.ProductId))
                {
                    _log.LogDebug($"Skipping {candidate.ProductName}: already on shopping list.");
                    skipped++;
                    continue;
                }

                _shoppingList.AddToShoppingList(blackboard, candidate.ProductId, candidate.BoxPrice);
                added++;
                _log.LogInfo($"Added {candidate.ProductName} (ID={candidate.ProductId}) to shopping list. Price=${candidate.BoxPrice:F2}");
            }

            return (added, skipped);
        }
    }
}
