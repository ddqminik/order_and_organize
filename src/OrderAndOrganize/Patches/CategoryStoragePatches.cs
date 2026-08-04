using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OrderAndOrganize.Services;
using OrderAndOrganize.UI;
using UnityEngine;

namespace OrderAndOrganize.Patches
{
    /// <summary>
    /// Harmony patches for category storage shelf feature.
    /// </summary>
    [HarmonyPatch]
    internal static class CategoryStoragePatches
    {
        internal static CategoryShelfManager ShelfManager;
        internal static CategoryMapper CategoryMapper;
        internal static bool Enabled;

        /// <summary>
        /// Prefix on Data_Container.GetStorageBox — restricts player placement by category.
        /// If the storage shelf has a category tag and the carried product doesn't match, block placement.
        /// </summary>
        [HarmonyPatch(typeof(Data_Container), "GetStorageBox")]
        [HarmonyPrefix]
        static bool OnGetStorageBox(Data_Container __instance, int boxIndex)
        {
            if (!Enabled || ShelfManager == null) return true;

            try
            {
                int? category = ShelfManager.GetCategory(__instance);
                if (!category.HasValue)
                    return true;

                // Only restrict when player is placing a product (equippedItem == 1)
                var fpc = GetFirstPersonController();
                if (fpc == null) return true;

                var playerNet = fpc.GetComponent<PlayerNetwork>();
                if (playerNet == null || playerNet.equippedItem != 1)
                    return true;

                int productId = playerNet.extraParameter1;
                if (productId < 0)
                    return true;

                if (!ShelfManager.IsProductAllowed(__instance, productId))
                {
                    string categoryName = CategoryMapper?.GetGroupName(category.Value) ?? $"Group {category.Value}";
                    try
                    {
                        GameCanvas.Instance?.CreateCanvasNotification($"`Wrong category! This shelf is for: {categoryName}");
                    }
                    catch { }

                    Plugin.Log?.LogDebug($"Category block: product {productId} rejected from shelf tagged '{categoryName}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"CategoryStoragePatches.OnGetStorageBox error: {ex.Message}");
            }

            return true;
        }

        /// <summary>
        /// Prefix on NPC_Manager.GetFreeStorageContainer — makes employees prefer
        /// category-matched storage when putting away leftover products.
        /// Replaces the original method with category-aware logic.
        /// </summary>
        [HarmonyPatch(typeof(NPC_Manager), "GetFreeStorageContainer")]
        [HarmonyPrefix]
        static bool OnGetFreeStorageContainer(NPC_Manager __instance, int boxIDProduct, ref int __result)
        {
            if (!Enabled || ShelfManager == null || CategoryMapper == null)
                return true;

            try
            {
                var storageOBJ = GetStorageOBJ(__instance);
                if (storageOBJ == null)
                    return true;

                Transform storageTransform = storageOBJ.transform;
                if (storageTransform.childCount == 0)
                {
                    __result = -1;
                    return false;
                }

                int productGroup = CategoryMapper.GetGroupForProduct(boxIDProduct);

                var storageDists = GetStorageDistanceRef(__instance);

                // Build 4 priority tiers:
                // 1. Category-matched labeled storage with same product (reserved empty slot)
                // 2. Category-matched labeled storage with any empty slot
                // 3. Non-tagged labeled storage (original game behavior)
                // 4. Unlabeled storage (original game behavior)
                var matchedLabeled = new List<int>();
                var otherLabeled = new List<int>();
                var unlabeled = new List<int>();

                for (int i = 0; i < storageTransform.childCount; i++)
                {
                    var dc = storageTransform.GetChild(i).GetComponent<Data_Container>();
                    if (dc == null) continue;

                    int? shelfCategory = ShelfManager.GetCategory(dc);
                    bool isLabeled = dc.containerID == 5;

                    if (shelfCategory.HasValue && shelfCategory.Value == productGroup)
                        matchedLabeled.Add(i);
                    else if (isLabeled && !shelfCategory.HasValue)
                        otherLabeled.Add(i);
                    else if (!isLabeled && !shelfCategory.HasValue)
                        unlabeled.Add(i);
                    // Category-tagged but wrong category: skip entirely
                }

                // Sort each tier by distance from reference point
                if (storageDists != Vector3.zero)
                {
                    matchedLabeled.Sort((a, b) =>
                        Vector3.Distance(storageDists, storageTransform.GetChild(a).position)
                        .CompareTo(Vector3.Distance(storageDists, storageTransform.GetChild(b).position)));
                    otherLabeled.Sort((a, b) =>
                        Vector3.Distance(storageDists, storageTransform.GetChild(a).position)
                        .CompareTo(Vector3.Distance(storageDists, storageTransform.GetChild(b).position)));
                    unlabeled.Sort((a, b) =>
                        Vector3.Distance(storageDists, storageTransform.GetChild(a).position)
                        .CompareTo(Vector3.Distance(storageDists, storageTransform.GetChild(b).position)));
                }

                // Search in priority order: matched > other labeled > unlabeled
                // Within each tier, first look for same-product reserved slots, then empty slots
                int result = SearchTierForSlot(storageTransform, matchedLabeled, boxIDProduct);
                if (result < 0)
                    result = SearchTierForSlot(storageTransform, otherLabeled, boxIDProduct);
                if (result < 0)
                    result = SearchTierForSlot(storageTransform, unlabeled, boxIDProduct);

                __result = result;
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"CategoryStoragePatches.OnGetFreeStorageContainer error: {ex.Message}");
                return true;
            }
        }

        private static int SearchTierForSlot(Transform storageTransform, List<int> indices, int boxIDProduct)
        {
            // Pass 1: same-product reserved but empty slot (labeled storage canvas sign scenario)
            foreach (int idx in indices)
            {
                if (idx >= storageTransform.childCount) continue;
                var dc = storageTransform.GetChild(idx).GetComponent<Data_Container>();
                if (dc == null) continue;

                int[] arr = dc.productInfoArray;
                int slotCount = arr.Length / 2;
                for (int s = 0; s < slotCount; s++)
                {
                    int pid = arr[s * 2];
                    int count = arr[s * 2 + 1];
                    Transform boxContainer = storageTransform.GetChild(idx).transform.Find("BoxContainer");
                    if (boxContainer != null
                        && s < boxContainer.childCount
                        && boxContainer.GetChild(s).childCount <= 0
                        && pid == boxIDProduct && count <= 0)
                    {
                        return idx;
                    }
                }
            }

            // Pass 2: completely empty slot
            foreach (int idx in indices)
            {
                if (idx >= storageTransform.childCount) continue;
                var dc = storageTransform.GetChild(idx).GetComponent<Data_Container>();
                if (dc == null) continue;

                int[] arr = dc.productInfoArray;
                int slotCount = arr.Length / 2;
                for (int s = 0; s < slotCount; s++)
                {
                    if (arr[s * 2] == -1)
                        return idx;
                }
            }

            return -1;
        }

        // --- Reflection helpers to access private/internal game fields ---

        private static FieldInfo _storageOBJField;
        private static FieldInfo _storageDistField;

        private static GameObject GetStorageOBJ(NPC_Manager instance)
        {
            if (_storageOBJField == null)
                _storageOBJField = AccessTools.Field(typeof(NPC_Manager), "storageOBJ");
            return _storageOBJField?.GetValue(instance) as GameObject;
        }

        private static Vector3 GetStorageDistanceRef(NPC_Manager instance)
        {
            if (_storageDistField == null)
                _storageDistField = AccessTools.Field(typeof(NPC_Manager), "storageDistanceReferencePoint");
            if (_storageDistField != null)
                return (Vector3)_storageDistField.GetValue(instance);
            return Vector3.zero;
        }

        private static MonoBehaviour GetFirstPersonController()
        {
            return CategoryPickerUI.FindFirstPersonController();
        }
    }
}
