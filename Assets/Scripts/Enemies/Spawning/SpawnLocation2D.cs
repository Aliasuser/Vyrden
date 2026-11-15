using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Somewhere an <see cref="EnemySpawner2D"/> may place enemies on the XY plane. The spawner still checks
    /// room bounds, free space and player distance for every sampled point.
    /// </summary>
    public abstract class SpawnLocation2D : MonoBehaviour
    {
        [Tooltip("How often this location is picked relative to the spawner's other locations.")]
        [SerializeField, Min(0f)] private float _weight = 1f;

        public float Weight => _weight;

        /// <summary>A random candidate point (the enemy's feet) on the XY plane.</summary>
        public abstract Vector2 SamplePoint(System.Random random);

        protected Vector2 Position => new Vector2(transform.position.x, transform.position.y);

        protected static float Range(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
    }
}
