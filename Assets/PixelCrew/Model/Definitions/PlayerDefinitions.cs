using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrew.Model.Definitions
{
    [CreateAssetMenu(menuName = "Definitions/PlayerDefinitions", fileName = "PlayerDefinitions")]
    public class PlayerDefinitions : ScriptableObject
    {
        [SerializeField] private int _inventorySize;
        public int InventorySize => _inventorySize;
    }
}
