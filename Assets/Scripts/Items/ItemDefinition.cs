using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Items
{
    /// <summary>Data for one item type. Runtime state (how many are held) lives in <see cref="Inventory"/>.</summary>
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Vyrden/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Tooltip("Stable unique ID (save data, networking). Never change it once used.")]
        [SerializeField] private string _id = "";
        [SerializeField] private string _displayName = "Item";
        [SerializeField, TextArea(2, 5)] private string _description = "";
        [SerializeField] private Sprite _icon;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        [SerializeField] private ItemKind _kind = ItemKind.Passive;
        [SerializeField] private List<ItemEffect> _effects = new List<ItemEffect>();

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public ItemRarity Rarity => _rarity;
        public ItemKind Kind => _kind;
        public IReadOnlyList<ItemEffect> Effects => _effects;
    }
}
