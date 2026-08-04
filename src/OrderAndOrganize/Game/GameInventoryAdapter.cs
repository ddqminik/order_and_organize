using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Game
{
    /// <summary>
    /// Wraps ManagerBlackboard.GetProductsExistences (private method).
    /// Returns int[3]: [0]=OnShelves(red), [1]=InStorage(green), [2]=InBoxes/Movement(yellow).
    /// </summary>
    public class GameInventoryAdapter
    {
        private readonly ManualLogSource _log;
        private MethodInfo _getProductsExistences;
        private bool _resolved;

        public GameInventoryAdapter(ManualLogSource log)
        {
            _log = log;
        }

        public bool Resolve()
        {
            if (_resolved) return _getProductsExistences != null;

            _getProductsExistences = typeof(ManagerBlackboard).GetMethod(
                "GetProductsExistences",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            _resolved = true;

            if (_getProductsExistences == null)
            {
                _log.LogError("Failed to resolve ManagerBlackboard.GetProductsExistences");
                return false;
            }

            if (_getProductsExistences.ReturnType != typeof(int[]))
            {
                _log.LogError($"GetProductsExistences return type mismatch: expected int[], got {_getProductsExistences.ReturnType}");
                _getProductsExistences = null;
                return false;
            }

            _log.LogInfo("Resolved ManagerBlackboard.GetProductsExistences successfully");
            return true;
        }

        /// <summary>
        /// Returns stock array [shelves, storage, boxes/movement] for a product,
        /// or null on failure.
        /// </summary>
        public int[] GetProductExistences(ManagerBlackboard blackboard, int productId)
        {
            if (_getProductsExistences == null) return null;

            try
            {
                return (int[])_getProductsExistences.Invoke(blackboard, new object[] { productId });
            }
            catch (System.Exception ex)
            {
                _log.LogWarning($"GetProductsExistences failed for product {productId}: {ex.Message}");
                return null;
            }
        }
    }
}
