namespace Vyrden.Items
{
    /// <summary>Pure stacking math, kept apart so it can be unit tested and reused.</summary>
    public static class StackingCalculator
    {
        public static float Evaluate(StackingRule rule, float valuePerStack, int stacks)
        {
            if (stacks <= 0)
            {
                return 0f;
            }

            switch (rule)
            {
                case StackingRule.Hyperbolic:
                    return 1f - 1f / (1f + valuePerStack * stacks);
                case StackingRule.NonStacking:
                    return valuePerStack;
                default:
                    return valuePerStack * stacks;
            }
        }
    }
}
