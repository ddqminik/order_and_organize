using System;
using System.Collections.Generic;
using BepInEx.Logging;
using OrderAndOrganize.Game;
using OrderAndOrganize.Models;
using UnityEngine;

namespace OrderAndOrganize.Services
{
    public class InventoryScanner
    {
        private readonly ManualLogSource _log;
        private readonly GameInventoryAdapter _inventory;
        private readonly GameProductCatalogAdapter _catalog;
        private readonly GameShoppingListAdapter _shoppingList;
        private readonly PendingOrderTracker _pendingTracker;
        private readonly bool _verbose;

        public InventoryScanner(
            ManualLogSource log,
            GameInventoryAdapter inventory,
            GameProductCatalogAdapter catalog,
            GameShoppingListAdapter shoppingList,
            PendingOrderTracker pendingTracker,
            bool verbose)
        {
            _log = log;
            _inventory = inventory;
            _catalog = catalog;
            _shoppingList = shoppingList;
            _pendingTracker = pendingTracker;
            _verbose = verbose;
        }

        public List<ProductStockSnapshot> ScanAll()
        {
            var snapshots = new List<ProductStockSnapshot>();

            var listing = _catalog.GetProductListing();
            var blackboard = _catalog.GetManagerBlackboard();

            if (listing == null || blackboard == null)
            {
                _log.LogWarning("Cannot scan: ProductListing or ManagerBlackboard not available.");
                return snapshots;
            }

            if (!_inventory.Resolve())
            {
                _log.LogError("Cannot scan: GetProductsExistences method not resolved.");
                return snapshots;
            }

            var availableProducts = _catalog.GetAvailableProducts(listing);
            if (availableProducts == null || availableProducts.Count == 0)
            {
                _log.LogDebug("No available products to scan.");
                return snapshots;
            }

            foreach (int productId in availableProducts)
            {
                try
                {
                    var snapshot = ScanProduct(blackboard, listing, productId);
                    if (snapshot != null)
                        snapshots.Add(snapshot);
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"Error scanning product {productId}: {ex.Message}");
                }
            }

            _log.LogDebug($"Scan complete: {snapshots.Count} products scanned.");
            return snapshots;
        }

        private ProductStockSnapshot ScanProduct(ManagerBlackboard blackboard, ProductListing listing, int productId)
        {
            if (listing.productsData == null || productId < 0 || productId >= listing.productsData.Length)
                return null;

            bool isUnlocked = _catalog.IsProductUnlocked(listing, productId);
            int[] existences = _inventory.GetProductExistences(blackboard, productId);
            if (existences == null || existences.Length < 3)
                return null;

            int onShelves = existences[0];
            int inStorage = existences[1];
            int inMovement = existences[2];

            int pendingUnits = 0;
            var pending = _pendingTracker?.GetPendingOrder(productId);
            if (pending != null)
            {
                int reflectedIncrease = Math.Max(0, inMovement - pending.InMovementBeforePurchase);
                pendingUnits = Math.Max(0, pending.OrderedUnits - reflectedIncrease);
            }

            string name = _catalog.GetProductName(productId);
            int unitsPerBox = _catalog.GetMaxItemsPerBox(listing, productId);
            float boxPrice = _catalog.GetBoxPrice(listing, productId);
            bool onList = _shoppingList.IsProductOnShoppingList(blackboard, productId);

            var snapshot = new ProductStockSnapshot
            {
                ProductId = productId,
                ProductName = name,
                OnShelves = onShelves,
                InStorage = inStorage,
                InMovement = inMovement,
                PendingUnreflectedUnits = pendingUnits,
                UnitsPerBox = unitsPerBox,
                BoxPrice = boxPrice,
                IsUnlocked = isUnlocked,
                IsOrderable = isUnlocked && unitsPerBox > 0,
                IsOnShoppingList = onList
            };

            if (_verbose)
            {
                _log.LogDebug(
                    $"Product={name} ProductId={productId} " +
                    $"OnShelves={onShelves} InStorage={inStorage} InMovement={inMovement} " +
                    $"PendingUnreflected={pendingUnits} CombinedStock={snapshot.CombinedStock} " +
                    $"EffectiveCombinedStock={snapshot.EffectiveCombinedStock} " +
                    $"UnitsPerBox={unitsPerBox} BoxPrice={boxPrice:F2} " +
                    $"AlreadyOnShoppingList={onList}");
            }

            return snapshot;
        }
    }
}
