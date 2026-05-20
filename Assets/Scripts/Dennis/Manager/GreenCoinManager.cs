using Extra;

namespace Manager
{
    public class GreenCoinManager : Singleton<GreenCoinManager>
    {
        public int currentGold;
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddGold(int amount)
        {
            currentGold += amount;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public bool SpendGold(int amount)
        {
            var canSpendGold = false;
            if (amount <= currentGold)
            {
                canSpendGold = true;
                currentGold -=  amount;
            }
            return canSpendGold;
        }
    }
}