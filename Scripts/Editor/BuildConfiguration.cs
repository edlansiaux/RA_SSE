using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;

namespace RASSE.Editor
{
    /// <summary>
    /// Configuration et automatisation des builds Unity pour RA-SSE.
    /// Supporte les builds Android (RealWear), Windows Standalone et WebGL.
    /// </summary>
    public static class BuildConfiguration
    {
        private const string BUILD_FOLDER = "Builds";
        private const string COMPANY_NAME = "RASSE";
        private const string PRODUCT_NAME = "RA-SSE Simulateur Triage";
        private const string VERSION = "1.0.0";

        // Scènes à inclure dans le build
        private static readonly string[] SCENES = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/TrainingScene.unity",
            "Assets/Scenes/Scenario_IndustrialExplosion.unity",
            "Assets/Scenes/Scenario_TrainAccident.unity",
            "Assets/Scenes/Scenario_BuildingCollapse.unity"
        };

        #region Menu Items

        [MenuItem("RA-SSE/Build/Android (RealWear)", false, 100)]
        public static void BuildAndroid()
        {
            BuildForPlatform(BuildTarget.Android, "RA-SSE.apk");
        }

        [MenuItem("RA-SSE/Build/Windows Standalone", false, 101)]
        public static void BuildWindows()
        {
            BuildForPlatform(BuildTarget.StandaloneWindows64, "RA-SSE.exe");
        }

        [MenuItem("RA-SSE/Build/WebGL", false, 102)]
        public static void BuildWebGL()
        {
            BuildForPlatform(BuildTarget.WebGL, "RA-SSE-WebGL");
        }

        [MenuItem("RA-SSE/Build/All Platforms", false, 200)]
        public static void BuildAll()
        {
            BuildAndroid();
            BuildWindows();
            BuildWebGL();
            Debug.Log("[BuildConfiguration] Tous les builds terminés!");
        }

        [MenuItem("RA-SSE/Build/Open Build Folder", false, 300)]
        public static void OpenBuildFolder()
        {
            string path = Path.Combine(Application.dataPath, "..", BUILD_FOLDER);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            EditorUtility.RevealInFinder(path);
        }

        #endregion

        #region Build Methods

        private static void BuildForPlatform(BuildTarget target, string outputName)
        {
            string platformFolder = GetPlatformFolderName(target);
            string buildPath = Path.Combine(BUILD_FOLDER, platformFolder, outputName);
            
            // Créer le dossier si nécessaire
            string directory = Path.GetDirectoryName(buildPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Configurer les options de build
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = SCENES,
                locationPathName = buildPath,
                target = target,
                options = BuildOptions.None
            };

            // Configurer les paramètres spécifiques à la plateforme
            ConfigurePlatformSettings(target);

            // Lancer le build
            Debug.Log($"[BuildConfiguration] Démarrage du build {target}...");
            BuildReport report = BuildPipeline.BuildPlayer(options);

            // Analyser le résultat
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildConfiguration] Build {target} réussi!");
                Debug.Log($"  Chemin: {buildPath}");
                Debug.Log($"  Taille: {report.summary.totalSize / 1024 / 1024} MB");
                Debug.Log($"  Durée: {report.summary.totalTime.TotalSeconds:F1}s");
            }
            else
            {
                Debug.LogError($"[BuildConfiguration] Build {target} échoué!");
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                        {
                            Debug.LogError($"  {message.content}");
                        }
                    }
                }
            }
        }

        private static string GetPlatformFolderName(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.Android => "Android",
                BuildTarget.StandaloneWindows64 => "Windows",
                BuildTarget.StandaloneOSX => "macOS",
                BuildTarget.WebGL => "WebGL",
                BuildTarget.iOS => "iOS",
                _ => target.ToString()
            };
        }

        private static void ConfigurePlatformSettings(BuildTarget target)
        {
            // Paramètres communs
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.bundleVersion = VERSION;

            switch (target)
            {
                case BuildTarget.Android:
                    ConfigureAndroidSettings();
                    break;
                case BuildTarget.StandaloneWindows64:
                    ConfigureWindowsSettings();
                    break;
                case BuildTarget.WebGL:
                    ConfigureWebGLSettings();
                    break;
            }
        }

        private static void ConfigureAndroidSettings()
        {
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            
            // Configuration pour RealWear Navigator 520
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Permissions pour la caméra et le microphone
            // Note: Ces permissions sont définies dans AndroidManifest.xml
            
            // Définir les symboles de compilation
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Android, 
                "UNITY_AR;REALWEAR_DEVICE"
            );
        }

        private static void ConfigureWindowsSettings()
        {
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.resizableWindow = true;
            
            // Mode debug pour développement
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone, 
                "UNITY_EDITOR_DEBUG"
            );
        }

        private static void ConfigureWebGLSettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.memorySize = 512;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
        }

        #endregion

        #region Validation

        [MenuItem("RA-SSE/Validate/Check Scenes", false, 400)]
        public static void ValidateScenes()
        {
            Debug.Log("[BuildConfiguration] Validation des scènes...");
            
            foreach (string scenePath in SCENES)
            {
                if (File.Exists(scenePath))
                {
                    Debug.Log($"  ✓ {scenePath}");
                }
                else
                {
                    Debug.LogError($"  ✗ {scenePath} - MANQUANTE!");
                }
            }
        }

        [MenuItem("RA-SSE/Validate/Check Dependencies", false, 401)]
        public static void ValidateDependencies()
        {
            Debug.Log("[BuildConfiguration] Validation des dépendances...");
            
            // Vérifier les packages requis
            string[] requiredPackages = new string[]
            {
                "com.unity.inputsystem",
                "com.unity.textmeshpro",
                "com.unity.ai.navigation"
            };

            // Note: La vérification complète nécessiterait l'API Package Manager
            Debug.Log("  Voir Packages/manifest.json pour la liste des packages");
        }

        #endregion
    }
}
