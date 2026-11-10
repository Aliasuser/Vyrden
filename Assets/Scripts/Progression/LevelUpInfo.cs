namespace Vyrden.Progression
{
    /// <summary>One level-up event. A single reward can cover several levels.</summary>
    public readonly struct LevelUpInfo
    {
        public readonly int PreviousLevel;
        public readonly int NewLevel;

        public LevelUpInfo(int previousLevel, int newLevel)
        {
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
        }

        public int LevelsGained => NewLevel - PreviousLevel;
    }
}
