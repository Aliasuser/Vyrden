using UnityEngine;

namespace Vyrden.Items
{
    /// <summary>
    /// Base for what an item does. Items only reference effects; the inventory never runs them.
    /// New kinds of effect (on-hit, on-kill, active use) add new virtual hooks here.
    /// </summary>
    public abstract class ItemEffect : ScriptableObject
    {
        /// <summary>Adds this effect's stat modifiers for the given number of copies.</summary>
        public virtual void CollectStatModifiers(int stacks, StatModifierBuffer buffer)
        {
        }
    }
}
