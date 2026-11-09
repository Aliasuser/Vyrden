namespace Vyrden.Stats
{
    /// <summary>One change to one stat, tagged with the object that owns it so it can be removed later.</summary>
    public readonly struct StatModifier
    {
        public readonly StatType Stat;
        public readonly StatModifierType Type;
        public readonly float Value;

        /// <summary>Who added it (an item applier, a buff, a difficulty system...). Used for removal.</summary>
        public readonly object Source;

        public StatModifier(StatType stat, StatModifierType type, float value, object source)
        {
            Stat = stat;
            Type = type;
            Value = value;
            Source = source;
        }
    }
}
