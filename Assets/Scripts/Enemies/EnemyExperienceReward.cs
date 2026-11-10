using UnityEngine;
using Vyrden.Combat;
using Vyrden.Progression;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Gives the enemy's experience reward to whoever landed the killing blow, if that character can
    /// gain experience. Kill credit comes from <see cref="DamageInfo.Instigator"/>.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyExperienceReward : MonoBehaviour
    {
        private Enemy _enemy;

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
            if (_enemy.Definition == null || _enemy.Definition.ExperienceReward <= 0f || killingBlow.Instigator == null)
            {
                return;
            }

            CharacterExperience killer = killingBlow.Instigator.GetComponentInParent<CharacterExperience>();
            if (killer != null)
            {
                killer.AddExperience(_enemy.Definition.ExperienceReward, gameObject);
            }
        }
    }
}
