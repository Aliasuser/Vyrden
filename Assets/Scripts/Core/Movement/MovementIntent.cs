using UnityEngine;

namespace Vyrden.Movement
{
    /// <summary>
    /// What a character wants to do this frame, independent of where the request came from
    /// (player input, AI, or later a network message).
    /// </summary>
    public struct MovementIntent
    {
        /// <summary>
        /// World-space movement direction on the gameplay plane: XZ for the 3D <see cref="CharacterMotor"/>,
        /// XY for <see cref="CharacterMotor2D"/>. Magnitude 0..1 (analog input keeps partial speed).
        /// </summary>
        public Vector3 MoveDirection;

        /// <summary>True while the character wants to sprint.</summary>
        public bool Sprint;

        /// <summary>True on the frame a jump was requested.</summary>
        public bool JumpRequested;

        /// <summary>
        /// World-space direction the character should face (e.g. while aiming).
        /// Zero means face the movement direction.
        /// </summary>
        public Vector3 FaceDirection;
    }
}
