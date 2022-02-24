using PixelCrew.Model.Definitions.Repository.Item;
using System;
using UnityEngine;

namespace PixelCrew.Model.Definitions.Repository
{
    [CreateAssetMenu(menuName = "Definitions/Repositories/Potions", fileName = "Potions")]
    public class PotionRepository : DefinitionRepository<PotionDefinition>
    {
        
    }

    [Serializable]
    public struct PotionDefinition : IHaveId
    {
        [InventoryId] [SerializeField] private string _id;
        [SerializeField] private Effect _effect;
        [SerializeField] private float _value;
        [SerializeField] private float _time;

        public string Id => _id;
        public Effect Effect => _effect;
        public float Value => _value;
        public float Time => _time;
    }

    public enum Effect
    {
        Heal,
        SpeedUp,
        Mana
    }
}
