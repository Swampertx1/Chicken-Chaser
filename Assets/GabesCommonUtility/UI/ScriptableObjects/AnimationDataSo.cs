using UnityEngine;

namespace GabesCommonUtility.UI.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AnimationData", menuName = "GabesCommonUtility/UI/Animation Data")]
    public class AnimationDataSo : ScriptableObject
    {
        [SerializeField] private Vector2 hoverSize = Vector2.one;
        [SerializeField] private bool useLiteralScale = true;
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private AnimationCurve transitionCurve = new(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.1f, -0.2f, 0f, 2f),
            new Keyframe(0.9f, 1.2f, 2f, 0f),
            new Keyframe(1f, 1f, 0f, 0f)
        );

        public Vector2 HoverSize => hoverSize;
        public bool UseLiteralScale => useLiteralScale;
        public float TransitionDuration => transitionDuration;
        public AnimationCurve TransitionCurve => transitionCurve;
    }
}