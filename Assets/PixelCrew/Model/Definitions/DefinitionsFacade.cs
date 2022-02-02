using UnityEngine;

namespace PixelCrew.Model.Definitions
{
    [CreateAssetMenu(menuName = "Definitions/DefinitionsFacade", fileName = "DefinitionsFacade")]
    public class DefinitionsFacade : ScriptableObject
    {
        [SerializeField] private InventoryItemsDefinitions _items;

        public InventoryItemsDefinitions Items => _items;

        private static DefinitionsFacade _instance;
        public static DefinitionsFacade Instance => _instance == null ? LoadDefinitions() : _instance;

        private static DefinitionsFacade LoadDefinitions()
        {
            return _instance = Resources.Load<DefinitionsFacade>("DefinitionsFacade");
        }
    }
}
