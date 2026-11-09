using System;
using UnityEngine;

namespace Vyrden.Items
{
    /// <summary>
    /// World pickup. Gives its item to the first inventory that touches its trigger, exactly once,
    /// then disables or destroys itself.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField, Min(1)] private int _count = 1;
        [Tooltip("Destroy the pickup after collection. Off = just deactivate it (for pooling).")]
        [SerializeField] private bool _destroyOnCollect = true;

        private bool _collected;

        /// <summary>Raised once when collected. Args: pickup, collecting inventory.</summary>
        public event Action<ItemPickup, Inventory> Collected;

        public ItemDefinition Item => _item;
        public int Count => _count;
        public bool IsCollected => _collected;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Inventory inventory = other.GetComponentInParent<Inventory>();
            if (inventory != null)
            {
                TryCollect(inventory);
            }
        }

        /// <summary>Gives the item to <paramref name="inventory"/>. Returns false if already collected.</summary>
        public bool TryCollect(Inventory inventory)
        {
            if (_collected || _item == null || inventory == null)
            {
                return false;
            }

            _collected = true;
            GetComponent<Collider>().enabled = false;
            inventory.AddItem(_item, _count);
            Collected?.Invoke(this, inventory);

            if (_destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        /// <summary>Configures a pickup at runtime (e.g. dropped by an enemy or a chest).</summary>
        public void Setup(ItemDefinition item, int count = 1)
        {
            _item = item;
            _count = Mathf.Max(1, count);
            _collected = false;
            GetComponent<Collider>().enabled = true;
        }
    }
}
