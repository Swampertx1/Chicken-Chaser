using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Sliders
{
    [DefaultExecutionOrder(1000)]
    public class SliderText : MonoBehaviour
    {
        private enum ESliderType
        {
            Percentage,
            Number,
            NumberWithMax,
            Hidden
        }

        [SerializeField] private ESliderType sliderType;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private bool inverse;
        private float _maxValue;
        private float _currentValue;


        private void OnEnable()
        {
            _currentValue = slider.value;
            _maxValue = slider.maxValue;

            float percent = _currentValue / _maxValue;
            if(inverse) percent = 1 - percent;
            slider.SetValueWithoutNotify(percent);
            UpdateSliderText(percent);
            
            slider.onValueChanged.AddListener(UpdateCurrent);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(UpdateCurrent);
        }

        public void UpdateMax(float value)
        {
            _maxValue = value;
            UpdateCurrent(_currentValue);
        }

        public void UpdateCurrent(float value)
        {
            _currentValue = value;
        
            float percent = _currentValue / _maxValue;
            if(inverse) percent = 1 - percent;
            UpdateSliderText(percent);
        }

        private void UpdateSliderText(float percent)
        {
            switch (sliderType)
            {
                case ESliderType.Percentage:
                    text.text = ((int)(percent * 100)) + "%";
                    break;
                case ESliderType.Number:
                    text.text = _currentValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case ESliderType.NumberWithMax:
                    text.text = _currentValue.ToString(CultureInfo.InvariantCulture) + "/" + _maxValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case ESliderType.Hidden:
                    text?.gameObject.SetActive(false);
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (slider == null)
            {
                slider = GetComponentInChildren<Slider>();
            }

            if (text != null)
            {
                UpdateSliderText(slider.value / slider.maxValue);

            }

        }
#endif

    }
}
