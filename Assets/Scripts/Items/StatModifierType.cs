namespace Vyrden.Items
{
    /// <summary>
    /// How a modifier combines: final = (base + sum of Flat) x (1 + sum of Percent).
    /// Percent values are fractions (0.1 = +10%).
    /// </summary>
    public enum StatModifierType
    {
        Flat = 0,
        Percent = 1,
    }
}
