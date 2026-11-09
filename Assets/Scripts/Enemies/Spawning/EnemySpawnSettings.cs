using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Spawn rules for one arena or stage: what spawns, how often, how many at once and where it is allowed.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySpawnSettings", menuName = "Vyrden/Enemies/Enemy Spawn Settings")]
    public class EnemySpawnSettings : ScriptableObject
    {
        [Header("What")]
        [SerializeField] private List<EnemySpawnEntry> _enemies = new List<EnemySpawnEntry>();

        [Header("When")]
        [Tooltip("Seconds before the first spawn after spawning starts.")]
        [SerializeField, Min(0f)] private float _initialDelay = 1f;
        [Tooltip("Seconds between spawns while below the active limit.")]
        [SerializeField, Min(0.05f)] private float _spawnInterval = 3f;
        [Tooltip("Maximum enemies from this spawner alive at the same time.")]
        [SerializeField, Min(1)] private int _maxActiveEnemies = 5;

        [Header("Where")]
        [Tooltip("Enemies never spawn closer than this to any player.")]
        [SerializeField, Min(0f)] private float _minDistanceFromPlayers = 10f;
        [Tooltip("Random positions tried per spawn before giving up until the retry delay passes.")]
        [SerializeField, Range(1, 32)] private int _maxPlacementAttempts = 10;
        [Tooltip("Seconds to wait after a spawn found no valid position.")]
        [SerializeField, Min(0.05f)] private float _retryDelay = 0.5f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask _groundLayers = ~0;
        [Tooltip("Height above the candidate point the ground ray starts from.")]
        [SerializeField, Min(0.1f)] private float _groundProbeHeight = 10f;
        [Tooltip("How far down the ground ray looks.")]
        [SerializeField, Min(0.1f)] private float _groundProbeDistance = 30f;
        [SerializeField, Range(0f, 89f)] private float _maxGroundSlope = 40f;
        [Tooltip("Max height difference between the spawn location and the ground found, so enemies don't land on roofs or wall tops.")]
        [SerializeField, Min(0f)] private float _maxHeightFromLocation = 1.5f;

        [Header("Clearance")]
        [Tooltip("Free space needed at the spawn point (roughly the enemy's capsule).")]
        [SerializeField, Min(0.05f)] private float _clearanceRadius = 0.5f;
        [SerializeField, Min(0.1f)] private float _clearanceHeight = 1.8f;

        public IReadOnlyList<EnemySpawnEntry> Enemies => _enemies;
        public float InitialDelay => _initialDelay;
        public float SpawnInterval => _spawnInterval;
        public int MaxActiveEnemies => _maxActiveEnemies;
        public float MinDistanceFromPlayers => _minDistanceFromPlayers;
        public int MaxPlacementAttempts => _maxPlacementAttempts;
        public float RetryDelay => _retryDelay;
        public LayerMask GroundLayers => _groundLayers;
        public float GroundProbeHeight => _groundProbeHeight;
        public float GroundProbeDistance => _groundProbeDistance;
        public float MaxGroundSlope => _maxGroundSlope;
        public float MaxHeightFromLocation => _maxHeightFromLocation;
        public float ClearanceRadius => _clearanceRadius;
        public float ClearanceHeight => _clearanceHeight;

        /// <summary>Weighted random pick. Returns null when the table is empty or all weights are zero.</summary>
        public GameObject PickPrefab(System.Random random)
        {
            float total = 0f;
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].Prefab != null)
                {
                    total += _enemies[i].Weight;
                }
            }

            if (total <= 0f)
            {
                return null;
            }

            float roll = (float)random.NextDouble() * total;
            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemySpawnEntry entry = _enemies[i];
                if (entry.Prefab == null || entry.Weight <= 0f)
                {
                    continue;
                }

                roll -= entry.Weight;
                if (roll < 0f)
                {
                    return entry.Prefab;
                }
            }

            return null;
        }
    }
}
