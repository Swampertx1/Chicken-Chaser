using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;

namespace GabesCommonUtility.Editor
{
    [InitializeOnLoad]
    public static class RequiredPackageInstaller
    {
        private const string PrefsKey = "GabesCommonUtility_HasShownPackagePrompt";
        
        private static readonly string[] RecommendedPackages = new string[]
        {
            "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
            "https://github.com/starikcetin/Eflatun.SceneReference.git#4.1.1"
        };

        private static readonly string[] PackageDisplayNames = new string[]
        {
            "UniTask",
            "Scene Reference"
        };

        private static readonly string[] PackageIds = new string[]
        {
            "com.cysharp.unitask",
            "com.eflatun.scenereference"
        };

        static RequiredPackageInstaller()
        {
            // Check if we've already shown the prompt
            if (!EditorPrefs.GetBool(PrefsKey, false))
            {
                EditorApplication.delayCall += ShowInstallPrompt;
            }
        }

        private static void ShowInstallPrompt()
        {
            bool shouldInstall = EditorUtility.DisplayDialog(
                "Recommended Packages",
                "Gabe's Common Utility works best with the following packages:\n\n" +
                "• UniTask - For efficient async operations\n" +
                "• Scene Reference - For type-safe scene references\n\n" +
                "Would you like to install them now?\n\n" +
                "(You can always install them later via Tools > Gabe's Common Utility > Install Recommended Packages)",
                "Install All",
                "Not Now"
            );

            // Mark that we've shown the prompt
            EditorPrefs.SetBool(PrefsKey, true);

            if (shouldInstall)
            {
                InstallAllPackages();
            }
        }

        [MenuItem("Tools/Gabe's Common Utility/Install Recommended Packages/Install All")]
        public static void InstallAllPackages()
        {
            foreach (string package in RecommendedPackages)
            {
                Debug.Log($"Installing package: {package}");
                Client.Add(package);
            }

            EditorUtility.DisplayDialog(
                "Installing Packages",
                $"Installing {RecommendedPackages.Length} recommended packages.\n\n" +
                "Check the Package Manager or Console for progress.",
                "OK"
            );
        }

        [MenuItem("Tools/Gabe's Common Utility/Install Recommended Packages/Install UniTask")]
        public static void InstallUniTask()
        {
            InstallPackage(0);
        }

        [MenuItem("Tools/Gabe's Common Utility/Install Recommended Packages/Install Scene Reference")]
        public static void InstallSceneReference()
        {
            InstallPackage(1);
        }

        private static void InstallPackage(int index)
        {
            Debug.Log($"Installing package: {PackageDisplayNames[index]}");
            Client.Add(RecommendedPackages[index]);
            
            EditorUtility.DisplayDialog(
                "Installing Package",
                $"Installing {PackageDisplayNames[index]}.\n\n" +
                "Check the Package Manager or Console for progress.",
                "OK"
            );
        }

        [MenuItem("Tools/Gabe's Common Utility/Reset Package Prompt")]
        public static void ResetPackagePrompt()
        {
            EditorPrefs.DeleteKey(PrefsKey);
            Debug.Log("Package prompt will show again on next domain reload.");
        }

        [MenuItem("Tools/Gabe's Common Utility/Check Missing Packages")]
        public static void CheckMissingPackages()
        {
            ListRequest listRequest = Client.List(true);
            
            EditorApplication.update += CheckListRequestCompleted;

            void CheckListRequestCompleted()
            {
                if (listRequest.IsCompleted)
                {
                    EditorApplication.update -= CheckListRequestCompleted;
                    
                    if (listRequest.Status == StatusCode.Success)
                    {
                        var installedPackages = new System.Collections.Generic.HashSet<string>();
                        foreach (var package in listRequest.Result)
                        {
                            installedPackages.Add(package.name);
                        }

                        string missingPackages = "";
                        var missingIndices = new System.Collections.Generic.List<int>();
                        
                        for (int i = 0; i < PackageIds.Length; i++)
                        {
                            if (!installedPackages.Contains(PackageIds[i]))
                            {
                                missingPackages += $"• {PackageDisplayNames[i]}\n";
                                missingIndices.Add(i);
                            }
                        }

                        if (string.IsNullOrEmpty(missingPackages))
                        {
                            EditorUtility.DisplayDialog(
                                "All Packages Installed",
                                "All recommended packages are already installed!",
                                "OK"
                            );
                        }
                        else
                        {
                            int choice = EditorUtility.DisplayDialogComplex(
                                "Missing Recommended Packages",
                                "The following recommended packages are not installed:\n\n" + missingPackages,
                                "Install All",
                                "Cancel",
                                "Choose Individual"
                            );

                            if (choice == 0) // Install All
                            {
                                foreach (int index in missingIndices)
                                {
                                    InstallPackage(index);
                                }
                            }
                            else if (choice == 2) // Choose Individual
                            {
                                ShowIndividualPackageMenu(missingIndices);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to check packages: {listRequest.Error.message}");
                    }
                }
            }
        }

        private static void ShowIndividualPackageMenu(System.Collections.Generic.List<int> missingIndices)
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (int index in missingIndices)
            {
                int capturedIndex = index; // Capture for closure
                menu.AddItem(
                    new GUIContent($"Install {PackageDisplayNames[capturedIndex]}"),
                    false,
                    () => InstallPackage(capturedIndex)
                );
            }
            
            menu.ShowAsContext();
        }
    }
}