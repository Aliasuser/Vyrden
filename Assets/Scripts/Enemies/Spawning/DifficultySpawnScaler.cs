using UnityEngine;
using Vyrden.Progression;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Applies the <see cref="DifficultyDirector"/>'s current values to a spawner: spawn interval and
    /// max active every frame, health and damage multipliers to each enemy as it spawns. Enemies keep the
    /// stats they spawned with, so difficulty never changes an enemy mid-fight. No difficulty math here.
    /// </summary>
    [RequireComponent(typeof(EnemySpawner))]
    public class DifficultySpawnScaler : MonoBehaviour
    {
        [SerializeField] private DifficultyDirector _director;

        private EnemySpawner _spawner;

        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
            if (_director == null)
            {
                Debug.LogError($"{nameof(DifficultySpawnScaler)} on '{name}' has no {nameof(DifficultyDirector)} assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _spawner.EnemySpawned += OnEnemySpawned;
            ApplySpawnModifiers();
        }

        private void OnDisable()
        {
            _spawner.EnemySpawned -= OnEnemySpawned;
            _spawner.SetSpawnModifiers(1f, 0);
        }

        private void Update()
        {
            ApplySpawnModifiers();
        }

        private void ApplySpawnModifiers()
        {
            DifficultyState state = _director.Current;
            _spawner.SetSpawnModifiers(state.SpawnIntervalMultiplier, state.MaxActiveBonus);
        }

        private void OnEnemySpawned(GameObject spawned)
        {
            if (spawned.TryGetComponent(out Enemy enemy))
            {
                DifficultyState state = _director.Current;
                enemy.ApplyStatMultipliers(state.EnemyHealthMultiplier, state.EnemyDamageMultiplier);
            }
        }
    }
}
