using System;
using UnityEngine;

namespace Player
{
    public interface IPlayerView
    {
        IPlayerMovement Movement { get; set; }
        Transform TransformPlayer { get; set; }
        public event Action OnCollision;
        void SetNormalMovement();
        void SetRotateMovement();
    }
}