using UnityEngine;

namespace Vyrden.Progression
{
    /// <summary>
    /// Pure difficulty math. The coefficient is a smooth function of time:
    /// (1 + intervals × perInterval) × compound^intervals, where intervals = time / interval length × preset.
    /// Every effect is derived from the coefficient, so this is the only place difficulty is calculated.
    /// </summary>
    public static class DifficultyCalculator
    {
        public static DifficultyState Evaluate(DifficultySettings settings, float elapsedSeconds)
        {
            if (settings == null)
            {
                return DifficultyState.Baseline;
            }

            float coefficient = GetCoefficient(settings, elapsedSeconds);
            float above = coefficient - 1f;

            float health = 1f + above * settings.EnemyHealthPerCoefficient;
            float damage = 1f + above * settings.EnemyDamagePerCoefficient;
            float interval = Mathf.Max(settings.MinSpawnIntervalMultiplier, 1f / (1f + above * settings.SpawnRatePerCoefficient));
            int maxActiveBonus = Mathf.Min(settings.MaxActiveBonusCap, Mathf.FloorToInt(above * settings.MaxActivePerCoefficient));

            return new DifficultyState(coefficient, GetTierIndex(settings, coefficient), health, damage, interval, maxActiveBonus);
        }

        public static float GetCoefficient(DifficultySettings settings, float elapsedSeconds)
        {
            float intervals = Mathf.Max(0f, elapsedSeconds) / settings.SecondsPerInterval * settings.PresetMultiplier;
            float coefficient = (1f + intervals * settings.CoefficientPerInterval) * Mathf.Pow(settings.CompoundPerInterval, intervals);

            if (float.IsNaN(coefficient) || float.IsInfinity(coefficient))
            {
                coefficient = float.MaxValue;
            }

            if (settings.MaxCoefficient > 0f)
            {
                coefficient = Mathf.Min(coefficient, Mathf.Max(1f, settings.MaxCoefficient));
            }

            return Mathf.Max(1f, coefficient);
        }

        /// <summary>Highest tier whose minimum the coefficient has reached, or -1.</summary>
        public static int GetTierIndex(DifficultySettings settings, float coefficient)
        {
            DifficultyTier[] tiers = settings.Tiers;
            int index = -1;
            if (tiers == null)
            {
                return index;
            }

            for (int i = 0; i < tiers.Length; i++)
            {
                if (coefficient >= tiers[i].MinCoefficient)
                {
                    index = i;
                }
            }

            return index;
        }
    }
}
