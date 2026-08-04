using BepInEx.Logging;
using Mirror;

namespace OrderAndOrganize.Game
{
    public class GameAuthorityAdapter
    {
        private readonly ManualLogSource _log;

        public GameAuthorityAdapter(ManualLogSource log)
        {
            _log = log;
        }

        /// <summary>
        /// Returns true if the local instance is the authoritative host
        /// (server/host in Mirror networking, or singleplayer).
        /// </summary>
        public bool IsLocalAuthority()
        {
            return NetworkServer.active;
        }

        /// <summary>
        /// Returns true if a game world/store is currently loaded
        /// (ManagerBlackboard and GameData singletons exist).
        /// </summary>
        public bool IsStoreLoaded()
        {
            return GameData.Instance != null &&
                   ProductListing.Instance != null;
        }
    }
}
