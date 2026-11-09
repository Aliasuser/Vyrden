using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>Shows a placeholder telegraph while a melee attack winds up. Presentation only.</summary>
    public class EnemyAttackFeedback : MonoBehaviour
    {
        [SerializeField] private MeleeAttack _attack;
        [SerializeField] private GameObject _windupIndicator;

        private void Awake()
        {
            if (_attack == null)
            {
                _attack = GetComponentInChildren<MeleeAttack>();
            }

            if (_windupIndicator != null)
            {
                _windupIndicator.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_attack != null)
            {
                _attack.AttackStarted += OnAttackStarted;
                _attack.AttackFinished += OnAttackFinished;
            }
        }

        private void OnDisable()
        {
            if (_attack != null)
            {
                _attack.AttackStarted -= OnAttackStarted;
                _attack.AttackFinished -= OnAttackFinished;
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
