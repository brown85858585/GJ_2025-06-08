using System;
using UnityEngine;

namespace Game.Interactions
{
    [RequireComponent(typeof(SphereCollider))]
    public class ItemInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private LayerMask targetMask;
        
        public event Action<ItemInteractable> OnEnter;
        public event Action<ItemInteractable> OnExit;
        
        private SphereCollider sphereCollider;
        
        private void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            targetMask = LayerMask.GetMask("Player");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (CheckLayerMask(other.gameObject, targetMask))
            {
                OnEnter?.Invoke(this);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (CheckLayerMask(other.gameObject, targetMask))
            {
                OnExit?.Invoke(this);
            }
        }

        public void Interact()
        {
            Debug.Log("Interact with item: " + gameObject.name);
        }

        private bool CheckLayerMask(GameObject obj, LayerMask layers)
        {
            if (((1 << obj.layer) & layers) != 0)
            {
                return true;
            }

            return false;
        }
    }
}