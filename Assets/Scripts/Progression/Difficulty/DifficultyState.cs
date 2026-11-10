namespace Vyrden.Progression
{
    /// <summary>Difficulty at one moment of a run and everything derived from it.</summary>
    public readonly struct DifficultyState
    {
        public readonly float Coefficient;

        /// <summary>Index into the settings' tier list, -1 when no tier applies.</summary>
        public readonly int TierIndex;

        public readonly float EnemyHealthMultiplier;
        public readonly float EnemyDamageMultiplier;

        /// <summary>Multiplier on the spawner's base interval (below 1 = faster spawning).</summary>
        public readonly float SpawnIntervalMultiplier;

        public readonly int MaxActiveBonus;

        public DifficultyState(float coefficient, int tierIndex, float enemyHealthMultiplier, float enemyDamageMultiplier,
            float spawnIntervalMultiplier, int maxActiveBonus)
        {
            Coefficient = coefficient;
            TierIndex = tierIndex;
            EnemyHealthMultiplier = enemyHealthMultiplier;
            EnemyDamageMultiplier = enemyDamageMultiplier;
            SpawnIntervalMultiplier = spawnIntervalMultiplier;
            MaxActiveBonus = maxActiveBonus;
        }

        /// <summary>The start of a run: coefficient 1, nothing changed.</summary>
        public static DifficultyState Baseline => new DifficultyState(1f, -1, 1f, 1f, 1f, 0);
    }
}
