using System;
using UnityEngine;

namespace Dennis.Manager
{
    public class GreenCoinManager : Singleton<GreenCoinManager>
    {
        [field: SerializeField] public int CurrentGold { get; private set; } = 10000;

        public static event Action<int> OnGoldChanged;

        public void AddGold(int amount)
        {
            CurrentGold += amount;
            OnGoldChanged?.Invoke(amount);
        }

        public void SetGold(int amount)
        {
            CurrentGold = amount;
            OnGoldChanged?.Invoke(amount);
        }

        public bool SpendGold(int amount)
        {
            if (amount > CurrentGold) return false;
            CurrentGold -= amount;
            OnGoldChanged?.Invoke(-amount);
            return true;
        }
    }
}