using UnityEngine;

namespace Dennis.Manager
{
    public class GreenCoinManager : Singleton<GreenCoinManager>
    {
        [field: SerializeField] public int CurrentGold { get; private set; } = 10000;
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddGold(int amount)
        {
            CurrentGold += amount;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public bool SpendGold(int amount)
        {
            var canSpendGold = false;
            if (amount <= CurrentGold)
            {
                canSpendGold = true;
                CurrentGold -=  amount;
            }
            return canSpendGold;
        }
    }
}