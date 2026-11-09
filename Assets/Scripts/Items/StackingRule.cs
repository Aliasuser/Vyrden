namespace Vyrden.Items
{
    /// <summary>How an effect's value grows with extra copies of the item.</summary>
    public enum StackingRule
    {
        /// <summary>value x stacks.</summary>
        Linear = 0,

        /// <summary>1 - 1 / (1 + value x stacks). Approaches 1; for chances and reductions.</summary>
        Hyperbolic = 1,

        /// <summary>value once; extra copies add nothing.</summary>
        NonStacking = 2,
    }
}
