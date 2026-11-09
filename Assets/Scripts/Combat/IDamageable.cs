namespace Vyrden.Combat
{
    /// <summary>
    /// Anything that can be hit. All damage goes through this single entry point.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        /// <summary>Applies damage and returns how much health was actually lost.</summary>
        float ApplyDamage(in DamageInfo damage);
    }
}
