using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>Shows a placeholder telegraph while an attack winds up. Presentation only.</summary>
    public class EnemyAttackFeedback : MonoBehaviour
    {
        [Tooltip("3D prototype melee attack. Leave empty to use the first attack (IAttack) in children.")]
        [SerializeField] private MeleeAttack _attack;
        [SerializeField] private GameObject _windupIndicator;

        private IAttack _source;

        private void Awake()
        {
            _source = _attack != null ? _attack : GetComponentInChildren<IAttack>();

            if (_windupIndicator != null)
            {
                _windupIndicator.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_source != null)
            {
                _source.AttackStarted += OnAttackStarted;
                _source.AttackFinished += OnAttackFinished;
            }
        }

        private void OnDisable()
        {
            if (_source != null)
            {
                _source.AttackStarted -= OnAttackStarted;
                _source.AttackFinished -= OnAttackFinished;
            }
        }

        private void OnAttackStarted()
        {
            if (_windupIndicator != null)
            {
                _windupIndicator.SetActive(true);
            }
        }

        private void OnAttackFinished(bool hit)
        {
            if (_windupIndicator != null)
            {
                _windupIndicator.SetActive(false);
            }
        }
    }
}
