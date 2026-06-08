namespace Dennis.Manager
{
    public class GreenCoinManager : Singleton<GreenCoinManager>
    {
        public int CurrentGold { get; private set; } = 1000;
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