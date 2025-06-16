using System;
using Player.Interfaces;

namespace Game.Interactions
{
    public class InteractionSystem : IInteractionSystem, IDisposable
    {
        public event Action<ItemCategory> OnInteraction;
        
        private RoomView _roomView = new ();
        private readonly IInputAdapter _inputAdapter;
        public ItemInteractable CurrentInteractable { get; private set; }
        public InteractionSystem(IInputAdapter inputAdapter)
        {
            _inputAdapter = inputAdapter;
            _inputAdapter.OnInteract += HandleInteract;
        }

        public void Dispose()
        {
            foreach (var item in _roomView.ObjectsToInteract)
            {
                item.OnEnter -= SetItemInteractable;
                item.OnExit -= RemoveItemInteractable;
            }
            _inputAdapter.OnInteract -= HandleInteract;
        }

        public void AddNewInteractionCollection(RoomView itemCollection)
        {
            if (_roomView.ObjectsToInteract != null)
            {
                foreach (var item in _roomView.ObjectsToInteract)
                {
                    item.OnEnter -= SetItemInteractable;
                    item.OnExit -= RemoveItemInteractable;
                }
                _roomView.ObjectsToInteract.Clear();
                
            }
            _roomView = itemCollection;
           
            foreach (var item in _roomView.ObjectsToInteract)
            {
                item.OnEnter += SetItemInteractable;
                item.OnExit += RemoveItemInteractable;
            }
            
        }
        
        private void RemoveItemInteractable(ItemInteractable item)
        {
            if (CurrentInteractable == item)
            {
                CurrentInteractable = null;

                foreach (var itemInCollection in _roomView.ObjectsToInteract)
                {
                    itemInCollection.CheckStayCollider = true;
                }
            }
        }

        private void SetItemInteractable(ItemInteractable item)
        {
            CurrentInteractable?.TurnPopup(false);
            CurrentInteractable = item;
        }

        private void HandleInteract(bool interact)
        {
            if (interact && CurrentInteractable != null)
            {
                CurrentInteractable.Interact();
                OnInteraction?.Invoke(CurrentInteractable.Category);
                CurrentInteractable = null;
            }
        }
    }
}