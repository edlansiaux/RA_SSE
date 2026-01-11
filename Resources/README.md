# Resources Directory

Ce dossier contient les ressources chargées dynamiquement via `Resources.Load()`.

## Structure

```
Resources/
├── Audio/
│   ├── SFX/           - Effets sonores
│   ├── Voice/         - Messages vocaux et alertes
│   └── Ambient/       - Sons d'ambiance
├── Prefabs/           - Prefabs chargés dynamiquement
├── ScriptableObjects/ - Configurations chargées au runtime
├── Sprites/           - Images et icônes UI
└── Data/              - Fichiers JSON et configurations
```

## Conventions de Nommage

- **Audio**: `SFX_NomAction`, `Voice_TypeMessage`, `Amb_Scene`
- **Prefabs**: `Prefab_Type_Nom`
- **Sprites**: `Icon_Nom`, `UI_Element`
- **Data**: `Data_Type_Version`

## Utilisation

```csharp
// Charger un effet sonore
AudioClip clip = Resources.Load<AudioClip>("Audio/SFX/SFX_Alert");

// Charger un prefab
GameObject prefab = Resources.Load<GameObject>("Prefabs/Prefab_Victim");

// Charger une configuration
ScenarioProfileSO scenario = Resources.Load<ScenarioProfileSO>("ScriptableObjects/Scenario_Training");
```

## Notes

- Les ressources dans ce dossier sont incluses dans le build même si non référencées
- Préférer les AssetBundles pour les ressources volumineuses
- Utiliser `Resources.UnloadUnusedAssets()` pour libérer la mémoire
