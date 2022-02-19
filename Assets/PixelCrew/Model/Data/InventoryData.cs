using PixelCrew.Model.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PixelCrew.Model.Data
{
    [Serializable]
    public class InventoryData
    {
        [SerializeField] private List<InventoryItemData> _inventory = new List<InventoryItemData>();

        public delegate void OnInventoryChanged(string id, int value);

        public OnInventoryChanged OnChanged;

        public void Add(string id, int value)
        {
            if (value <= 0) return;

            var itemDef = DefinitionsFacade.Instance.Items.Get(id);
            if (itemDef.IsVoid) return;

            var inventoryIsFull = _inventory.Count >= DefinitionsFacade.Instance.Player.InventorySize;

            if (itemDef.HasTag(ItemTag.Stackable))
            {
                var item = GetItem(id);
                if (item == null)
                {
                    if (inventoryIsFull)
                    {
                        Debug.Log("Inventory is full");
                        return;
                    }

                    item = new InventoryItemData(id);
                    _inventory.Add(item);
                }

                item.Value += value;
            }

            else
            {
                for (int i = 0; i < value; i++)
                {
                    if (inventoryIsFull)
                    {
                        Debug.Log("Inventory is full");
                        return;
                    }

                    var item = new InventoryItemData(id) {Value = 1};
                    _inventory.Add(item);
                }
            }

            OnChanged?.Invoke(id, Count(id));
        }

        public InventoryItemData[] GetAll(params ItemTag[] tags)
        {
            var returnValue = new List<InventoryItemData>();
            foreach (var item in _inventory)
            {
                var itemDefinition = DefinitionsFacade.Instance.Items.Get(item.Id);
                var isAllRequirementsMet = tags.All(x => itemDefinition.HasTag(x));
                if (isAllRequirementsMet)
                {
                    returnValue.Add(item);
                }
            }

            return returnValue.ToArray();
        }

        public void Remove(string id, int value)
        {
            var itemDef = DefinitionsFacade.Instance.Items.Get(id);
            if (itemDef.IsVoid) return;

            if (itemDef.HasTag(ItemTag.Stackable))
            {
                var item = GetItem(id);
                if (item == null) return;

                item.Value -= value;

                if (item.Value <= 0)
                {
                    _inventory.Remove(item);
                }
            }

            else
            {
                for (int i = 0; i < value; i++)
                {
                    var item = GetItem(id);
                    if (item == null) return;

                    _inventory.Remove(item);
                }
            }

            OnChanged?.Invoke(id, Count(id));
        }

        private InventoryItemData GetItem(string id)
        {
            foreach (var itemData in _inventory)
            {
                if (itemData.Id == id)
                {
                    return itemData;
                }
            }

            return null;
        }

        public int Count(string id)
        {
            var count = 0;
            foreach (var item in _inventory)
            {
                if (item.Id == id)
                {
                    count += item.Value;
                }
            }

            return count;
        }
    }

    [Serializable]
    public class InventoryItemData
    {
        [InventoryId] public string Id;
        public int Value;

        public InventoryItemData(string id)
        {
            Id = id;
        }
    }
}
