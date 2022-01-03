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
            string _coin = default;
            int points = default;

            if (_objectToDestroy.name.Contains("Coins_0"))
            {
                points = 1;
                _coin = "silver";
            }

            else if (_objectToDestroy.name.Contains("Coins_4"))
            {
                points = 10;
                _coin = "gold";
            }

            _hero._score += points;
            Debug.Log($"You have collected {_coin} coin (+{points})\n" +
                      $"SCORE: {_hero._score}");
        }
    }
}
