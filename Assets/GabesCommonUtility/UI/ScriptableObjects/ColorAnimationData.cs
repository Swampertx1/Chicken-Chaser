using UnityEngine;

namespace GabesCommonUtility.UI.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ColorAnimationData", menuName = "GabesCommonUtility/UI/Color Animation Data")]
    public class ColorAnimationDataSo : ScriptableObject
    {
        [SerializeField] private Gradient colorGradient;
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool useOriginalColorAsStart = true;

        public Gradient ColorGradient => colorGradient;
        public bool UseOriginalColorAsStart => useOriginalColorAsStart;
        public float TransitionDuration => transitionDuration;
        public AnimationCurve TransitionCurve => transitionCurve;
    }
}