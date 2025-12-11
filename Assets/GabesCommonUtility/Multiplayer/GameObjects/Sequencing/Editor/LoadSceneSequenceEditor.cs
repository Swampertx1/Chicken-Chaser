#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    [CustomEditor(typeof(LoadSceneSequence))]
    public class LoadSceneSequenceEditor : Editor
    {
        private SerializedProperty _nextProp;
        private SerializedProperty _loadModeProp;
        private SerializedProperty _scenesToLoadProp;
        private SerializedProperty _scenesToUnloadProp;
        private SerializedProperty _useLoadingScreenProp;

        private void OnEnable()
        {
            _nextProp = serializedObject.FindProperty("next");
            _loadModeProp = serializedObject.FindProperty("loadMode");
            _scenesToLoadProp = serializedObject.FindProperty("scenesToLoad");
            _scenesToUnloadProp = serializedObject.FindProperty("scenesToUnload");
            _useLoadingScreenProp = serializedObject.FindProperty("useLoadingScreen");
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_nextProp);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_loadModeProp);
            
            UnityEngine.SceneManagement.LoadSceneMode loadMode = 
                (UnityEngine.SceneManagement.LoadSceneMode)_loadModeProp.enumValueIndex;

            EditorGUILayout.Space();

            // Handle scenesToLoad based on loadMode
            if (loadMode == UnityEngine.SceneManagement.LoadSceneMode.Single)
            {
                EditorGUILayout.LabelField("Scenes To Load", EditorStyles.boldLabel);
                
                // Ensure array size is exactly 1
                if (_scenesToLoadProp.arraySize != 1)
                {
                    _scenesToLoadProp.arraySize = 1;
                }

                // Draw the single element
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_scenesToLoadProp.GetArrayElementAtIndex(0), new GUIContent("Scene"), true);
                EditorGUI.indentLevel--;

                // Show warning if user tries to add more
                EditorGUILayout.HelpBox("Single mode only allows one scene to load.", MessageType.Info);
            }
            else // Additive mode
            {
                EditorGUILayout.PropertyField(_scenesToLoadProp, new GUIContent("Scenes To Load"), true);
                EditorGUILayout.PropertyField(_scenesToUnloadProp, new GUIContent("Scenes To Unload"), true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_useLoadingScreenProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif