using PixelCrew.Model.Data;
using PixelCrew.Model.Definitions;
using PixelCrew.Model.Definitions.Repository.Item;
using PixelCrew.Utils;
using UnityEngine;

namespace PixelCrew.Components.Collectables
{
    public class AddToInventoryComponent : MonoBehaviour
    {
        [InventoryId] [SerializeField] private string _id;
        [SerializeField] private int _count;

        public void Add(GameObject go)
        {
            var hero = go.GetInterface<ICanAddToInventory>();
            hero?.AddToInventory(_id, _count);
        }
    }
}
