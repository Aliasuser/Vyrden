using UnityEngine;
using Vyrden.Enemies;

namespace Vyrden.World
{
    /// <summary>Tuning for one teleporter charging event. Pure data; the rules are in <see cref="TeleporterChargeCalculator"/>.</summary>
    [CreateAssetMenu(fileName = "TeleporterEncounterSettings", menuName = "Vyrden/World/Teleporter Encounter Settings")]
    public class TeleporterEncounterSettings : ScriptableObject
    {
        [Header("Charging")]
        [Tooltip("Seconds to charge from 0 to 100% with a player in the zone and no slowdowns.")]
        [SerializeField, Min(1f)] private float _chargeDuration = 60f;
        [Tooltip("Radius of the zone a player must stay in for charging to continue.")]
        [SerializeField, Min(1f)] private float _zoneRadius = 12f;
        [Tooltip("Progress lost per second while no player is in the zone (0.01 = 1% per second).")]
        [SerializeField, Min(0f)] private float _decayPerSecond = 0.01f;

        [Header("Combat")]
        [Tooltip("Progress gained for each encounter enemy killed (0.01 = 1%).")]
        [SerializeField, Min(0f)] private float _progressPerKill = 0.01f;
        [Tooltip("Living enemies inside the zone at which charging slows down. 0 = never.")]
        [SerializeField, Min(0)] private int _contestedEnemyCount = 4;
        [Tooltip("Charge speed while the zone is contested (0.5 = half speed).")]
        [SerializeField, Range(0f, 1f)] private float _contestedChargeMultiplier = 0.5f;

        [Header("Spawning")]
        [Tooltip("Spawn settings used by the teleporter's own spawner during the event.")]
        [SerializeField] private EnemySpawnSettings _encounterSpawnSettings;

        public float ChargeDuration => _chargeDuration;
        public float ZoneRadius => _zoneRadius;
        public float DecayPerSecond => _decayPerSecond;
        public float ProgressPerKill => _progressPerKill;
        public int ContestedEnemyCount => _contestedEnemyCount;
        public float ContestedChargeMultiplier => _contestedChargeMultiplier;
        public EnemySpawnSettings EncounterSpawnSettings => _encounterSpawnSettings;
    }
}
