using System;
using BepInEx.Logging;
using OrderAndOrganize.Game;
using UnityEngine;

namespace OrderAndOrganize.Diagnostics
{
    public class GameApiDiagnostics
    {
        private readonly ManualLogSource _log;
        private readonly GameInventoryAdapter _inventory;
        private readonly GameProductCatalogAdapter _catalog;
        private readonly GameShoppingListAdapter _shoppingList;

        public GameApiDiagnostics(
            ManualLogSource log,
            GameInventoryAdapter inventory,
            GameProductCatalogAdapter catalog,
            GameShoppingListAdapter shoppingList)
        {
            _log = log;
            _inventory = inventory;
            _catalog = catalog;
            _shoppingList = shoppingList;
        }

        public void LogDiagnosticsForProduct(int productId)
        {
            try
            {
                var listing = _catalog.GetProductListing();
                var blackboard = _catalog.GetManagerBlackboard();

                if (listing == null || blackboard == null)
                {
                    _log.LogWarning("Diagnostics: game managers not available.");
                    return;
                }

                if (!_inventory.Resolve())
                {
                    _log.LogWarning("Diagnostics: GetProductsExistences not resolved.");
                    return;
                }

                string name = _catalog.GetProductName(productId);
                int[] existences = _inventory.GetProductExistences(blackboard, productId);
                int unitsPerBox = _catalog.GetMaxItemsPerBox(listing, productId);
                float boxPrice = _catalog.GetBoxPrice(listing, productId);
                bool onList = _shoppingList.IsProductOnShoppingList(blackboard, productId);

                _log.LogInfo("=== DIAGNOSTIC: Product Stock Report ===");
                _log.LogInfo($"  ProductId        = {productId}");
                _log.LogInfo($"  ProductName      = {name}");

                if (existences != null && existences.Length >= 3)
                {
                    _log.LogInfo($"  Raw value [0]    = {existences[0]} (OnShelves / Red)");
                    _log.LogInfo($"  Raw value [1]    = {existences[1]} (InStorage / Green)");
                    _log.LogInfo($"  Raw value [2]    = {existences[2]} (InMovement / Yellow)");
                    _log.LogInfo($"  CombinedStock    = {existences[0] + existences[1] + existences[2]}");
                }
                else
                {
                    _log.LogWarning("  Stock values could not be read.");
                }

                _log.LogInfo($"  UnitsPerBox      = {unitsPerBox}");
                _log.LogInfo($"  BoxPrice         = {boxPrice:F2}");
                _log.LogInfo($"  IsOnShoppingList = {onList}");
                _log.LogInfo($"  GameFunds        = {GameData.Instance?.gameFunds:F2}");
                _log.LogInfo("========================================");
            }
            catch (Exception ex)
            {
                _log.LogError($"Diagnostics error: {ex}");
            }
        }

        public void LogResolvedApi()
        {
            _log.LogInfo("=== Order & Organize: Resolved Game API ===");

            _log.LogInfo($"  GameData.Instance: {(GameData.Instance != null ? "OK" : "NULL")}");
            _log.LogInfo($"  ProductListing.Instance: {(ProductListing.Instance != null ? "OK" : "NULL")}");
            _log.LogInfo($"  GameCanvas.Instance: {(GameCanvas.Instance != null ? "OK" : "NULL")}");

            var blackboard = _catalog.GetManagerBlackboard();
            _log.LogInfo($"  ManagerBlackboard: {(blackboard != null ? "OK" : "NULL")}");

            bool inventoryOk = _inventory.Resolve();
            _log.LogInfo($"  GetProductsExistences: {(inventoryOk ? "RESOLVED" : "FAILED")}");

            var listing = _catalog.GetProductListing();
            if (listing != null)
            {
                _log.LogInfo($"  Available products: {listing.availableProducts?.Count ?? 0}");
                _log.LogInfo($"  Products data length: {listing.productsData?.Length ?? 0}");
                _log.LogInfo($"  Tier inflation length: {listing.tierInflation?.Length ?? 0}");
                _log.LogInfo($"  Unlocked tiers length: {listing.unlockedProductTiers?.Length ?? 0}");
            }

            _log.LogInfo($"  NetworkServer.active: {Mirror.NetworkServer.active}");
            _log.LogInfo("========================================");
        }
    }
}
