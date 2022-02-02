using System;
using UnityEngine;

namespace PixelCrew.Model.Data
{
    [Serializable]
    public class PlayerData
    {
        [SerializeField] private InventoryData _inventory;

        public int Hp;

        [Header("Skills")]
        public bool DoubleJumpIsActive;
        public bool DashIsActive;
        public bool WallClimbIsActive;
        public bool SwordIsActive;

        public InventoryData Inventory => _inventory;

        public PlayerData Clone()
        {
            var json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<PlayerData>(json);
        }
    }
}
