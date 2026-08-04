using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Game
{
    public class GameUiAdapter
    {
        private readonly ManualLogSource _log;

        public GameUiAdapter(ManualLogSource log)
        {
            _log = log;
        }

        /// <summary>
        /// Finds the ordering UI's tab/button bar where native buttons live.
        /// The ordering tab parent is on the ManagerBlackboard's tabsOBJ.
        /// </summary>
        public GameObject FindOrderingTabParent()
        {
            var blackboard = Object.FindFirstObjectByType<ManagerBlackboard>();
            if (blackboard == null) return null;
            return blackboard.tabsOBJ;
        }

        /// <summary>
        /// Checks if the ordering interface is currently open/visible.
        /// </summary>
        public bool IsOrderingInterfaceOpen()
        {
            var parent = FindOrderingTabParent();
            return parent != null && parent.activeSelf;
        }
    }
}
