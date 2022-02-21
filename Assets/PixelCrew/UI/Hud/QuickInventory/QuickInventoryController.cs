using System.Collections.Generic;
using PixelCrew.Model;
using PixelCrew.Utils.Disposables;
using UnityEngine;

namespace PixelCrew.UI.Hud.QuickInventory
{
    public class QuickInventoryController : MonoBehaviour
    {
        [SerializeField] private Transform _itemCell;
        [SerializeField] private InventoryItemWidget _prefab;

        private readonly CompositeDisposable _trash = new CompositeDisposable();

        private GameSession _session;
        private List<InventoryItemWidget> _createdItem = new List<InventoryItemWidget>();

        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            _trash.Retain(_session.QuickInventory.Subscribe(Rebuild));
            Rebuild();
        }

        private void Rebuild()
        {
            var inventory = _session.QuickInventory.Inventory;

            //creating required items
            for (var i = _createdItem.Count; i < inventory.Length; i++)
            {
                var item = Instantiate(_prefab, _itemCell);
                _createdItem.Add(item);
            }

            //updating data and activate
            for (var i = 0; i < inventory.Length; i++)
            {
                _createdItem[i].SetData(inventory[i], i);
                _createdItem[i].gameObject.SetActive(true);
            }

            //hiding unused items
            for (var i = inventory.Length; i < _createdItem.Count; i++)
            {
                _createdItem[i].gameObject.SetActive(false);
            }
        }
    }
}