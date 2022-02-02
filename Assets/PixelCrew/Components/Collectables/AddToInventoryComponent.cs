using PixelCrew.Creatures.Hero;
using PixelCrew.Model.Definitions;
using UnityEngine;

namespace PixelCrew.Components.Collectables
{
    public class AddToInventoryComponent : MonoBehaviour
    {
        [InventoryId] [SerializeField] private string _id;
        [SerializeField] private int _count;

        public void Add(GameObject go)
        {
            var hero = go.GetComponent<Hero>();
            if (hero != null)
            {
                hero.AddToInventory(_id, _count);
            }
        }
    }
}
