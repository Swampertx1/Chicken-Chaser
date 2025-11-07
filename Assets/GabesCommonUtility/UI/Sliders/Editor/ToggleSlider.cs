using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(GabesCommonUtility.UI.Sliders.ToggleSlider))]
[CanEditMultipleObjects]
public class ToggleSliderEditor : Editor
{
    private SerializedProperty _stateProp;

    private void OnEnable()
    {
        _stateProp = serializedObject.FindProperty("state");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw state (On/Off) as a toggle (serialized so undo & multi-edit work)
        EditorGUILayout.PropertyField(_stateProp, new GUIContent("State (On/Off)"));

        EditorGUILayout.Space();

        // We'll iterate the serialized properties in their declared order and draw them,
        // skipping the numeric range/value fields and transition child props (which we'll
        // draw inline when we hit m_Transition).
        var skip = new HashSet<string>
        {
            "m_MinValue",
            "m_MaxValue",
            "m_Value",
            "m_WholeNumbers",
            // transition child props — drawn inline with m_Transition
            "m_Colors",
            "m_SpriteState",
            "m_AnimationTriggers",
            "m_TargetGraphic"
        };

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        // Iterate and draw each property in the same order Unity would
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // We've already drawn the state property above; skip it here
            if (iterator.name == "state")
                continue;

            // Handle the transition property specially so we can show only the relevant sub-fields
            if (iterator.name == "m_Transition")
            {
                EditorGUILayout.PropertyField(iterator, true);

                var transition = (Selectable.Transition)iterator.enumValueIndex;
                var targetGraphicProp = serializedObject.FindProperty("m_TargetGraphic");

                switch (transition)
                {
                    case Selectable.Transition.ColorTint:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Colors"), true);
                        EditorGUILayout.PropertyField(targetGraphicProp, true);
                        break;

                    case Selectable.Transition.SpriteSwap:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SpriteState"), true);
                        EditorGUILayout.PropertyField(targetGraphicProp, true);
                        break;

                    case Selectable.Transition.Animation:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AnimationTriggers"), true);
                        EditorGUILayout.PropertyField(targetGraphicProp, true);
                        break;

                    case Selectable.Transition.None:
                    default:
                        // No extra fields for None
                        break;
                }

                // continue so we don't draw the transition children again later
                continue;
            }

            // Skip the properties we want hidden
            if (skip.Contains(iterator.name))
                continue;

            // Default draw for everything else (this keeps the same order / layout as Unity)
            EditorGUILayout.PropertyField(iterator, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
