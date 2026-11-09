using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Somewhere an <see cref="EnemySpawner"/> may place enemies. The spawner still checks ground,
    /// clearance, arena bounds and player distance for every sampled point.
    /// </summary>
    public abstract class SpawnLocation : MonoBehaviour
    {
        [Tooltip("How often this location is picked relative to the spawner's other locations.")]
        [SerializeField, Min(0f)] private float _weight = 1f;

        public float Weight => _weight;

        /// <summary>A random candidate point in world space. Height is corrected by the ground check.</summary>
        public abstract Vector3 SamplePoint(System.Random random);

        protected static float Range(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
    }
}
