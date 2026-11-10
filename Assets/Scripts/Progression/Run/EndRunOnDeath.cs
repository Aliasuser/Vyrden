using UnityEngine;
using Vyrden.Combat;
using Vyrden.Core;

namespace Vyrden.Progression
{
    /// <summary>Ends the run when the watched character dies (the player in single-player).</summary>
    public class EndRunOnDeath : MonoBehaviour
    {
        [SerializeField] private RunTimer _runTimer;
        [SerializeField] private Health _health;

        private void Awake()
        {
            if (_runTimer == null || _health == null)
            {
                Debug.LogError($"{nameof(EndRunOnDeath)} on '{name}' needs a {nameof(RunTimer)} and a {nameof(Health)}.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= OnDied;
            }
        }

        private void OnDied(DamageInfo killingBlow)
        {
            _runTimer.EndRun();
        }
    }
}
