namespace Vyrden.Enemies
{
    /// <summary>A spawner that <see cref="SpawnedEnemyTracker"/> reports removals to.</summary>
    internal interface ISpawnedEnemyOwner
    {
        /// <summary>Called exactly once per tracked enemy, when it dies or is destroyed.</summary>
        void NotifyRemoved(SpawnedEnemyTracker tracker);
    }
}
