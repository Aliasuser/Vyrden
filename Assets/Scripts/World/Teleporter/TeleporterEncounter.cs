using System;
using System.Collections.Generic;
using UnityEngine;
using Vyrden.Combat;
using Vyrden.Enemies;
using Vyrden.Progression;

namespace Vyrden.World
{
    /// <summary>
    /// The teleporter's charging event: Inactive → Charging → Charged. While charging it runs its own
    /// spawner, tracks whether a living player is inside the zone and how many enemies are, and moves the
    /// progress with <see cref="TeleporterChargeCalculator"/>. At 100% it stops its spawner and tells
    /// <see cref="StageProgression"/> the stage is complete. Knows nothing about input or interaction.
    /// </summary>
    public class TeleporterEncounter : MonoBehaviour
    {
        [SerializeField] private TeleporterEncounterSettings _settings;
        [Tooltip("Spawner used only during the event. Should not spawn on start.")]
        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private StageProgression _stageProgression;
        [Tooltip("Centre of the zone. Defaults to this transform.")]
        [SerializeField] private Transform _zoneCenter;

        private readonly List<GameObject> _encounterEnemies = new List<GameObject>();
        private TeleporterState _state = TeleporterState.Inactive;
        private float _progress;
        private bool _playerInZone;
        private int _enemiesInZone;
        private int _kills;

        /// <summary>Raised on every state change. Arg: new state.</summary>
        public event Action<TeleporterState> StateChanged;

        /// <summary>Raised when progress changes. Arg: progress 0..1.</summary>
        public event Action<float> ProgressChanged;

        /// <summary>Raised when a player enters (true) or the last player leaves (false) the zone while charging.</summary>
        public event Action<bool> PlayerInZoneChanged;

        /// <summary>Raised once when charging reaches 100%.</summary>
        public event Action Completed;

        public TeleporterState State => _state;
        public float Progress => _progress;
        public bool IsPlayerInZone => _playerInZone;
        public int EnemiesInZone => _enemiesInZone;
        public int Kills => _kills;
        public bool IsContested => _settings != null && TeleporterChargeCalculator.IsContested(_settings, _enemiesInZone);
        public TeleporterEncounterSettings Settings => _settings;
        public EnemySpawner Spawner => _spawner;
        public StageProgression StageProgression => _stageProgression;
        public float ZoneRadius => _settings != null ? _settings.ZoneRadius : 0f;
        public Vector3 ZoneCenter => _zoneCenter != null ? _zoneCenter.position : transform.position;

        private void Awake()
        {
            if (_settings == null || _spawner == null)
            {
                Debug.LogError($"{nameof(TeleporterEncounter)} on '{name}' needs settings and an encounter spawner.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_spawner != null)
            {
                _spawner.EnemySpawned += OnEnemySpawned;
            }
        }

        private void OnDisable()
        {
            if (_spawner != null)
            {
                _spawner.EnemySpawned -= OnEnemySpawned;
            }
        }

        /// <summary>Starts the charging event. Returns false unless the teleporter was inactive.</summary>
        public bool Begin()
        {
            if (!enabled || _state != TeleporterState.Inactive)
            {
                return false;
            }

            _progress = 0f;
            _kills = 0;
            SetState(TeleporterState.Charging);

            if (_settings.EncounterSpawnSettings != null)
            {
                _spawner.SetSettings(_settings.EncounterSpawnSettings);
            }

            _spawner.StartSpawning();
            ProgressChanged?.Invoke(_progress);
            return true;
        }

        private void Update()
        {
            if (_state != TeleporterState.Charging)
            {
                return;
            }

            UpdateZoneOccupancy();
            SetProgress(TeleporterChargeCalculator.Step(_settings, _progress, Time.deltaTime, _playerInZone, _enemiesInZone));
        }

        private void UpdateZoneOccupancy()
        {
            Vector3 center = ZoneCenter;
            float radiusSqr = _settings.ZoneRadius * _settings.ZoneRadius;
            bool playerInZone = false;
            int enemies = 0;

            IReadOnlyList<TeamMember> members = TeamMember.Active;
            for (int i = 0; i < members.Count; i++)
            {
                TeamMember member = members[i];
                if (!member.IsAlive || (member.transform.position - center).sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                if (member.Team == Team.Player)
                {
                    playerInZone = true;
                }
                else if (member.Team == Team.Enemy)
                {
                    enemies++;
                }
            }

            _enemiesInZone = enemies;
            if (playerInZone != _playerInZone)
            {
                _playerInZone = playerInZone;
                PlayerInZoneChanged?.Invoke(_playerInZone);
            }
        }

        private void OnEnemySpawned(GameObject enemy)
        {
            if (_state != TeleporterState.Charging || !enemy.TryGetComponent(out Health health))
            {
                return;
            }

            _encounterEnemies.Add(enemy);
            health.Died += _ => OnEncounterEnemyDied(enemy);
        }

        private void OnEncounterEnemyDied(GameObject enemy)
        {
            _encounterEnemies.Remove(enemy);
            if (_state != TeleporterState.Charging)
            {
                return;
            }

            _kills++;
            SetProgress(TeleporterChargeCalculator.AddKill(_settings, _progress));
        }

        private void SetProgress(float progress)
        {
            if (Mathf.Approximately(progress, _progress) && progress < 1f)
            {
                return;
            }

            _progress = progress;
            ProgressChanged?.Invoke(_progress);

            if (_progress >= 1f && _state == TeleporterState.Charging)
            {
                Complete();
            }
        }

        private void Complete()
        {
            _spawner.StopSpawning();
            _encounterEnemies.Clear();
            _enemiesInZone = 0;
            if (_playerInZone)
            {
                _playerInZone = false; // The zone stops mattering once charged.
                PlayerInZoneChanged?.Invoke(false);
            }

            SetState(TeleporterState.Charged);
            Completed?.Invoke();

            if (_stageProgression != null)
            {
                _stageProgression.CompleteStage();
            }
        }

        private void SetState(TeleporterState state)
        {
            _state = state;
            StateChanged?.Invoke(_state);
        }

        private void OnDrawGizmosSelected()
        {
            if (_settings == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.6f, 0.1f);
            Gizmos.DrawWireSphere(ZoneCenter, _settings.ZoneRadius);
        }
    }
}
