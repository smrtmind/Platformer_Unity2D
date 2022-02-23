using PixelCrew.Model.Definitions.Repository;
using PixelCrew.Model.Definitions.Repository.Item;
using UnityEngine;

namespace PixelCrew.Model.Definitions
{
    [CreateAssetMenu(menuName = "Definitions/DefinitionsFacade", fileName = "DefinitionsFacade")]
    public class DefinitionsFacade : ScriptableObject
    {
        [SerializeField] private ItemsRepository _items;
        [SerializeField] private ThrowableItemsDefinitions _throwableItems;
        [SerializeField] private PotionRepository _potions;
        [SerializeField] private PlayerDefinitions _player;

        public ItemsRepository Items => _items;
        public ThrowableItemsDefinitions ThrowableItems => _throwableItems;
        public PotionRepository Potion => _potions;
        public PlayerDefinitions Player => _player;

        private static DefinitionsFacade _instance;
        public static DefinitionsFacade Instance => _instance == null ? LoadDefinitions() : _instance;

        private static DefinitionsFacade LoadDefinitions()
        {
            return _instance = Resources.Load<DefinitionsFacade>("DefinitionsFacade");
        }
    }
}
