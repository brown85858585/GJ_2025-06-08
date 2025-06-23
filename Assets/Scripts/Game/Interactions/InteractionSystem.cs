using System;
using System.Collections.Generic;
using Player.Interfaces;
using UnityEngine;

namespace Game.Interactions
{
    public class InteractionSystem : IInteractionSystem, IDisposable
    {
        public event Action<ItemCategory> OnInteraction;
        public event Action ExitInteraction;
        public ItemInteractable CurrentInteractable { get; private set; }

        private RoomView _roomView;
        private readonly IInputAdapter _inputAdapter;
        private List<ItemInteractable> _itemPool = new();
        private int _poolSwitchIndex;

        public InteractionSystem(IInputAdapter inputAdapter)
        {
            _inputAdapter = inputAdapter;
            _inputAdapter.OnInteract += HandleInteract;
            _inputAdapter.OnSwitchInteract += HandleSwitchInteract;
        }

        private void HandleSwitchInteract()
        {
            Debug.Log( $"[HandleSwitchInteract]" +
                       $"Pool{_itemPool.Count} contained {CurrentInteractable?.Category} is:"+_itemPool.Contains(CurrentInteractable));
            if (CurrentInteractable != null)
            {
                var findIndex = _itemPool.FindIndex(currentInteractable => currentInteractable == CurrentInteractable);
                if (findIndex != -1)
                {
                   _poolSwitchIndex = findIndex;
                }
                else
                {
                    _poolSwitchIndex = 0;
                }
                
                _poolSwitchIndex++;
                if(_itemPool.Count-1 < _poolSwitchIndex)
                {
                    _poolSwitchIndex = 0;
                }
                
                CurrentInteractable = _itemPool[_poolSwitchIndex];
                CurrentInteractable.TurnPopup();
            }
            else
            {
                CurrentInteractable = _itemPool[_poolSwitchIndex];
                if(_itemPool.Count-1 >_poolSwitchIndex)
                {
                    
                    _poolSwitchIndex++;
                }
                else
                {
                    _poolSwitchIndex = 0;
                }
                
                CurrentInteractable.TurnPopup(true);
            }
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
            if (_roomView?.ObjectsToInteract != null)
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
            }
            
            _itemPool.Remove(item);
            _poolSwitchIndex = 0;
            Debug.Log( $"[RemoveItemInteractable]" +
                       $"Pool{_itemPool.Count} contained {item.Category} is:"+_itemPool.Contains(item));

        }

        private void SetItemInteractable(ItemInteractable item)
        {
            _itemPool.Add(item);
            _poolSwitchIndex = 0;
            Debug.Log( $"[SetItemInteractable]" +
                       $"Pool{_itemPool.Count} contained {item.Category} is:"+_itemPool.Contains(item));
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