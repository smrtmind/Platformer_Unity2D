using PixelCrew.Creatures;
using UnityEngine;

namespace PixelCrew.Components
{
    public class AddSwordComponent : MonoBehaviour
    {
        private Hero _hero;

        private void Start()
        {
            _hero = FindObjectOfType<Hero>();
        }

        public void Add()
        {
            _hero.AddSwords();
        }
    }
}