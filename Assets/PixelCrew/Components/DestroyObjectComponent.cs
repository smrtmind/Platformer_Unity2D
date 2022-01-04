using System.Collections;
using System.Collections.Generic;
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
        }

        public void CollectCoin()
        {
            string coinName = default;
            int coinCost = default;

            if (_objectToDestroy.name.Contains("SilverCoin"))
            {
                coinCost = 1;
                coinName = "silver";
            }

            else if (_objectToDestroy.name.Contains("GoldCoin"))
            {
                coinCost = 10;
                coinName = "gold";
            }

            _hero._score += coinCost;
            Debug.Log($"You have collected {coinName} coin (+{coinCost}) / TOTAL SCORE: {_hero._score}");
        }
    }
}
