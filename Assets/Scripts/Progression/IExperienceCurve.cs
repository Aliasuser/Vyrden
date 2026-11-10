namespace Vyrden.Progression
{
    /// <summary>How much experience each level costs. Kept as an interface so the level math can be tested without assets.</summary>
    public interface IExperienceCurve
    {
        /// <summary>Highest reachable level. 0 means no cap.</summary>
        int MaxLevel { get; }

        /// <summary>Experience needed to go from <paramref name="level"/> to the next one. Always at least 1.</summary>
        float GetRequirement(int level);
    }
}
