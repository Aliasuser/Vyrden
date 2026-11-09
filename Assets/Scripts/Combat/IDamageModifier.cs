namespace Vyrden.Combat
{
    /// <summary>
    /// Hook for anything that changes incoming damage before health is reduced
    /// (shields, armor, resistances, items). Components on the same GameObject are picked up
    /// automatically by <see cref="Health"/>; others can register at runtime.
    /// </summary>
    public interface IDamageModifier
    {
        /// <summary>Lower values run first.</summary>
        int Order { get; }

        void ModifyIncomingDamage(ref DamageInfo damage);
    }
}
