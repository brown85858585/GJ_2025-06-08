using System;

namespace Game.Interactions
{
    public interface IInteractionSystem
    {
        public event Action<ItemCategory> OnInteraction;
        public event Action ExitInteraction;
        
        public void DisableCurrentMultiplyInteractable();
    }
}