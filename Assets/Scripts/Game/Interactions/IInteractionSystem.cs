using System;

namespace Game.Interactions
{
    public interface IInteractionSystem
    {
        public event Action<ItemCategory> OnInteraction;
    }
}