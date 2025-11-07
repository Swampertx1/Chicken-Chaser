// Created by ChatGPT - Unity Editor Snapshot Tool

using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GabesCommonUtility.Editor
{
    public class SceneSnapshotEditor : EditorWindow
    {
        [MenuItem("Tools/Screenshot/Scene Snapshot")]
        public static void ShowWindow()
        {
            GetWindow<SceneSnapshotEditor>("Snapshot");
        }

        // Editor keybind for capturing a Game view snapshot (Ctrl+Shift+Q).
        [MenuItem("Tools/Screenshot/Snapshot Game View %#q")]
        public static void SnapshotGameViewKeybind()
        {
            CaptureGameViewSnapshot();
        }

        private void OnGUI()
        {
            GUILayout.Label("Snapshot Options", EditorStyles.boldLabel);

            if (GUILayout.Button("Snapshot Scene View"))
            {
                CaptureSceneViewSnapshot();
            }

            if (GUILayout.Button("Snapshot Game View (using Camera.main)"))
            {
                CaptureGameViewSnapshot();
            }
        }

        // Captures the currently active Scene view.
        private static void CaptureSceneViewSnapshot()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                Debug.LogError("No active Scene view found!");
                return;
            }

            Camera sceneCamera = sceneView.camera;
            if (sceneCamera == null)
            {
                Debug.LogError("No camera found in the Scene view!");
                return;
            }

            int width = (int)sceneView.position.width;
            int height = (int)sceneView.position.height;

            RenderTexture rt = new RenderTexture(width, height, 24);
            sceneCamera.targetTexture = rt;
            Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGB24, false);

            sceneCamera.Render();
            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            snapshot.Apply();

            sceneCamera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);

            SaveSnapshot(snapshot, "SceneSnapshot");
        }

        // Captures the Game view snapshot using Camera.main.
        // This requires the game is running (play mode) and a camera tagged "MainCamera" exists.
        private static void CaptureGameViewSnapshot()
        {

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found. Please ensure a camera is tagged 'MainCamera' in your scene.");
                return;
            }

            // Retrieve the actual Game view size via reflection.
            Vector2 gameSize = GetGameViewSize();
            int width = (int)gameSize.x;
            int height = (int)gameSize.y;

            RenderTexture rt = new RenderTexture(width, height, 24);
            mainCamera.targetTexture = rt;
            Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGB24, false);

            mainCamera.Render();
            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            snapshot.Apply();

            mainCamera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);

            SaveSnapshot(snapshot, "GameSnapshot");
        }

        // Uses reflection to retrieve the Game view's actual resolution.
        private static Vector2 GetGameViewSize()
        {
            System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            EditorWindow gameViewWindow = EditorWindow.GetWindow(gameViewType);
            if (gameViewWindow == null)
            {
                return new Vector2(Screen.width, Screen.height);
            }

            PropertyInfo actualViewSizeProp = gameViewType.GetProperty("actualViewSize", BindingFlags.Instance | BindingFlags.Public);
            if (actualViewSizeProp != null)
            {
                return (Vector2)actualViewSizeProp.GetValue(gameViewWindow, null);
            }
            return new Vector2(Screen.width, Screen.height);
        }

        // Saves the snapshot image to Assets/Snapshots with a timestamp.
        private static void SaveSnapshot(Texture2D snapshot, string filenamePrefix)
        {
            string folderPath = "Assets/Snapshots";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filename = Path.Combine(folderPath, $"{filenamePrefix}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            File.WriteAllBytes(filename, snapshot.EncodeToPNG());

            Debug.Log($"{filenamePrefix} saved: " + filename);
            AssetDatabase.Refresh();
        }
    }
}
