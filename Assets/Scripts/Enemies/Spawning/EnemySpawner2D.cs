using System;
using System.Collections.Generic;
using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Spawns enemies on the XY plane from an <see cref="EnemySpawnSettings2D"/> table, at its
    /// <see cref="SpawnLocation2D"/>s (or anywhere in the room when it has none). Every candidate must be inside
    /// the room bounds with room for the enemy's body, free of walls, rocks and characters, and far enough from
    /// players. Tracks which spawned enemies are alive and frees their slot when they die or are destroyed.
    /// Knows nothing about enemy AI (any prefab works; <see cref="IRoomBoundsReceiver"/>s get the room bounds)
    /// or about rooms (a room calls <see cref="SetRoomBounds"/>, <see cref="StartSpawning"/> and listens to
    /// <see cref="Cleared"/>).
    /// </summary>
    public class EnemySpawner2D : MonoBehaviour, ISpawnedEnemyOwner
    {
        private const int MaxOverlapResults = 4;

        [SerializeField] private EnemySpawnSettings2D _settings;
        [Tooltip("Where enemies may appear. Empty = anywhere inside the room bounds.")]
        [SerializeField] private List<SpawnLocation2D> _locations = new List<SpawnLocation2D>();

        [Header("Room")]
        [Tooltip("When on, enemies only spawn inside the room bounds and are told the bounds.")]
        [SerializeField] private bool _useRoomBounds = true;
        [Tooltip("Room interior on the XY plane (inside the walls).")]
        [SerializeField] private Rect _roomBounds = new Rect(-7f, -4f, 14f, 8f);

        [Header("Behaviour")]
        [Tooltip("Optional parent for spawned enemies, to keep the hierarchy tidy.")]
        [SerializeField] private Transform _container;
        [SerializeField] private bool _spawnOnStart = true;
        [Tooltip("Random seed for placement and enemy choice. 0 = different every run.")]
        [SerializeField] private int _seed;
        [Tooltip("Enemies keep their spawn distance from living members of this team.")]
        [SerializeField] private Team _avoidTeam = Team.Player;

        private readonly List<GameObject> _activeEnemies = new List<GameObject>();
        private readonly Collider2D[] _overlapResults = new Collider2D[MaxOverlapResults];
        private System.Random _random;
        private ContactFilter2D _blockingFilter;
        private float _timer;
        private bool _isSpawning;
        private int _spawnedCount;
        private bool _clearedRaised;

        /// <summary>Raised after an enemy is created and registered.</summary>
        public event Action<GameObject> EnemySpawned;

        /// <summary>Raised when a tracked enemy dies or is destroyed.</summary>
        public event Action<GameObject> EnemyRemoved;

        /// <summary>Raised once when a limited encounter has spawned all its enemies and all of them are gone.</summary>
        public event Action Cleared;

        public EnemySpawnSettings2D Settings => _settings;
        public IReadOnlyList<GameObject> ActiveEnemies => _activeEnemies;
        public int ActiveCount => _activeEnemies.Count;
        public int SpawnedCount => _spawnedCount;
        public bool IsSpawning => _isSpawning;
        public bool UsesRoomBounds => _useRoomBounds;
        public Rect RoomBounds => _roomBounds;

        /// <summary>True when the settings' total spawn count has been reached.</summary>
        public bool HasSpawnedAll => _settings != null && _settings.HasSpawnLimit && _spawnedCount >= _settings.TotalSpawns;

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

        /// <summary>Starts (or resumes) spawning. The first spawn comes after the settings' initial delay.</summary>
        public void StartSpawning()
        {
            if (_isSpawning)
            {
                return;
            }

            if (_settings == null || (_locations.Count == 0 && !_useRoomBounds))
            {
                Debug.LogWarning($"{nameof(EnemySpawner2D)} on '{name}' needs spawn settings and spawn locations or room bounds.", this);
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

        /// <summary>Swaps the spawn rules (e.g. a room's encounter) and restarts the total count.</summary>
        public void SetSettings(EnemySpawnSettings2D settings)
        {
            _settings = settings;
            ResetEncounter();
        }

        /// <summary>Restarts the total spawn count, e.g. when a room's encounter is replayed.</summary>
        public void ResetEncounter()
        {
            _spawnedCount = 0;
            _clearedRaised = false;
        }

        /// <summary>Sets the room this spawner fills. Enemies spawned from now on get these bounds.</summary>
        public void SetRoomBounds(Rect bounds)
        {
            _roomBounds = bounds;
            _useRoomBounds = true;
        }

        private void Update()
        {
            if (!_isSpawning)
            {
                return;
            }

            if (HasSpawnedAll)
            {
                _isSpawning = false;
                CheckCleared();
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
        /// Spawns one enemy now if below the limits and a valid position is found.
        /// The single entry point for creating enemies (host-only in a future multiplayer build).
        /// </summary>
        public bool TrySpawn(out GameObject enemy)
        {
            enemy = null;
            if (_settings == null || _activeEnemies.Count >= _settings.MaxActiveEnemies || HasSpawnedAll)
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
                if (!TrySampleCandidate(out Vector2 candidate) || !IsValidSpawnPosition(candidate))
                {
                    continue;
                }

                enemy = Instantiate(prefab, new Vector3(candidate.x, candidate.y, 0f), Quaternion.identity, _container);
                if (_useRoomBounds)
                {
                    foreach (IRoomBoundsReceiver receiver in enemy.GetComponents<IRoomBoundsReceiver>())
                    {
                        receiver.SetRoomBounds(_roomBounds);
                    }
                }

                Register(enemy);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether an enemy may appear with its feet at <paramref name="position"/>: inside the room (with its
        /// clearance radius), not overlapping walls, rocks or characters, and not too close to a player.
        /// </summary>
        public bool IsValidSpawnPosition(Vector2 position)
        {
            if (_settings == null)
            {
                return false;
            }

            Vector2 centre = position + _settings.ClearanceOffset;
            float radius = _settings.ClearanceRadius;

            if (_useRoomBounds && !ContainsCircle(_roomBounds, centre, radius))
            {
                return false;
            }

            if (IsTooCloseToPlayers(position))
            {
                return false;
            }

            _blockingFilter = new ContactFilter2D { useLayerMask = true, layerMask = _settings.BlockingLayers, useTriggers = false };
            return Physics2D.OverlapCircle(centre, radius, _blockingFilter, _overlapResults) == 0;
        }

        private bool TrySampleCandidate(out Vector2 candidate)
        {
            if (_locations.Count == 0)
            {
                candidate = SampleRoom();
                return _useRoomBounds;
            }

            SpawnLocation2D location = PickLocation();
            candidate = location != null ? location.SamplePoint(_random) : default;
            return location != null;
        }

        private Vector2 SampleRoom()
        {
            float x = _roomBounds.xMin + (float)_random.NextDouble() * _roomBounds.width;
            float y = _roomBounds.yMin + (float)_random.NextDouble() * _roomBounds.height;
            return new Vector2(x, y);
        }

        private bool IsTooCloseToPlayers(Vector2 position)
        {
            float minSqr = _settings.MinDistanceFromPlayers * _settings.MinDistanceFromPlayers;
            IReadOnlyList<TeamMember> members = TeamMember.Active;

            for (int i = 0; i < members.Count; i++)
            {
                TeamMember member = members[i];
                if (member.Team != _avoidTeam || !member.IsAlive)
                {
                    continue;
                }

                Vector2 offset = new Vector2(member.transform.position.x, member.transform.position.y) - position;
                if (offset.sqrMagnitude < minSqr)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCircle(Rect rect, Vector2 centre, float radius)
        {
            return centre.x - radius >= rect.xMin && centre.x + radius <= rect.xMax
                && centre.y - radius >= rect.yMin && centre.y + radius <= rect.yMax;
        }

        private SpawnLocation2D PickLocation()
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
                SpawnLocation2D location = _locations[i];
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

        private static bool IsUsable(SpawnLocation2D location)
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
            _spawnedCount++;
            EnemySpawned?.Invoke(enemy);
        }

        void ISpawnedEnemyOwner.NotifyRemoved(SpawnedEnemyTracker tracker)
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
            CheckCleared();
        }

        private void CheckCleared()
        {
            if (_clearedRaised || !HasSpawnedAll || _activeEnemies.Count > 0)
            {
                return;
            }

            _clearedRaised = true;
            Cleared?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (!_useRoomBounds)
            {
                return;
            }

            Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.6f);
            Gizmos.DrawWireCube(new Vector3(_roomBounds.center.x, _roomBounds.center.y, 0f), new Vector3(_roomBounds.width, _roomBounds.height, 0f));
        }
    }
}
