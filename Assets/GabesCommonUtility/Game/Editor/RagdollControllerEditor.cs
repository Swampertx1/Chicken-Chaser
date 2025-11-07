using UnityEngine;
using UnityEditor;

namespace GabesCommonUtility.Game
{

[CustomEditor(typeof(RagdollController))]
public class RagdollControllerEditor : Editor
{
    private SerializedProperty _startRagdolled;
    private SerializedProperty _ragdollRigidbodies;
    private SerializedProperty _coreRigidbody;
    private SerializedProperty _enableOnRagdoll;
    private SerializedProperty _disableOnRagdoll;
    
    private void OnEnable()
    {
        _startRagdolled = serializedObject.FindProperty("startRagdolled");
        _ragdollRigidbodies = serializedObject.FindProperty("ragdollRigidbodies");
        _coreRigidbody = serializedObject.FindProperty("coreRigidbody");
        _enableOnRagdoll = serializedObject.FindProperty("enableOnRagdoll");
        _disableOnRagdoll = serializedObject.FindProperty("disableOnRagdoll");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        RagdollController controller = (RagdollController)target;
        
        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ragdoll Controller", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Default state
        EditorGUILayout.PropertyField(_startRagdolled, new GUIContent("Start Ragdolled", "Should the ragdoll be active when the game starts?"));
        
        EditorGUILayout.Space();
        
        // Rigidbodies section
        EditorGUILayout.LabelField("Rigidbodies", EditorStyles.boldLabel);
        
        // Gather button
        if (GUILayout.Button("Gather Child Rigidbodies", GUILayout.Height(30)))
        {
            Undo.RecordObject(controller, "Gather Rigidbodies");
            controller.GatherRigidbodies();
            EditorUtility.SetDirty(controller);
        }
        
        EditorGUILayout.PropertyField(_coreRigidbody, new GUIContent("Core Rigidbody"), true);
        EditorGUILayout.PropertyField(_ragdollRigidbodies, new GUIContent("Ragdoll Rigidbodies"), true);
        
        EditorGUILayout.Space();
        
        // Behaviors section
        EditorGUILayout.LabelField("Behavior Management", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_enableOnRagdoll, new GUIContent("Enable When Ragdolled"), true);
        EditorGUILayout.PropertyField(_disableOnRagdoll, new GUIContent("Disable When Ragdolled"), true);
        
        EditorGUILayout.Space();
        
        // Runtime controls
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Start Ragdoll", GUILayout.Height(35)))
            {
                controller.SetRagdoll(true);
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Stop Ragdoll", GUILayout.Height(35)))
            {
                controller.SetRagdoll(false);
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            GUI.backgroundColor = new Color(1f, 0.7f, 0.2f);
            if (GUILayout.Button("Apply Random Force (10)", GUILayout.Height(35)))
            {
                Vector3 randomForce = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(5f, 10f),
                    Random.Range(-10f, 10f)
                );
                controller.ApplyForce(randomForce);
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Ragdoll is currently: {(controller.IsRagdolled ? "ENABLED" : "DISABLED")}", 
                controller.IsRagdolled ? MessageType.Warning : MessageType.Info);
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enter Play Mode to test ragdoll controls", MessageType.Info);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
}