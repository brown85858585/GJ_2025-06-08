using System;
using Player.Interfaces;

namespace Game.Interactions
{
    public class InteractionSystem : IInteractionSystem, IDisposable
    {
        public event Action<ItemCategory> OnInteraction;
        public event Action ExitInteraction;

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
            ExitInteraction?.Invoke();
            
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
                OnInteraction?.Invoke(CurrentInteractable.Category);
                
                if (!CurrentInteractable.IsMultiplyInteractable)
                {
                    CurrentInteractable.Interact();
                    CurrentInteractable = null;
                }
            }
        }

        public void DisableCurrentMultiplyInteractable()
        {
            if (CurrentInteractable != null && CurrentInteractable.IsMultiplyInteractable)
            {
                CurrentInteractable.DisableMultiplyInteractable();
            }
        }
        
        public void ClearAll()
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
            CurrentInteractable = null;
        }
    }
}