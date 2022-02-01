﻿using PixelCrew.Components.ColliderBased;
using System.Collections;
using UnityEngine;

namespace PixelCrew.Creatures.Mobs.Patrolling
{
    public class PlatformPatrol : Patrol
    {
        [SerializeField] private LayerCheck _groundCheck;
        [SerializeField] private LayerCheck _obstacleCheck;
        [SerializeField] private int _direction;
        [SerializeField] private Creature _creature;

        public override IEnumerator DoPatrol()
        {
            while (enabled)
            {
                if (_groundCheck.IsTouchingLayer && !_obstacleCheck.IsTouchingLayer)
                {
                    _creature.SetDirection(new Vector2(_direction, 0f));
                }

                else 
                {
                    _direction = -_direction;
                    _creature.SetDirection(new Vector2(_direction, 0f));
                }

                yield return null;
            }
        }
    }
}
