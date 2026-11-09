using UnityEngine;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Enemies
{
    /// <summary>
    /// What happens to the body when an enemy dies: stops movement, removes its collision,
    /// tips the placeholder body over and removes the enemy after a delay.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyDeathHandler : MonoBehaviour
    {
        [Tooltip("Visual root that falls over on death.")]
        [SerializeField] private Transform _body;
        [SerializeField, Min(0.01f)] private float _fallDuration = 0.35f;

        private Enemy _enemy;
        private float _fallTime = -1f;
        private Quaternion _startRotation;
        private Quaternion _endRotation;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _enemy.Health.Died += OnDied;
        }

        private void OnDisable()
        {
            _enemy.Health.Died -= OnDied;
        }

        private void OnDied(DamageInfo killingBlow)
        {
            if (TryGetComponent(out CharacterMotor motor))
            {
                motor.enabled = false;
            }

            if (TryGetComponent(out CharacterController controller))
            {
                controller.enabled = false; // Shots and the player pass through the body.
            }

            if (TryGetComponent(out TeamMember member))
            {
                member.enabled = false; // No longer a valid target for anyone.
            }

            if (_body != null)
            {
                _startRotation = _body.localRotation;
                _endRotation = Quaternion.Euler(-90f, 0f, 0f) * _startRotation;
                _fallTime = 0f;
            }

            float corpseDuration = _enemy.Definition != null ? _enemy.Definition.CorpseDuration : 3f;
            Destroy(gameObject, corpseDuration);
        }

        private void Update()
        {
            if (_fallTime < 0f || _body == null)
            {
                return;
            }

            _fallTime += Time.deltaTime;
            float t = Mathf.Clamp01(_fallTime / _fallDuration);
            _body.localRotation = Quaternion.Slerp(_startRotation, _endRotation, t * t);

            if (t >= 1f)
            {
                _fallTime = -1f;
            }
        }
    }
}
