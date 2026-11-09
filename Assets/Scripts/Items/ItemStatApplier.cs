using System.Collections.Generic;
using UnityEngine;
using Vyrden.Stats;

namespace Vyrden.Items
{
    /// <summary>
    /// Bridges inventory and stats: whenever the inventory changes, rebuilds the full set of item
    /// modifiers and hands them to <see cref="CharacterStats"/> as one source. Rebuilding from scratch
    /// means stacks, removals and repeated updates can never leave stale or doubled modifiers.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class ItemStatApplier : MonoBehaviour
    {
        [Tooltip("Defaults to the CharacterStats on this object.")]
        [SerializeField] private CharacterStats _stats;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private Inventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
            if (_stats == null)
            {
                _stats = GetComponent<CharacterStats>();
            }

            if (_stats == null)
            {
                Debug.LogError($"{nameof(ItemStatApplier)} on '{name}' needs a {nameof(CharacterStats)}.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _inventory.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            _inventory.Changed -= Apply;
            if (_stats != null)
            {
                _stats.RemoveModifiersFromSource(this);
            }
        }

        public void Apply()
        {
            _modifiers.Clear();
            IReadOnlyList<ItemStack> items = _inventory.Items;
            for (int i = 0; i < items.Count; i++)
            {
                ItemStack stack = items[i];
                IReadOnlyList<ItemEffect> effects = stack.Item.Effects;
                for (int e = 0; e < effects.Count; e++)
                {
                    if (effects[e] != null)
                    {
                        effects[e].CollectStatModifiers(stack.Count, _modifiers, this);
                    }
                }
            }

            _stats.ReplaceModifiers(this, _modifiers);
        }
    }
}
