using System.Collections.Generic;
using UnityEngine;
using Vyrden.Core;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Spawn rules for a 2D room or encounter: what spawns, how often, how many at once, how many in total and
    /// where a spawn is allowed. One asset per encounter type; rooms pick an asset.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySpawnSettings2D", menuName = "Vyrden/Enemies/Enemy Spawn Settings 2D")]
    public class EnemySpawnSettings2D : ScriptableObject
    {
        [Header("What")]
        [SerializeField] private List<EnemySpawnEntry> _enemies = new List<EnemySpawnEntry>();

        [Header("When")]
        [Tooltip("Seconds before the first spawn after spawning starts.")]
        [SerializeField, Min(0f)] private float _initialDelay = 1f;
        [Tooltip("Seconds between spawns while below the active limit.")]
        [SerializeField, Min(0.05f)] private float _spawnInterval = 2.5f;
        [Tooltip("Maximum enemies from this spawner alive at the same time.")]
        [SerializeField, Min(1)] private int _maxActiveEnemies = 4;
        [Tooltip("Enemies spawned in total before the spawner finishes (an encounter). 0 = no limit.")]
        [SerializeField, Min(0)] private int _totalSpawns;

        [Header("Where")]
        [Tooltip("Enemies never spawn closer than this to a living player.")]
        [SerializeField, Min(0f)] private float _minDistanceFromPlayers = 3f;
        [Tooltip("Random positions tried per spawn before waiting for the retry delay.")]
        [SerializeField, Range(1, 32)] private int _maxPlacementAttempts = 12;
        [Tooltip("Seconds to wait after a spawn found no valid position.")]
        [SerializeField, Min(0.05f)] private float _retryDelay = 0.5f;

        [Header("Clearance")]
        [Tooltip("Free radius needed at the spawn point (about the enemy's feet collider, plus a little).")]
        [SerializeField, Min(0.05f)] private float _clearanceRadius = 0.35f;
        [Tooltip("Offset from the spawn pivot (the feet) to the centre of the clearance circle.")]
        [SerializeField] private Vector2 _clearanceOffset = new Vector2(0f, 0.25f);
        [Tooltip("Layers that make a spawn point occupied (walls, rocks, characters).")]
        [SerializeField] private LayerMask _blockingLayers = PhysicsLayers.ObstacleMask | PhysicsLayers.EnemyMask | PhysicsLayers.PlayerMask;

        public IReadOnlyList<EnemySpawnEntry> Enemies => _enemies;
        public float InitialDelay => _initialDelay;
        public float SpawnInterval => _spawnInterval;
        public int MaxActiveEnemies => _maxActiveEnemies;
        public int TotalSpawns => _totalSpawns;
        public bool HasSpawnLimit => _totalSpawns > 0;
        public float MinDistanceFromPlayers => _minDistanceFromPlayers;
        public int MaxPlacementAttempts => _maxPlacementAttempts;
        public float RetryDelay => _retryDelay;
        public float ClearanceRadius => _clearanceRadius;
        public Vector2 ClearanceOffset => _clearanceOffset;
        public LayerMask BlockingLayers => _blockingLayers;

        /// <summary>Weighted random pick. Returns null when the table is empty or all weights are zero.</summary>
        public GameObject PickPrefab(System.Random random)
        {
            return EnemySpawnTable.Pick(_enemies, random);
        }
    }
}
