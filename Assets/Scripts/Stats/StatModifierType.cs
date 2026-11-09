namespace Vyrden.Stats
{
    /// <summary>
    /// How a modifier combines. Final value =
    /// (base + sum of Flat) x (1 + sum of AdditivePercent) x product of (1 + each MultiplicativePercent).
    /// Percent values are fractions (0.1 = +10%, -0.2 = -20%).
    /// </summary>
    public enum StatModifierType
    {
        /// <summary>Added to the base value.</summary>
        Flat = 0,

        /// <summary>Summed with other additive percents, then applied once.</summary>
        AdditivePercent = 1,

        /// <summary>Applied on its own, compounding with other multiplicative percents.</summary>
        MultiplicativePercent = 2,
    }
}
