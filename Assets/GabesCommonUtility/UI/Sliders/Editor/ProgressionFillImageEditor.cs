#if UNITY_EDITOR
using GabesCommonUtility.UI.Sliders;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Editor
{
    [CustomEditor(typeof(ProgressionFillImage), true)]
    [CanEditMultipleObjects]
    public class ProgressionFillImageEditor : ImageEditor
    {
        private SerializedProperty _delayProperty;
        private SerializedProperty _speedProperty;
        private SerializedProperty _animationCurveProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _delayProperty = serializedObject.FindProperty("delay");
            _speedProperty = serializedObject.FindProperty("speed");
            _animationCurveProperty = serializedObject.FindProperty("animationCurve");

            // Force image type to filled
            foreach (var t in targets)
            {
                var image = t as ProgressionFillImage;
                if (image != null)
                {
                    image.type = Image.Type.Filled;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw progression settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Progression Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_delayProperty, new GUIContent("Delay", "Time to wait before starting animation"));
            EditorGUILayout.PropertyField(_speedProperty, new GUIContent("Speed", "Fill amount units per second"));
            EditorGUILayout.PropertyField(_animationCurveProperty, new GUIContent("Animation Curve", "Curve to follow during animation"));

            EditorGUILayout.Space();

            // Draw image settings
            EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);
            
            SerializedProperty sprite = serializedObject.FindProperty("m_Sprite");
            SerializedProperty color = serializedObject.FindProperty("m_Color");
            SerializedProperty material = serializedObject.FindProperty("m_Material");
            SerializedProperty raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            SerializedProperty raycastPadding = serializedObject.FindProperty("m_RaycastPadding");
            SerializedProperty maskable = serializedObject.FindProperty("m_Maskable");
            SerializedProperty fillMethod = serializedObject.FindProperty("m_FillMethod");
            SerializedProperty fillOrigin = serializedObject.FindProperty("m_FillOrigin");
            SerializedProperty fillClockwise = serializedObject.FindProperty("m_FillClockwise");
            SerializedProperty preserveAspect = serializedObject.FindProperty("m_PreserveAspect");

            if (sprite != null) EditorGUILayout.PropertyField(sprite);
            if (color != null) EditorGUILayout.PropertyField(color);
            if (material != null) EditorGUILayout.PropertyField(material);
            if (raycastTarget != null) EditorGUILayout.PropertyField(raycastTarget);
            if (raycastPadding != null) EditorGUILayout.PropertyField(raycastPadding);
            if (maskable != null) EditorGUILayout.PropertyField(maskable);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fill Settings", EditorStyles.boldLabel);
            
            // Draw fill amount slider
            if (targets.Length == 1)
            {
                var image = target as ProgressionFillImage;
                if (image != null)
                {
                    EditorGUI.BeginChangeCheck();
                    float newFillAmount = EditorGUILayout.Slider("Fill Amount", image.fillAmount, 0f, 1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        image.fillAmount = newFillAmount;
                        EditorUtility.SetDirty(image);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Fill Amount editing only available for single selection.", MessageType.Info);
            }

            if (fillMethod != null) EditorGUILayout.PropertyField(fillMethod);
            if (fillOrigin != null) EditorGUILayout.PropertyField(fillOrigin);
            if (fillClockwise != null) EditorGUILayout.PropertyField(fillClockwise);
            if (preserveAspect != null) EditorGUILayout.PropertyField(preserveAspect);

            serializedObject.ApplyModifiedProperties();

            // Ensure image type stays filled
            foreach (var t in targets)
            {
                var image = t as ProgressionFillImage;
                if (image != null && image.type != Image.Type.Filled)
                {
                    image.type = Image.Type.Filled;
                    EditorUtility.SetDirty(image);
                }
            }

            // Test buttons
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Test Controls", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set to 0%"))
                {
                    foreach (var t in targets)
                    {
                        var image = t as ProgressionFillImage;
                        if (image != null) image.fillAmountAnimated = 0f;
                    }
                }
                if (GUILayout.Button("Set to 100%"))
                {
                    foreach (var t in targets)
                    {
                        var image = t as ProgressionFillImage;
                        if (image != null) image.fillAmountAnimated = 1f;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set to 25%"))
                {
                    foreach (var t in targets)
                    {
                        var image = t as ProgressionFillImage;
                        if (image != null) image.fillAmountAnimated = 0.25f;
                    }
                }
                if (GUILayout.Button("Set to 50%"))
                {
                    foreach (var t in targets)
                    {
                        var image = t as ProgressionFillImage;
                        if (image != null) image.fillAmountAnimated = 0.5f;
                    }
                }
                if (GUILayout.Button("Set to 75%"))
                {
                    foreach (var t in targets)
                    {
                        var image = t as ProgressionFillImage;
                        if (image != null) image.fillAmountAnimated = 0.75f;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Set Immediate (No Animation)"))
                {
                    foreach (var t in targets)
                    {
                        var image = t as ProgressionFillImage;
                        if (image != null) image.SetFillAmountImmediate(1f);
                    }
                }
            }
        }
    }
}
#endif