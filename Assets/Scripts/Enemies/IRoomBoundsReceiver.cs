using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Something that should stay within one room (an enemy's AI). Spawners and rooms hand over the room's
    /// bounds through this, without knowing what the receiver does with them.
    /// </summary>
    public interface IRoomBoundsReceiver
    {
        void SetRoomBounds(Rect bounds);
    }
}
