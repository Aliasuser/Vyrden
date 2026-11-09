using UnityEngine;

namespace Vyrden.Player
{
    /// <summary>
    /// What a character wants to do this frame, independent of where the request came from
    /// (player input, AI, or later a network message).
    /// </summary>
    public struct MovementIntent
    {
        /// <summary>World-space horizontal direction. Magnitude 0..1 (analog input keeps partial speed).</summary>
        public Vector3 MoveDirection;

        /// <summary>True while the character wants to sprint.</summary>
        public bool Sprint;

        /// <summary>True on the frame a jump was requested.</summary>
        public bool JumpRequested;
    }
}
