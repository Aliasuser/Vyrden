using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Reduces incoming damage by armor. Positive armor: damage × 100 / (100 + armor)
    /// (10 armor ≈ 9% less, 100 armor = 50% less, never immune). Negative armor increases damage:
    /// damage × (2 − 100 / (100 − armor)). Picked up automatically by <see cref="Health"/>.
    /// </summary>
    public class ArmorDamageModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private float _armor;

        // Runs after most modifiers so flat shields and the like absorb first.
        public int Order => 100;
        public float Armor => _armor;

        public void SetArmor(float armor)
        {
            _armor = armor;
        }

        public void ModifyIncomingDamage(ref DamageInfo damage)
        {
            damage.Amount *= GetDamageMultiplier(_armor);
        }

        public static float GetDamageMultiplier(float armor)
        {
            return armor >= 0f ? 100f / (100f + armor) : 2f - 100f / (100f - armor);
        }
    }
}
