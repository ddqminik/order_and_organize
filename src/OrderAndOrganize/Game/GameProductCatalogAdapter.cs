using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Game
{
    public class GameProductCatalogAdapter
    {
        private readonly ManualLogSource _log;

        public GameProductCatalogAdapter(ManualLogSource log)
        {
            _log = log;
        }

        public ProductListing GetProductListing()
        {
            return ProductListing.Instance;
        }

        public ManagerBlackboard GetManagerBlackboard()
        {
            return Object.FindFirstObjectByType<ManagerBlackboard>();
        }

        public List<int> GetAvailableProducts(ProductListing listing)
        {
            return listing?.availableProducts;
        }

        public bool IsProductUnlocked(ProductListing listing, int productId)
        {
            if (listing?.productsData == null || productId < 0 || productId >= listing.productsData.Length)
                return false;

            int tier = listing.productsData[productId].productTier;
            if (tier < 0 || tier >= listing.unlockedProductTiers.Length)
                return false;

            return listing.unlockedProductTiers[tier];
        }

        public string GetProductName(int productId)
        {
            try
            {
                if (LocalizationManager.instance != null)
                    return LocalizationManager.instance.GetLocalizationString("product" + productId);
            }
            catch (System.Exception ex)
            {
                _log.LogWarning($"Failed to get localized name for product {productId}: {ex.Message}");
            }
            return $"Product_{productId}";
        }

        public int GetMaxItemsPerBox(ProductListing listing, int productId)
        {
            if (listing?.productsData == null || productId < 0 || productId >= listing.productsData.Length)
                return 0;
            return listing.productsData[productId].maxItemsPerBox;
        }

        public float GetBoxPrice(ProductListing listing, int productId)
        {
            if (listing?.productsData == null || productId < 0 || productId >= listing.productsData.Length)
                return 0f;

            var data = listing.productsData[productId];
            int tier = data.productTier;
            if (tier < 0 || tier >= listing.tierInflation.Length)
                return 0f;

            float pricePerUnit = data.basePricePerUnit * listing.tierInflation[tier];
            pricePerUnit = Mathf.Round(pricePerUnit * 100f) / 100f;
            float boxPrice = pricePerUnit * data.maxItemsPerBox;
            return Mathf.Round(boxPrice * 100f) / 100f;
        }
    }
}
