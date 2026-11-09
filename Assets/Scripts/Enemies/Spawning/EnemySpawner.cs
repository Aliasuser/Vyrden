using System;
using System.Collections.Generic;
using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Spawns enemies from an <see cref="EnemySpawnSettings"/> table at its <see cref="SpawnLocation"/>s,
    /// keeps them on valid ground inside the arena and away from players, and tracks which are alive.
    /// Knows nothing about enemy AI; any prefab works.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private const float GroundClearanceMargin = 0.05f;
        private const int MaxOverlapResults = 8;

        [SerializeField] private EnemySpawnSettings _settings;
        [SerializeField] private List<SpawnLocation> _locations = new List<SpawnLocation>();
        [Tooltip("Optional. When set, enemies only spawn inside these bounds.")]
        [SerializeField] private ArenaBounds _arenaBounds;
        [Tooltip("Optional parent for spawned enemies, to keep the hierarchy tidy.")]
        [SerializeField] private Transform _container;
        [SerializeField] private bool _spawnOnStart = true;
        [Tooltip("Random seed for placement and enemy choice. 0 = different every run.")]
        [SerializeField] private int _seed;
        [Tooltip("Enemies keep their spawn distance from living members of this team.")]
        [SerializeField] private Team _avoidTeam = Team.Player;

        private readonly List<GameObject> _activeEnemies = new List<GameObject>();
        private readonly Collider[] _overlapResults = new Collider[MaxOverlapResults];
        private System.Random _random;
        private float _timer;
        private bool _isSpawning;

        /// <summary>Raised after an enemy is created and registered.</summary>
        public event Action<GameObject> EnemySpawned;

        /// <summary>Raised when a tracked enemy dies or is destroyed.</summary>
        public event Action<GameObject> EnemyRemoved;

        public EnemySpawnSettings Settings => _settings;
        public IReadOnlyList<GameObject> ActiveEnemies => _activeEnemies;
        public int ActiveCount => _activeEnemies.Count;
        public bool IsSpawning => _isSpawning;

        private void Awake()
        {
            _random = _seed != 0 ? new System.Random(_seed) : new System.Random(Environment.TickCount);
        }

        private void Start()
        {
            if (_spawnOnStart)
            {
                StartSpawning();
            }
        }

        public void StartSpawning()
        {
            if (_isSpawning)
            {
                return;
            }

            if (_settings == null || _locations.Count == 0)
            {
                Debug.LogWarning($"{nameof(EnemySpawner)} on '{name}' needs spawn settings and at least one spawn location.", this);
                return;
            }

            _isSpawning = true;
            _timer = _settings.InitialDelay;
        }

        /// <summary>Stops new spawns. Enemies already alive stay and are still tracked.</summary>
        public void StopSpawning()
        {
            _isSpawning = false;
        }

        /// <summary>Swaps the spawn rules, e.g. when a new stage or difficulty starts.</summary>
        public void SetSettings(EnemySpawnSettings settings)
        {
            _settings = settings;
        }

        private void Update()
        {
            if (!_isSpawning)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f || _activeEnemies.Count >= _settings.MaxActiveEnemies)
            {
                // At the limit the timer stays expired, so the next enemy comes as soon as a slot frees up.
                return;
            }

            _timer = TrySpawn(out _) ? _settings.SpawnInterval : _settings.RetryDelay;
        }

        /// <summary>
        /// Spawns one enemy now if below the limit and a valid position is found.
        /// This is the single entry point for creating enemies (host-only in a future multiplayer build).
        /// </summary>
        public bool TrySpawn(out GameObject enemy)
        {
            enemy = null;
            if (_settings == null || _locations.Count == 0 || _activeEnemies.Count >= _settings.MaxActiveEnemies)
            {
                return false;
            }

            GameObject prefab = _settings.PickPrefab(_random);
            if (prefab == null)
            {
                return false;
            }

            for (int attempt = 0; attempt < _settings.MaxPlacementAttempts; attempt++)
            {
                SpawnLocation location = PickLocation();
                if (location == null)
                {
                    return false;
                }

                if (!TryResolvePosition(location.SamplePoint(_random), out Vector3 position))
                {
                    continue;
                }

                var rotation = Quaternion.Euler(0f, (float)_random.NextDouble() * 360f, 0f);
                enemy = Instantiate(prefab, position, rotation, _container);
                Register(enemy);
                return true;
            }

            return false;
        }

        /// <summary>Checks ground, slope, arena bounds, player distance and free space for a candidate point.</summary>
        public bool TryResolvePosition(Vector3 candidate, out Vector3 position)
        {
            position = default;

            Vector3 rayOrigin = candidate + Vector3.up * _settings.GroundProbeHeight;
            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _settings.GroundProbeDistance,
                    _settings.GroundLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            // Don't stand on top of characters or moving physics objects.
            if (hit.collider is CharacterController || hit.collider.attachedRigidbody != null)
            {
                return false;
            }

            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > _settings.MaxGroundSlope || Mathf.Abs(hit.point.y - candidate.y) > _settings.MaxHeightFromLocation)
            {
                return false;
            }

            position = hit.point;

            if (_arenaBounds != null && !_arenaBounds.Contains(position))
            {
                return false;
            }

            if (IsTooCloseToPlayers(position))
            {
                return false;
            }

            return HasClearance(position, slope);
        }

        private bool IsTooCloseToPlayers(Vector3 position)
        {
            float minSqr = _settings.MinDistanceFromPlayers * _settings.MinDistanceFromPlayers;
            IReadOnlyList<TeamMember> members = TeamMember.Active;

            for (int i = 0; i < members.Count; i++)
            {
                TeamMember member = members[i];
                if (member.Team == _avoidTeam && member.IsAlive
                    && (member.transform.position - position).sqrMagnitude < minSqr)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasClearance(Vector3 groundPoint, float slope)
        {
            float radius = _settings.ClearanceRadius;
            // Lift the capsule enough that a sloped floor under it doesn't count as an obstacle.
            float lift = GroundClearanceMargin + radius * Mathf.Tan(slope * Mathf.Deg2Rad);
            Vector3 bottom = groundPoint + Vector3.up * (radius + lift);
            Vector3 top = groundPoint + Vector3.up * Mathf.Max(radius + lift, _settings.ClearanceHeight - radius);

            int hits = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, _overlapResults, ~0, QueryTriggerInteraction.Ignore);
            return hits == 0;
        }

        private SpawnLocation PickLocation()
        {
            float total = 0f;
            for (int i = 0; i < _locations.Count; i++)
            {
                if (IsUsable(_locations[i]))
                {
                    total += _locations[i].Weight;
                }
            }

            if (total <= 0f)
            {
                return null;
            }

            float roll = (float)_random.NextDouble() * total;
            for (int i = 0; i < _locations.Count; i++)
            {
                SpawnLocation location = _locations[i];
                if (!IsUsable(location))
                {
                    continue;
                }

                roll -= location.Weight;
                if (roll < 0f)
                {
                    return location;
                }
            }

            return null;
        }

        private static bool IsUsable(SpawnLocation location)
        {
            return location != null && location.isActiveAndEnabled && location.Weight > 0f;
        }

        private void Register(GameObject enemy)
        {
            if (!enemy.TryGetComponent(out SpawnedEnemyTracker tracker))
            {
                tracker = enemy.AddComponent<SpawnedEnemyTracker>();
            }

            tracker.Bind(this);
            _activeEnemies.Add(enemy);
            EnemySpawned?.Invoke(enemy);
        }

        internal void NotifyRemoved(SpawnedEnemyTracker tracker)
        {
            GameObject enemy = tracker.gameObject;
            int index = _activeEnemies.IndexOf(enemy);
            if (index < 0)
            {
                return;
            }

            int last = _activeEnemies.Count - 1;
            _activeEnemies[index] = _activeEnemies[last];
            _activeEnemies.RemoveAt(last);
            EnemyRemoved?.Invoke(enemy);
        }
    }
}
