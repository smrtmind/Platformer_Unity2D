using PixelCrew.Model.Definitions.Repository.Item;
using System;
using UnityEngine;

namespace PixelCrew.Model.Definitions.Repository
{
    [CreateAssetMenu(menuName = "Definitions/ThrowableItems", fileName = "ThrowableItems")]
    public class ThrowableItemsDefinitions : DefinitionRepository<ThrowableDef>
    {
        
    }

    [Serializable]
    public struct ThrowableDef : IHaveId
    {
        [InventoryId] [SerializeField] private string _id;
        [SerializeField] private GameObject _projectile;

        public string Id => _id;
        public GameObject Projectile => _projectile;
    }
}
