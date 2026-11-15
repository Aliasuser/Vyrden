using UnityEngine;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Enemies
{
    /// <summary>
    /// What happens to a 2D enemy's body when it dies: movement stops, the body leaves the physics
    /// simulation (shots and characters pass through, no more hits), it stops being a target, the placeholder
    /// visuals flatten and darken, and the enemy is removed after the definition's corpse duration.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyDeathHandler2D : MonoBehaviour
    {
        [Tooltip("Visual root that flattens on death (placeholder until death animations exist).")]
        [SerializeField] private Transform _visuals;
        [SerializeField, Min(0.01f)] private float _collapseDuration = 0.25f;
        [SerializeField] private Vector2 _collapsedScale = new Vector2(1.25f, 0.35f);
        [SerializeField] private Color _corpseTint = new Color(0.45f, 0.45f, 0.45f, 1f);

        private Enemy _enemy;
        private float _collapseTime = -1f;
        private Vector3 _startScale;

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
            if (TryGetComponent(out CharacterMotor2D motor))
            {
                motor.enabled = false;
            }

            if (TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            if (TryGetComponent(out TeamMember member))
            {
                member.enabled = false;
            }

            if (_visuals != null)
            {
                _startScale = _visuals.localScale;
                _collapseTime = 0f;
                foreach (SpriteRenderer sprite in _visuals.GetComponentsInChildren<SpriteRenderer>())
                {
                    sprite.color *= _corpseTint;
                }
            }

            float corpseDuration = _enemy.Definition != null ? _enemy.Definition.CorpseDuration : 3f;
            Destroy(gameObject, corpseDuration);
        }

        private void Update()
        {
            if (_collapseTime < 0f || _visuals == null)
            {
                return;
            }

            _collapseTime += Time.deltaTime;
            float t = Mathf.Clamp01(_collapseTime / _collapseDuration);
            Vector3 target = new Vector3(_startScale.x * _collapsedScale.x, _startScale.y * _collapsedScale.y, _startScale.z);
            _visuals.localScale = Vector3.Lerp(_startScale, target, t * t);

            if (t >= 1f)
            {
                _collapseTime = -1f;
            }
        }
    }
}
