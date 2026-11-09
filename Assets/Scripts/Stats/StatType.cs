namespace Vyrden.Stats
{
    /// <summary>Character stats that modifiers can change. Values are serialized as ints: add new ones at the end.</summary>
    public enum StatType
    {
        /// <summary>Maximum health in hit points.</summary>
        MaxHealth = 0,

        /// <summary>Multiplier on movement speed (1 = settings speed).</summary>
        MoveSpeed = 1,

        /// <summary>Multiplier on outgoing damage (1 = weapon damage).</summary>
        Damage = 2,

        /// <summary>Multiplier on weapon fire rate (1 = weapon fire rate).</summary>
        AttackSpeed = 3,

        /// <summary>Armor points; reduces incoming damage (see ArmorDamageModifier).</summary>
        Armor = 4,
    }
}
