# 📁 Arborescence du Projet Unity RA-SSE

```
UnityProject_RA_SSE/
│
├── 📄 .gitignore                          # Configuration Git
├── 📄 CHANGELOG.md                        # Historique des versions
├── 📄 CORRESPONDANCE_RAPPORT.md           # Mapping rapport académique
├── 📄 DEPLOYMENT.md                       # Guide de déploiement
├── 📄 GUIDE_INSTALLATION.md               # Instructions d'installation
├── 📄 LICENSE                             # Licence MIT
├── 📄 MAPPING_RAPPORT_CODE.md             # Correspondance code/spécifications
├── 📄 QUICK_START.md                      # Guide démarrage rapide
├── 📄 README.md                           # Documentation principale
│
├── 📁 Materials/                          # Matériaux Unity (5 fichiers)
│   ├── Mat_DangerZone.mat                 # Matériau zones de danger
│   ├── Mat_TriageZone_Black.mat           # Zone noire (décédés)
│   ├── Mat_TriageZone_Green.mat           # Zone verte (impliqués)
│   ├── Mat_TriageZone_Red.mat             # Zone rouge (urgences absolues)
│   └── Mat_TriageZone_Yellow.mat          # Zone jaune (urgences relatives)
│
├── 📁 Packages/                           # Dépendances Unity
│   └── manifest.json                      # Liste des packages requis
│
├── 📁 Prefabs/                            # Prefabs Unity (12 fichiers)
│   ├── 📁 AR/
│   │   ├── AR_NavigationMarker.prefab     # Marqueur de navigation AR
│   │   └── AR_VictimOverlay.prefab        # Overlay AR pour victimes
│   ├── 📁 Ambulances/
│   │   └── Ambulance_VSAV.prefab          # Véhicule VSAV
│   ├── 📁 Environment/
│   │   ├── Env_DangerZone.prefab          # Zone de danger
│   │   ├── PMA_PosteMedicalAvance.prefab  # Poste Médical Avancé
│   │   └── TriageZone_Marker.prefab       # Marqueur zone de triage
│   ├── 📁 Hospitals/
│   │   ├── Hospital_Generic.prefab        # Hôpital générique
│   │   └── Hospital_TraumaCenter.prefab   # Centre de traumatologie
│   ├── 📁 Rescuers/
│   │   └── Rescuer_Player.prefab          # Joueur/Secouriste
│   ├── 📁 UI/
│   │   ├── Canvas_HUD_AR.prefab           # Interface HUD AR
│   │   └── Canvas_MainMenu.prefab         # Menu principal
│   └── 📁 Victims/
│       └── Victim_Template.prefab         # Template de victime
│
├── 📁 ProjectSettings/                    # Configuration Unity (7 fichiers)
│   ├── EditorBuildSettings.asset          # Scènes de build
│   ├── InputManager.asset                 # Configuration des contrôles
│   ├── Physics2DSettings.asset            # Paramètres physique 2D
│   ├── ProjectSettings.asset              # Paramètres généraux du projet
│   ├── QualitySettings.asset              # Niveaux de qualité
│   ├── TagManager.asset                   # Tags et Layers
│   └── TimeManager.asset                  # Paramètres de temps
│
├── 📁 Resources/                          # Assets chargeables dynamiquement
│   └── README.md                          # Documentation Resources
│
├── 📁 Scenes/                             # Scènes Unity (5 fichiers)
│   ├── MainMenu.unity                     # Menu principal
│   ├── Scenario_BuildingCollapse.unity    # Scénario: Effondrement
│   ├── Scenario_IndustrialExplosion.unity # Scénario: Explosion industrielle
│   ├── Scenario_TrainAccident.unity       # Scénario: Accident ferroviaire
│   └── TrainingScene.unity                # Scène de formation/tutoriel
│
├── 📁 ScriptableObjects/                  # Données configurables (27 fichiers)
│   ├── 📁 Equipment/                      # Équipements médicaux (5)
│   │   ├── Equipment_Attelle.asset        # Attelle modelable
│   │   ├── Equipment_BAVU.asset           # Ballon auto-remplisseur
│   │   ├── Equipment_CollierCervical.asset# Collier cervical
│   │   ├── Equipment_CouvertureSurvie.asset# Couverture de survie
│   │   └── Equipment_Garrot.asset         # Garrot tourniquet
│   ├── 📁 Hospitals/                      # Profils d'hôpitaux (3)
│   │   ├── Hospital_CHU_Metropole.asset   # CHU Métropole
│   │   ├── Hospital_CH_SaintVincent.asset # CH Saint-Vincent
│   │   └── Hospital_CentreBrules.asset    # Centre des Brûlés
│   ├── 📁 MedicalKits/                    # Kits médicaux (5)
│   │   ├── MedicalKit_DSA.asset           # Défibrillateur
│   │   ├── MedicalKit_Hemorrhage.asset    # Kit hémorragie
│   │   ├── MedicalKit_PSE1.asset          # Kit PSE1
│   │   ├── MedicalKit_PSE2.asset          # Kit PSE2
│   │   └── MedicalKit_Trauma.asset        # Kit trauma avancé
│   ├── 📁 Scenarios/                      # Configurations scénarios (4)
│   │   ├── Scenario_BuildingCollapse.asset# Config effondrement
│   │   ├── Scenario_IndustrialExplosion.asset# Config explosion
│   │   ├── Scenario_TrainAccident.asset   # Config accident train
│   │   └── Scenario_Tutorial.asset        # Config tutoriel
│   ├── 📁 Settings/                       # Paramètres système (1)
│   │   └── SystemSettings_Default.asset   # Config par défaut
│   ├── 📁 TriageProtocols/                # Protocoles de triage (1)
│   │   └── TriageProtocol_START.asset     # Protocole START
│   └── 📁 VictimProfiles/                 # Profils de victimes (8)
│       ├── VictimProfile_Black_Deceased.asset    # Noir - Décédé
│       ├── VictimProfile_Green_Contusions.asset  # Vert - Contusions
│       ├── VictimProfile_Green_Minor.asset       # Vert - Blessures mineures
│       ├── VictimProfile_Red_HeadTrauma.asset    # Rouge - Trauma crânien
│       ├── VictimProfile_Red_Hemorrhage.asset    # Rouge - Hémorragie
│       ├── VictimProfile_Red_Respiratory.asset   # Rouge - Détresse respi
│       ├── VictimProfile_Yellow_Burns.asset      # Jaune - Brûlures
│       └── VictimProfile_Yellow_Fracture.asset   # Jaune - Fracture
│
├── 📁 Scripts/                            # Code source C# (48 fichiers)
│   ├── 📄 RASSE.asmdef                    # Assembly Definition principal
│   │
│   ├── 📁 AR/                             # Réalité Augmentée (1)
│   │   └── ARInterfaceController.cs       # Contrôleur interface AR
│   │
│   ├── 📁 Audio/                          # Gestion audio (1)
│   │   └── AudioManager.cs                # Gestionnaire audio
│   │
│   ├── 📁 Compliance/                     # Conformité réglementaire (1)
│   │   └── RequirementsComplianceMonitor.cs# Moniteur de conformité
│   │
│   ├── 📁 Coordination/                   # Coordination multi-agents (1)
│   │   └── SMACoordinationSystem.cs       # Système SMA
│   │
│   ├── 📁 Core/                           # Noyau du système (14)
│   │   ├── Bootstrapper.cs                # Initialisation automatique
│   │   ├── EnumDefinitions.cs             # Définitions des énumérations
│   │   ├── EventManager.cs                # Gestionnaire d'événements
│   │   ├── GameConstants.cs               # Constantes du jeu
│   │   ├── GameManager.cs                 # Gestionnaire principal
│   │   ├── GameManager_original.cs        # Backup GameManager
│   │   ├── IInteractable.cs               # Interface d'interaction
│   │   ├── RequirementsManager.cs         # Gestionnaire des exigences
│   │   ├── RescuerController.cs           # Contrôleur du secouriste
│   │   ├── ScenarioManager.cs             # Gestionnaire de scénarios
│   │   ├── SceneLoader.cs                 # Chargeur de scènes
│   │   ├── Singleton.cs                   # Pattern Singleton
│   │   ├── StartTriageSystem.cs           # Système de triage START
│   │   ├── SystemArchitecture.cs          # Documentation architecture
│   │   └── VoiceCommandSimulator.cs       # Simulateur commandes vocales
│   │
│   ├── 📁 Data/                           # ScriptableObjects (9)
│   │   ├── EquipmentSO.cs                 # Définition équipements
│   │   ├── HospitalProfileSO.cs           # Définition hôpitaux
│   │   ├── MedicalKitSO.cs                # Définition kits médicaux
│   │   ├── PatientRecordSystem.cs         # Système dossiers patients
│   │   ├── ScenarioProfileSO.cs           # Définition scénarios
│   │   ├── ScriptableObjects.cs           # Base ScriptableObjects
│   │   ├── SystemSettingsSO.cs            # Paramètres système
│   │   ├── TriageProtocolSO.cs            # Protocoles de triage
│   │   └── VictimProfileSO.cs             # Profils de victimes
│   │
│   ├── 📁 Editor/                         # Outils éditeur (3)
│   │   ├── BuildConfiguration.cs          # Configuration de build
│   │   ├── RASSE.Editor.asmdef            # Assembly Definition éditeur
│   │   └── RASSEEditorTools.cs            # Outils personnalisés
│   │
│   ├── 📁 Environment/                    # Environnement (1)
│   │   └── DangerZoneController.cs        # Contrôleur zones danger
│   │
│   ├── 📁 Hardware/                       # Matériel (1)
│   │   └── BatteryManager.cs              # Gestion batterie
│   │
│   ├── 📁 Hospital/                       # Gestion hospitalière (2)
│   │   ├── AmbulanceManager.cs            # Gestionnaire ambulances
│   │   └── HospitalCoordinationSystem.cs  # Coordination hospitalière
│   │
│   ├── 📁 ImageAnalysis/                  # Analyse d'image (1)
│   │   └── ImageAnalysisModule.cs         # Module analyse RGB/thermique
│   │
│   ├── 📁 Interoperability/               # Interopérabilité (1)
│   │   └── FHIRExportModule.cs            # Export FHIR/HL7
│   │
│   ├── 📁 Medical/                        # Protocoles médicaux (1)
│   │   └── FirstAidGuidanceModule.cs      # Guidage premiers secours
│   │
│   ├── 📁 Navigation/                     # Navigation (1)
│   │   └── NavigationSystem.cs            # Système de navigation
│   │
│   ├── 📁 Training/                       # Formation (1)
│   │   └── TutorialManager.cs             # Gestionnaire tutoriel
│   │
│   ├── 📁 UI/                             # Interface utilisateur (4)
│   │   ├── HUDController.cs               # Contrôleur HUD
│   │   ├── MainMenuController.cs          # Contrôleur menu principal
│   │   ├── PauseAndResultsUI.cs           # UI pause et résultats
│   │   └── UIManager.cs                   # Gestionnaire UI
│   │
│   ├── 📁 Vehicles/                       # Véhicules (1)
│   │   └── AmbulanceController.cs         # Contrôleur ambulance
│   │
│   ├── 📁 Victim/                         # Gestion victimes (2)
│   │   ├── VictimController.cs            # Contrôleur de victime
│   │   └── VictimSpawner.cs               # Générateur de victimes
│   │
│   └── 📁 Zones/                          # Gestion des zones (2)
│       ├── PMAController.cs               # Contrôleur PMA
│       └── TriageZoneController.cs        # Contrôleur zones triage
│
└── 📁 Tests/                              # Tests unitaires (2 fichiers)
    └── 📁 Editor/
        ├── RASSE.Tests.Editor.asmdef      # Assembly Definition tests
        └── TriageSystemTests.cs           # Tests système de triage
```

## 📊 Résumé par Catégorie

| Catégorie | Fichiers | Description |
|-----------|----------|-------------|
| **Documentation** | 9 | README, guides, changelog |
| **Scripts C#** | 48 | Code source du simulateur |
| **Prefabs** | 12 | Objets Unity préfabriqués |
| **ScriptableObjects** | 27 | Données configurables |
| **Scènes** | 5 | Niveaux du jeu |
| **Materials** | 5 | Matériaux visuels |
| **ProjectSettings** | 7 | Configuration Unity |
| **Tests** | 2 | Tests unitaires |
| **Packages** | 1 | Dépendances |
| **Resources** | 1 | Assets dynamiques |
| **TOTAL** | **119** | Fichiers |

## 🏗️ Architecture des Scripts

```
Scripts/ (48 fichiers)
├── Core/          (14) - Noyau: GameManager, Triage, Events
├── Data/          (9)  - ScriptableObjects definitions
├── UI/            (4)  - Interface utilisateur
├── Editor/        (3)  - Outils Unity Editor
├── Hospital/      (2)  - Coordination hospitalière
├── Victim/        (2)  - Gestion des victimes
├── Zones/         (2)  - Zones de triage et PMA
├── AR/            (1)  - Réalité augmentée
├── Audio/         (1)  - Gestion audio
├── Compliance/    (1)  - Conformité réglementaire
├── Coordination/  (1)  - Système multi-agents
├── Environment/   (1)  - Zones de danger
├── Hardware/      (1)  - Gestion matériel
├── ImageAnalysis/ (1)  - Analyse d'image
├── Interoperability/(1)- Export FHIR/HL7
├── Medical/       (1)  - Protocoles médicaux
├── Navigation/    (1)  - Guidage
├── Training/      (1)  - Formation
└── Vehicles/      (1)  - Ambulances
```
