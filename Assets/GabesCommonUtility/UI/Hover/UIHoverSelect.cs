using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Hover
{
    public class UIHoverSelect : MonoBehaviour
    {
        private EventSystem _eventSystem;
        private GraphicRaycaster _raycaster;
        private PointerEventData _pointerEventData;
        private GameObject _lastHoveredObject;
        private readonly List<RaycastResult> _raycastResults = new(10);
        private Mouse _mouse;

        void Start()
        {
            // Cache mouse reference
            _mouse = Mouse.current;
            if (_mouse == null)
            {
                Debug.LogError("No mouse detected!");
                enabled = false;
                return;
            }

            // Get required components
            _eventSystem = EventSystem.current;
            if (_eventSystem == null)
            {
                Debug.LogError("No EventSystem found in scene! Please add one.");
                enabled = false;
                return;
            }

            _raycaster = GetComponent<GraphicRaycaster>();
            if (_raycaster == null)
            {
                Debug.LogError("GraphicRaycaster not found on Canvas!");
                enabled = false;
                return;
            }

            _pointerEventData = new PointerEventData(_eventSystem);
        }

        void Update()
        {
            // Get mouse position using cached reference
            _pointerEventData.position = _mouse.position.ReadValue();

            // Raycast to find UI elements under mouse
            _raycastResults.Clear();
            _raycaster.Raycast(_pointerEventData, _raycastResults);

            // Early exit if no results
            if (_raycastResults.Count == 0)
            {
                _lastHoveredObject = null;
                return;
            }

            GameObject hoveredObject = null;

            // Find the first selectable object
            for (int i = 0; i < _raycastResults.Count; i++)
            {
                var selectable = _raycastResults[i].gameObject.GetComponent<Selectable>();
                if (selectable != null && selectable.IsInteractable())
                {
                    hoveredObject = _raycastResults[i].gameObject;
                    break;
                }
            }

            // Update selection if hovered object changed
            if (hoveredObject != _lastHoveredObject)
            {
                if (hoveredObject != null)
                {
                    _eventSystem.SetSelectedGameObject(hoveredObject);
                }
                _lastHoveredObject = hoveredObject;
            }
        }
    }
}