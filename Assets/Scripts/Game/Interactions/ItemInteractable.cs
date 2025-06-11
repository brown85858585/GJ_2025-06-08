using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class ItemInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject popup;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private string id;
        [SerializeField] private ItemCategory category;
        
        public bool CheckStayCollider{ get; set; }

        public string Guid => id;
        public ItemCategory Category => category;

        public event Action<ItemInteractable> OnEnter;
        public event Action<ItemInteractable> OnExit;

        private void Awake()
        {
            targetMask = LayerMask.GetMask("Player");
        }

        private void OnTriggerStay(Collider other)
        {
            if (!CheckStayCollider) return;
            
            if (CheckLayerMask(other.gameObject, targetMask))
            {
                OnEnter?.Invoke(this);
            
                TurnPopup();
            }

            CheckStayCollider = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CheckLayerMask(other.gameObject, targetMask))
            {
                OnEnter?.Invoke(this);

                TurnPopup();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CheckLayerMask(other.gameObject, targetMask))
            {
                OnExit?.Invoke(this);

                TurnPopup(false);
            }
        }

        public void Interact()
        {
            Debug.Log("Interact with item: " + Category + " with ID: " + Guid);

            TurnPopup(false);
        }

        public void TurnPopup(bool turn = true)
        {
            if (popup != null)
            {
                popup.SetActive(turn);

                if (turn)
                {
                    popup.gameObject.GetComponent<UIElementTweener>()?.Show();
                }
                else
                {
                    popup.gameObject.GetComponent<UIElementTweener>()?.Hide();
                }
            }
        }

        private bool CheckLayerMask(GameObject obj, LayerMask layers)
        {
            if (((1 << obj.layer) & layers) != 0)
            {
                return true;
            }

            return false;
        }
#if UNITY_EDITOR
        // генерация GUID при первом создании
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
                id = GUID.Generate().ToString();
        }
#endif
    }
}