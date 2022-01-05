using UnityEngine;

namespace PixelCrew.Components
{
    public class DestroyObjectComponent : MonoBehaviour
    {
        [SerializeField] private Hero _hero;
        [SerializeField] private GameObject _objectToDestroy;

        public void DestroyObject()
        {
            Destroy(_objectToDestroy);
            ItemCheck(_objectToDestroy.name);
        }

        private void ItemCheck(string name)
        {
            if (name.Contains("SilverCoin"))
            {
                _hero.AddCoins(coins: 1);
            }

            else if (name.Contains("GoldCoin"))
            {
                _hero.AddCoins(coins: 10);
            }
        }
    }
}
