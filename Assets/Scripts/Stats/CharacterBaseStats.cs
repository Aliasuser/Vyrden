using UnityEngine;

namespace Vyrden.Stats
{
    /// <summary>Starting stat values for one character type, before any modifiers.</summary>
    [CreateAssetMenu(fileName = "CharacterBaseStats", menuName = "Vyrden/Stats/Character Base Stats")]
    public class CharacterBaseStats : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [Tooltip("1 = the speeds in the character's motor settings.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 1f;
        [Tooltip("1 = the weapon's own damage.")]
        [SerializeField, Min(0f)] private float _damage = 1f;
        [Tooltip("1 = the weapon's own fire rate.")]
        [SerializeField, Min(0.01f)] private float _attackSpeed = 1f;
        [SerializeField] private float _armor;

        public float Get(StatType stat)
        {
            switch (stat)
            {
                case StatType.MaxHealth:
                    return _maxHealth;
                case StatType.MoveSpeed:
                    return _moveSpeed;
                case StatType.Damage:
                    return _damage;
                case StatType.AttackSpeed:
                    return _attackSpeed;
                case StatType.Armor:
                    return _armor;
                default:
                    return 0f;
            }
        }
    }
}
