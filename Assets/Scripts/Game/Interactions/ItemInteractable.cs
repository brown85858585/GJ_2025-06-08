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

        [FormerlySerializedAs("multiplyInteractable")]
        [Header("Popup Settings")] 
        [SerializeField] private bool isMultiplyInteractable = true;
        [SerializeField] private Vector3 popupOffset = new Vector3(0, 1.5f, 0);
        [SerializeField] private float popupScale = 1;
        
        public bool IsMultiplyInteractable => isMultiplyInteractable;
        public string Guid => id;
        public ItemCategory Category => category;

        public event Action<ItemInteractable> OnEnter;
        public event Action<ItemInteractable> OnExit;
        

        private FollowProjectionWithConstantSize _popup;
        private UIElementTweener _popupTweener;
        private PopupInteractionView _popupIcon;

        private void Awake()
        {
            targetMask = LayerMask.GetMask("Player");
        }

        private void Start()
        {
            _popup?.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (LayerChecker.CheckLayerMask(other.gameObject, targetMask))
            {
                OnEnter?.Invoke(this);

                TurnPopup();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (LayerChecker.CheckLayerMask(other.gameObject, targetMask))
            {
                OnExit?.Invoke(this);

                TurnPopup(false);
            }
        }

        public void DisableMultiplyInteractable()
        {
            isMultiplyInteractable = false;
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
                _popupIcon.SetInteractionImage(category);

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

        public void AddPopup(FollowProjectionWithConstantSize itemCollectionPopupForInteract)
        {
            _popup = itemCollectionPopupForInteract.GetComponent<FollowProjectionWithConstantSize>();
            _popupTweener = _popup.GetComponent<UIElementTweener>();
            _popupIcon = _popup.GetComponent<PopupInteractionView>();
           
            if (_popup == null)
            {
                Debug.LogError("PopupE not found in the scene. Please ensure it exists.");
            }
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