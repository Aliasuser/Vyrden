using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// Something a character can use with the interact button (teleporter, crates and shrines later).
    /// Found through a collider on the object or one of its children.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Where the interactor measures distance to.</summary>
        Vector3 InteractionPoint { get; }

        /// <summary>Whether <paramref name="interactor"/> can use it right now.</summary>
        bool CanInteract(GameObject interactor);

        /// <summary>Short text for a UI prompt, e.g. "Activate teleporter". Empty when nothing to show.</summary>
        string GetPrompt(GameObject interactor);

        /// <summary>Uses it. Callers check <see cref="CanInteract"/> first; implementations check again.</summary>
        void Interact(GameObject interactor);
    }
}
