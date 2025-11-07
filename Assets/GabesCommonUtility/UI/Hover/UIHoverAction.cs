using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Hover
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UnityEvent onHover;
        [SerializeField] private UnityEvent onHoverExit;
        private Selectable _selectable;


        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable == null || _selectable.IsInteractable()) return;

            
            onHover?.Invoke();
        }

  
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_selectable == null || _selectable.IsInteractable()) return;

            
            onHoverExit?.Invoke();
        }

    }
}
