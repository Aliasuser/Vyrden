using System;
using UnityEngine;
using Vyrden.Combat;
using Vyrden.Core;

namespace Vyrden.Player
{
    /// <summary>
    /// Finds the closest usable <see cref="IInteractable"/> in range and uses it when asked. Knows nothing
    /// about input: <see cref="PlayerInputReader"/> calls <see cref="TryInteract"/>.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        private const int MaxColliders = 32;

        [SerializeField, Min(0.1f)] private float _range = 3f;
        [Tooltip("Seconds between searches for interactables.")]
        [SerializeField, Min(0f)] private float _scanInterval = 0.1f;
        [SerializeField] private LayerMask _layers = ~0;
        [Tooltip("Distance is measured from here. Defaults to this transform.")]
        [SerializeField] private Transform _origin;

        private readonly Collider[] _results = new Collider[MaxColliders];
        private Health _health;
        private IInteractable _target;
        private float _nextScanTime;

        /// <summary>Raised when the closest usable interactable changes (null = none). For UI prompts.</summary>
        public event Action<IInteractable> TargetChanged;

        /// <summary>Raised after an interaction. Arg: what was used.</summary>
        public event Action<IInteractable> Interacted;

        public IInteractable CurrentTarget => _target;
        public float Range => _range;

        private Vector3 Origin => _origin != null ? _origin.position : transform.position;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void Update()
        {
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + _scanInterval;
                RefreshTarget();
            }
        }

        /// <summary>Uses the current target if it is still usable. Returns whether something was used.</summary>
        public bool TryInteract()
        {
            RefreshTarget();
            if (_target == null)
            {
                return false;
            }

            IInteractable used = _target;
            used.Interact(gameObject);
            Interacted?.Invoke(used);
            RefreshTarget();
            return true;
        }

        /// <summary>Searches for the closest usable interactable now.</summary>
        public void RefreshTarget()
        {
            IInteractable best = null;
            if (_health == null || _health.IsAlive)
            {
                best = FindClosest();
            }

            if (!ReferenceEquals(best, _target))
            {
                _target = best;
                TargetChanged?.Invoke(_target);
            }
        }

        private IInteractable FindClosest()
        {
            Vector3 origin = Origin;
            int count = Physics.OverlapSphereNonAlloc(origin, _range, _results, _layers, QueryTriggerInteraction.Collide);
            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = _results[i].GetComponentInParent<IInteractable>();
                if (candidate == null || ReferenceEquals(candidate, best) || !candidate.CanInteract(gameObject))
                {
                    continue;
                }

                float distance = (candidate.InteractionPoint - origin).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Origin, _range);
        }
    }
}
