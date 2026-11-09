namespace Vyrden.Combat
{
    /// <summary>
    /// Category of incoming damage. Resistances and armor can react to it later.
    /// Add new values at the end so serialized data keeps its meaning.
    /// </summary>
    public enum DamageType
    {
        Generic = 0,
    }
}
