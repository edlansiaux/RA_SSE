#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace RASSE.Editor
{
    /// <summary>
    /// Menu et outils d'éditeur pour faciliter la configuration du projet RA-SSE
    /// </summary>
    public class RASSEEditorTools : EditorWindow
    {
        [MenuItem("RA-SSE/Ouvrir Configuration", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<RASSEEditorTools>("RA-SSE Setup");
        }

        [MenuItem("RA-SSE/Créer Structure de Dossiers", false, 20)]
        public static void CreateFolderStructure()
        {
            string[] folders = new string[]
            {
                "Assets/Prefabs",
                "Assets/Prefabs/Victims",
                "Assets/Prefabs/Vehicles",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Markers",
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Scenarios",
                "Assets/ScriptableObjects/Victims",
                "Assets/Scenes",
                "Assets/Materials",
                "Assets/Materials/Victims",
                "Assets/Materials/Vehicles",
                "Assets/Materials/UI",
                "Assets/Audio",
                "Assets/Audio/Music",
                "Assets/Audio/SFX",
                "Assets/Audio/Voice",
                "Assets/Audio/Ambiance",
                "Assets/Models",
                "Assets/Textures",
                "Assets/Animations",
                "Assets/Fonts"
            };

            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Debug.Log($"Dossier créé: {folder}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Structure de dossiers créée avec succès!");
        }

        [MenuItem("RA-SSE/Créer Tags et Layers", false, 21)]
        public static void CreateTagsAndLayers()
        {
            // Tags
            AddTag("Victim");
            AddTag("Rescuer");
            AddTag("Ambulance");
            AddTag("Hospital");
            AddTag("SpawnPoint");
            AddTag("Waypoint");
            AddTag("Hazard");

            // Layers
            AddLayer("Victims");
            AddLayer("Vehicles");
            AddLayer("UI3D");
            AddLayer("Navigation");

            Debug.Log("Tags et Layers créés avec succès!");
        }

        private static void AddTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tag)) { found = true; break; }
            }

            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                n.stringValue = tag;
                tagManager.ApplyModifiedProperties();
            }
        }

        private static void AddLayer(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp.stringValue == "") 
                {
                    sp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
                if (sp.stringValue == layerName) return;
            }
        }

        [MenuItem("RA-SSE/Créer Prefab Victime", false, 40)]
        public static void CreateVictimPrefab()
        {
            // Créer un GameObject victime basique
            GameObject victim = new GameObject("Victim");
            
            // Ajouter les composants
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(victim.transform);
            capsule.transform.localPosition = Vector3.up;
            capsule.name = "Body";

            // Collider pour détection
            var collider = victim.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up;
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.isTrigger = true;

            // Indicateur de catégorie (sphere au-dessus de la tête)
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.SetParent(victim.transform);
            indicator.transform.localPosition = new Vector3(0, 2.5f, 0);
            indicator.transform.localScale = Vector3.one * 0.3f;
            indicator.name = "CategoryIndicator";
            DestroyImmediate(indicator.GetComponent<Collider>());

            // Tag
            victim.tag = "Victim";

            // Sauvegarder comme prefab
            string path = "Assets/Prefabs/Victims/Victim.prefab";
            EnsureDirectoryExists(path);
            
            PrefabUtility.SaveAsPrefabAsset(victim, path);
            DestroyImmediate(victim);

            Debug.Log($"Prefab Victime créé: {path}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        [MenuItem("RA-SSE/Créer Prefab Ambulance", false, 41)]
        public static void CreateAmbulancePrefab()
        {
            GameObject ambulance = new GameObject("Ambulance");

            // Corps principal (cube allongé)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(ambulance.transform);
            body.transform.localScale = new Vector3(2f, 1.5f, 4f);
            body.transform.localPosition = new Vector3(0, 0.75f, 0);
            body.name = "Body";

            // Gyrophare
            var siren = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            siren.transform.SetParent(ambulance.transform);
            siren.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
            siren.transform.localPosition = new Vector3(0, 1.6f, 1f);
            siren.name = "Siren";
            DestroyImmediate(siren.GetComponent<Collider>());

            // Collider
            var collider = ambulance.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 1.5f, 4f);
            collider.center = new Vector3(0, 0.75f, 0);

            // Tag
            ambulance.tag = "Ambulance";

            // Sauvegarder
            string path = "Assets/Prefabs/Vehicles/Ambulance.prefab";
            EnsureDirectoryExists(path);
            
            PrefabUtility.SaveAsPrefabAsset(ambulance, path);
            DestroyImmediate(ambulance);

            Debug.Log($"Prefab Ambulance créé: {path}");
        }

        [MenuItem("RA-SSE/Créer Prefabs Markers AR", false, 42)]
        public static void CreateARMarkerPrefabs()
        {
            CreateMarker("VictimMarker", Color.yellow, "Assets/Prefabs/Markers/VictimMarker.prefab");
            CreateMarker("AmbulanceMarker", Color.blue, "Assets/Prefabs/Markers/AmbulanceMarker.prefab");
            CreateMarker("HospitalMarker", Color.white, "Assets/Prefabs/Markers/HospitalMarker.prefab");
            CreateMarker("WaypointMarker", Color.cyan, "Assets/Prefabs/Markers/WaypointMarker.prefab");
            CreateMarker("DestinationMarker", Color.green, "Assets/Prefabs/Markers/DestinationMarker.prefab");

            Debug.Log("Prefabs Markers AR créés!");
        }

        private static void CreateMarker(string name, Color color, string path)
        {
            GameObject marker = new GameObject(name);

            // Forme de losange/diamant
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(marker.transform);
            visual.transform.localScale = Vector3.one * 0.5f;
            visual.transform.localRotation = Quaternion.Euler(45, 0, 45);
            visual.name = "Visual";
            DestroyImmediate(visual.GetComponent<Collider>());

            // Matériau coloré
            var renderer = visual.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            color.a = 0.8f;
            mat.color = color;
            renderer.material = mat;

            EnsureDirectoryExists(path);
            PrefabUtility.SaveAsPrefabAsset(marker, path);
            DestroyImmediate(marker);
        }

        [MenuItem("RA-SSE/Créer Scène Principale", false, 60)]
        public static void CreateMainScene()
        {
            // Créer une nouvelle scène
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // Terrain basique
            var terrain = new GameObject("Ground");
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(terrain.transform);
            plane.transform.localScale = new Vector3(10, 1, 10);
            plane.name = "Floor";

            // Lumière directionnelle (existe déjà dans DefaultGameObjects)

            // GameManager
            var gameManagerObj = new GameObject("GameManager");
            // Note: Les scripts ne seront pas ajoutés automatiquement car ils nécessitent d'être compilés

            // Player spawn point
            var playerSpawn = new GameObject("PlayerSpawnPoint");
            playerSpawn.transform.position = new Vector3(0, 1, -5);
            playerSpawn.tag = "SpawnPoint";

            // Victim spawn area
            var victimArea = new GameObject("VictimSpawnArea");
            victimArea.transform.position = Vector3.zero;
            var areaCollider = victimArea.AddComponent<BoxCollider>();
            areaCollider.size = new Vector3(30, 1, 30);
            areaCollider.isTrigger = true;

            // Ambulance spawn points
            var ambulanceSpawns = new GameObject("AmbulanceSpawnPoints");
            for (int i = 0; i < 3; i++)
            {
                var spawn = new GameObject($"AmbulanceSpawn_{i}");
                spawn.transform.SetParent(ambulanceSpawns.transform);
                spawn.transform.position = new Vector3(-20 + i * 10, 0, -15);
            }

            // UI Canvas
            var canvas = new GameObject("UICanvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Sauvegarder la scène
            string scenePath = "Assets/Scenes/MainGameScene.unity";
            EnsureDirectoryExists(scenePath);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"Scène principale créée: {scenePath}");
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // Fenêtre de configuration
        private Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("RA-SSE - Configuration du Projet", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Section Structure
            EditorGUILayout.LabelField("1. Structure du Projet", EditorStyles.boldLabel);
            if (GUILayout.Button("Créer Structure de Dossiers"))
            {
                CreateFolderStructure();
            }
            if (GUILayout.Button("Créer Tags et Layers"))
            {
                CreateTagsAndLayers();
            }
            EditorGUILayout.Space(10);

            // Section Prefabs
            EditorGUILayout.LabelField("2. Prefabs", EditorStyles.boldLabel);
            if (GUILayout.Button("Créer Prefab Victime"))
            {
                CreateVictimPrefab();
            }
            if (GUILayout.Button("Créer Prefab Ambulance"))
            {
                CreateAmbulancePrefab();
            }
            if (GUILayout.Button("Créer Markers AR"))
            {
                CreateARMarkerPrefabs();
            }
            EditorGUILayout.Space(10);

            // Section Scènes
            EditorGUILayout.LabelField("3. Scènes", EditorStyles.boldLabel);
            if (GUILayout.Button("Créer Scène Principale"))
            {
                CreateMainScene();
            }
            EditorGUILayout.Space(10);

            // Instructions
            EditorGUILayout.LabelField("Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Cliquez sur 'Créer Structure de Dossiers'\n" +
                "2. Cliquez sur 'Créer Tags et Layers'\n" +
                "3. Créez les Prefabs nécessaires\n" +
                "4. Créez la scène principale\n" +
                "5. Ajoutez les scripts aux GameObjects\n" +
                "6. Configurez le NavMesh\n" +
                "7. Ajoutez les AudioClips",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Vérification
            EditorGUILayout.LabelField("Vérification", EditorStyles.boldLabel);
            
            bool hasVictimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Victims/Victim.prefab") != null;
            bool hasAmbulancePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicles/Ambulance.prefab") != null;
            bool hasMainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainGameScene.unity") != null;

            EditorGUILayout.Toggle("Prefab Victime", hasVictimPrefab);
            EditorGUILayout.Toggle("Prefab Ambulance", hasAmbulancePrefab);
            EditorGUILayout.Toggle("Scène Principale", hasMainScene);

            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// Inspecteur personnalisé pour le GameManager
    /// </summary>
    [CustomEditor(typeof(Core.GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Actions de Debug", EditorStyles.boldLabel);

            Core.GameManager gm = (Core.GameManager)target;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Démarrer Scénario"))
                {
                    gm.StartScenario();
                }

                if (GUILayout.Button("Mettre en Pause"))
                {
                    gm.PauseScenario();
                }

                if (GUILayout.Button("Terminer Scénario"))
                {
                    gm.EndScenario(true);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Victimes détectées: {gm.VictimsDetected}");
                EditorGUILayout.LabelField($"Victimes triées: {gm.VictimsTriaged}");
                EditorGUILayout.LabelField($"Victimes évacuées: {gm.VictimsEvacuated}");
                EditorGUILayout.LabelField($"Temps écoulé: {gm.ElapsedTime:F1}s");
            }
            else
            {
                EditorGUILayout.HelpBox("Démarrez le jeu pour accéder aux actions de debug.", MessageType.Info);
            }
        }
    }

    /// <summary>
    /// Inspecteur personnalisé pour VictimController
    /// </summary>
    [CustomEditor(typeof(Victim.VictimController))]
    public class VictimControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!Application.isPlaying) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Actions de Test", EditorStyles.boldLabel);

            Victim.VictimController vc = (Victim.VictimController)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rouge"))
            {
                vc.SetTriageCategory(Core.StartCategory.Red, true);
            }
            if (GUILayout.Button("Jaune"))
            {
                vc.SetTriageCategory(Core.StartCategory.Yellow, true);
            }
            if (GUILayout.Button("Vert"))
            {
                vc.SetTriageCategory(Core.StartCategory.Green, true);
            }
            if (GUILayout.Button("Noir"))
            {
                vc.SetTriageCategory(Core.StartCategory.Black, true);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Forcer Détérioration"))
            {
                // La méthode DeteriorateCondition est private, donc on ne peut pas l'appeler directement
                Debug.Log("Détérioration forcée (méthode à exposer si nécessaire)");
            }
        }
    }
}
#endif
