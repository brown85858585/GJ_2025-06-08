using System;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class ItemInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private LayerMask targetMask = 1 << 6;
        [SerializeField] private string id;
        [SerializeField] private ItemCategory category;
        
        [Header("Popup Settings")]
        [SerializeField] private Vector3 popupOffset = new Vector3(0, 1.5f, 0);
        [SerializeField] private int popupScale = 1;
        
        public bool CheckStayCollider{ get; set; }
        public string Guid => id;
        public ItemCategory Category => category;

        public event Action<ItemInteractable> OnEnter;
        public event Action<ItemInteractable> OnExit;
        

        private FollowProjectionWithConstantSize _popup;
        private UIElementTweener _popupTweener;
        
        private void Awake()
        {
            targetMask = LayerMask.GetMask("Player");
            
            
            _popup = GameObject.Find("PopupE").GetComponent<FollowProjectionWithConstantSize>();
            _popupTweener = _popup.GetComponent<UIElementTweener>();
            if (_popup == null)
            {
                Debug.LogError("PopupE not found in the scene. Please ensure it exists.");
            }
        }

        private void Start()
        {
            _popup.gameObject.SetActive(false);
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
            if (_popup != null)
            {
                _popup.gameObject.SetActive(turn);
                _popup.target = transform;
                _popup.worldOffset = popupOffset;
                _popupTweener.scaleFactor = popupScale;

                if (turn)
                {
                    _popupTweener?.Show();
                }
                else
                {
                    _popupTweener?.Hide();
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