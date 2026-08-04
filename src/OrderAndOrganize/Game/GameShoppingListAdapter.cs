using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Game
{
    public class GameShoppingListAdapter
    {
        private readonly ManualLogSource _log;

        public GameShoppingListAdapter(ManualLogSource log)
        {
            _log = log;
        }

        public bool IsProductOnShoppingList(ManagerBlackboard blackboard, int productId)
        {
            if (blackboard?.shoppingListParent == null) return false;

            foreach (Transform item in blackboard.shoppingListParent.transform)
            {
                var interactable = item.GetComponent<InteractableData>();
                if (interactable != null && interactable.thisSkillIndex == productId)
                    return true;
            }
            return false;
        }

        public int GetShoppingListCount(ManagerBlackboard blackboard)
        {
            if (blackboard?.shoppingListParent == null) return 0;
            return blackboard.shoppingListParent.transform.childCount;
        }

        public void AddToShoppingList(ManagerBlackboard blackboard, int productId, float boxPrice)
        {
            blackboard.AddShoppingListProduct(productId, boxPrice);
            _log.LogDebug($"Added product {productId} to shopping list at price {boxPrice:F2}");
        }
    }
}
