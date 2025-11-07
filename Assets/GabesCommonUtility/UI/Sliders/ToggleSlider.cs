using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Sliders
{
    public class ToggleSlider : Slider
    {
        [SerializeField, Tooltip("Current toggle state.")]
        private bool state;

        public bool State
        {
            get => state;
            set
            {
                if (state == value) return;
                state = value;
                UpdateVisualState();
                onValueChanged.Invoke(value?maxValue:minValue);
            }
        }

        protected override void Start()
        {
            base.Start();
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            // Clamp strictly to min/max depending on toggle state
            value = state ? maxValue : minValue;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            // Toggle instead of dragging
            State = !State;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            // Disable dragging behavior entirely
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            UpdateVisualState();
        }
#endif
    }
}