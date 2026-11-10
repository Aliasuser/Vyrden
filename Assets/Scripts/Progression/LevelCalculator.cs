namespace Vyrden.Progression
{
    /// <summary>Pure level math: turns an experience gain into new level and leftover progress.</summary>
    public static class LevelCalculator
    {
        /// <summary>Guard against a misconfigured curve turning one reward into an endless loop.</summary>
        public const int MaxLevelsPerGain = 1000;

        /// <summary>
        /// Adds <paramref name="amount"/> experience to the progress within the current level, levelling up as
        /// many times as it pays for. Leftover experience carries into the next level. At the level cap the
        /// progress stays at 0. Returns the number of levels gained.
        /// </summary>
        public static int AddExperience(IExperienceCurve curve, ref int level, ref float experienceIntoLevel, float amount)
        {
            if (curve == null || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
            {
                return 0;
            }

            if (IsMaxLevel(curve, level))
            {
                experienceIntoLevel = 0f;
                return 0;
            }

            int startLevel = level;
            experienceIntoLevel += amount;

            float requirement = curve.GetRequirement(level);
            while (experienceIntoLevel >= requirement && level - startLevel < MaxLevelsPerGain)
            {
                experienceIntoLevel -= requirement;
                level++;

                if (IsMaxLevel(curve, level))
                {
                    experienceIntoLevel = 0f;
                    break;
                }

                requirement = curve.GetRequirement(level);
            }

            return level - startLevel;
        }

        public static bool IsMaxLevel(IExperienceCurve curve, int level)
        {
            return curve.MaxLevel > 0 && level >= curve.MaxLevel;
        }
    }
}
