using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Game
{
    public class GameMoneyAdapter
    {
        private readonly ManualLogSource _log;

        public GameMoneyAdapter(ManualLogSource log)
        {
            _log = log;
        }

        public float GetCurrentMoney()
        {
            if (GameData.Instance == null)
            {
                _log.LogWarning("GameData.Instance is null; cannot read gameFunds.");
                return 0f;
            }
            return GameData.Instance.gameFunds;
        }

        public float GetSpendableMoney(float cashReserve)
        {
            float current = GetCurrentMoney();
            float spendable = current - cashReserve;
            return spendable < 0f ? 0f : spendable;
        }

        public bool CanAfford(float boxPrice, float cashReserve)
        {
            return boxPrice <= GetSpendableMoney(cashReserve);
        }
    }
}
