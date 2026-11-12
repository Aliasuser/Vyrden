namespace Vyrden.Movement
{
    /// <summary>A motor whose speed can be scaled from outside (stats, slows, buffs). 1 = normal speed.</summary>
    public interface IMovementSpeedReceiver
    {
        void SetSpeedMultiplier(float multiplier);
    }
}
