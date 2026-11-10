namespace Vyrden.Progression
{
    public enum StageState
    {
        /// <summary>The stage objective is not finished yet.</summary>
        InProgress = 0,

        /// <summary>The objective is done; the run can move to the next stage.</summary>
        Completed = 1
    }
}
