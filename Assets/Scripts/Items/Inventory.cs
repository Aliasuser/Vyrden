using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Items
{
    /// <summary>
    /// What items a character holds and how many of each. Only records collection; effects are applied by
    /// whoever listens (e.g. <see cref="ItemStatApplier"/>). <see cref="AddItem"/> and <see cref="RemoveItem"/>
    /// are the single entry points for changing it (host-only in multiplayer later).
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        private readonly List<ItemStack> _stacks = new List<ItemStack>();

        /// <summary>Raised after copies are added. Args: item, new total count.</summary>
        public event Action<ItemDefinition, int> ItemAdded;

        /// <summary>Raised after copies are removed. Args: item, new total count (0 = gone).</summary>
        public event Action<ItemDefinition, int> ItemRemoved;

        /// <summary>Raised after any change.</summary>
        public event Action Changed;

        /// <summary>Held items in the order they were first collected.</summary>
        public IReadOnlyList<ItemStack> Items => _stacks;

        public int TotalItemCount
        {
            get
            {
                int total = 0;
                for (int i = 0; i < _stacks.Count; i++)
                {
                    total += _stacks[i].Count;
                }

                return total;
            }
        }

        public int GetCount(ItemDefinition item)
        {
            int index = IndexOf(item);
            return index >= 0 ? _stacks[index].Count : 0;
        }

        public bool Has(ItemDefinition item)
        {
            return GetCount(item) > 0;
        }

        /// <summary>Adds copies and returns the new count.</summary>
        public int AddItem(ItemDefinition item, int count = 1)
        {
            if (item == null)
            {
                return 0;
            }

            if (count <= 0)
            {
                return GetCount(item);
            }

            int index = IndexOf(item);
            int newCount = (index >= 0 ? _stacks[index].Count : 0) + count;
            if (index >= 0)
            {
                _stacks[index] = new ItemStack(item, newCount);
            }
            else
            {
                _stacks.Add(new ItemStack(item, newCount));
            }

            ItemAdded?.Invoke(item, newCount);
            Changed?.Invoke();
            return newCount;
        }

        /// <summary>Removes up to <paramref name="count"/> copies and returns how many were removed.</summary>
        public int RemoveItem(ItemDefinition item, int count = 1)
        {
            int index = IndexOf(item);
            if (index < 0 || count <= 0)
            {
                return 0;
            }

            int current = _stacks[index].Count;
            int removed = Mathf.Min(count, current);
            int newCount = current - removed;
            if (newCount > 0)
            {
                _stacks[index] = new ItemStack(item, newCount);
            }
            else
            {
                _stacks.RemoveAt(index);
            }

            ItemRemoved?.Invoke(item, newCount);
            Changed?.Invoke();
            return removed;
        }

        private int IndexOf(ItemDefinition item)
        {
            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].Item == item)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
