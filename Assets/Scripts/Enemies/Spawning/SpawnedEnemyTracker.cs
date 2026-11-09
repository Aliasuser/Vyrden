using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Added by <see cref="EnemySpawner"/> to every enemy it creates. Tells the spawner exactly once when
    /// the enemy dies or is destroyed, whichever comes first.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class SpawnedEnemyTracker : MonoBehaviour
    {
        private EnemySpawner _owner;
        private Health _health;
        private bool _released;

        public EnemySpawner Owner => _owner;

        internal void Bind(EnemySpawner owner)
        {
            _owner = owner;
            _health = GetComponent<Health>();
            if (_health != null)
            {
                _health.Died += OnDied;
            }
        }

        private void OnDied(DamageInfo killingBlow)
        {
            Release();
        }

        private void OnDestroy()
        {
            Release();
        }

        private void Release()
        {
            if (_released)
            {
                return;
            }

            _released = true;
            if (_health != null)
            {
                _health.Died -= OnDied;
            }

            if (_owner != null)
            {
                _owner.NotifyRemoved(this);
            }
        }
    }
}
