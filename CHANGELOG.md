# Changelog - RA-SSE Simulateur de Triage Médical

Toutes les modifications notables de ce projet sont documentées dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/lang/fr/).

## [1.0.0] - 2026-01-10

### Ajouté

#### Core
- `GameManager.cs` - Gestionnaire principal du jeu avec machine à états
- `StartTriageSystem.cs` - Système de triage START complet (REQ-3)
- `RescuerController.cs` - Contrôleur du joueur/secouriste
- `ScenarioManager.cs` - Gestionnaire des scénarios SSE
- `VoiceCommandSimulator.cs` - Simulation des commandes vocales (NFR-UX)
- `RequirementsManager.cs` - Validation des exigences en temps réel
- `SystemArchitecture.cs` - Documentation de l'architecture SysML

#### Victimes
- `VictimController.cs` - Contrôleur des victimes avec états physiologiques
- `VictimSpawner.cs` - Générateur de victimes selon les scénarios

#### Analyse d'Image
- `ImageAnalysisModule.cs` - Module d'analyse RGB + thermique (Blocs 191/192)
  - Détection de victimes ≥95% (REQ-1)
  - Analyse des constantes vitales (REQ-2)

#### Interface AR
- `ARInterfaceController.cs` - Contrôleur de l'interface AR RealWear
- `HUDController.cs` - Affichage tête haute (Bloc 42)

#### Coordination
- `HospitalCoordinationSystem.cs` - Coordination hospitalière temps réel (REQ-6)
- `AmbulanceManager.cs` - Gestion des ambulances VSAV
- `SMACoordinationSystem.cs` - Système Multi-Agents (Blocs 4/41/42)

#### Navigation
- `NavigationSystem.cs` - Guidage vers victimes ≤2m (REQ-4)

#### Médical
- `FirstAidGuidanceModule.cs` - Protocoles premiers secours (REQ-5)

#### Interopérabilité
- `FHIRExportModule.cs` - Export FHIR R4 + HL7 v2.5 (REQ-7, NFR-INT)

#### Hardware
- `BatteryManager.cs` - Gestion autonomie 8h (NFR-DIS)
- `OfflineModeManager.cs` - Mode dégradé hors-ligne

#### Conformité
- `RequirementsComplianceMonitor.cs` - Validation 16 exigences NFR

#### Zones
- `DangerZoneController.cs` - Gestion des zones dangereuses
- `TriageZoneController.cs` - Zones de triage START
- `PMAController.cs` - Poste Médical Avancé

#### UI
- `UIManager.cs` - Gestionnaire UI global
- `MainMenuController.cs` - Menu principal
- `PauseAndResultsUI.cs` - Écrans pause et résultats

#### Training
- `TutorialManager.cs` - Système de formation

#### Data (ScriptableObjects)
- `VictimProfileSO.cs` - Profils de victimes
- `ScenarioProfileSO.cs` - Configuration des scénarios
- `HospitalProfileSO.cs` - Profils d'hôpitaux
- `EquipmentSO.cs` - Équipements médicaux
- `MedicalKitSO.cs` - Kits médicaux (PSE1, PSE2, DSA, Trauma)
- `TriageProtocolSO.cs` - Protocole START
- `SystemSettingsSO.cs` - Configuration NFR

### Scènes
- `MainMenu.unity` - Menu principal
- `TrainingScene.unity` - Scène de formation
- `Scenario_IndustrialExplosion.unity` - Explosion industrielle
- `Scenario_TrainAccident.unity` - Accident ferroviaire
- `Scenario_BuildingCollapse.unity` - Effondrement de bâtiment

### Prefabs
- AR: `AR_VictimOverlay`, `AR_NavigationMarker`
- Ambulances: `Ambulance_VSAV`
- Environment: `Env_DangerZone`, `TriageZone_Marker`, `PMA_PosteMedicalAvance`
- Hospitals: `Hospital_Generic`, `Hospital_TraumaCenter`
- Rescuers: `Rescuer_Player`
- UI: `Canvas_HUD_AR`, `Canvas_MainMenu`
- Victims: `Victim_Template`

### ScriptableObjects
- 5 Equipment (Garrot, Collier, Attelle, BAVU, Couverture)
- 3 Hospitals (CHU, CH, Centre Brûlés)
- 5 MedicalKits (PSE1, PSE2, DSA, Hemorrhage, Trauma)
- 4 Scenarios (Tutorial, Industrial, Train, Building)
- 1 Settings (SystemSettings_Default)
- 1 TriageProtocol (START)
- 8 VictimProfiles (4 catégories x 2 variantes)

### Configuration
- Configuration RealWear Navigator 520 (NFR-VIT)
- Sécurité AES-256 + TLS 1.3 (NFR-SEC)
- Conformité RGPD, ISO 14971, IEC 62304, MDR 2017/745 (NFR-REG)

### Documentation
- `README.md` - Guide de démarrage
- `GUIDE_INSTALLATION.md` - Instructions d'installation
- `CORRESPONDANCE_RAPPORT.md` - Mapping rapport académique
- `MAPPING_RAPPORT_CODE.md` - Correspondance code/spécifications
- `CHANGELOG.md` - Ce fichier

---

## [Unreleased]

### À venir
- Intégration SDK RealWear réel
- Tests unitaires automatisés
- Mode multijoueur coopératif
- Analytics et métriques de formation
- Export PDF des rapports de mission
- Localisation multilingue

---

## Conformité aux Exigences

### Exigences Fonctionnelles (REQ-*)
| ID | Description | Statut |
|----|-------------|--------|
| REQ-1 | Détection victimes ≥95% | ✅ Implémenté |
| REQ-2 | Constantes vitales | ✅ Implémenté |
| REQ-3 | Classification START | ✅ Implémenté |
| REQ-4 | Guidage ≤2m | ✅ Implémenté |
| REQ-5 | Protocoles premiers secours | ✅ Implémenté |
| REQ-6 | Coordination temps réel | ✅ Implémenté |
| REQ-7 | Export FHIR/HL7 | ✅ Implémenté |

### Exigences Non-Fonctionnelles (NFR-*)
| ID | Description | Statut |
|----|-------------|--------|
| NFR-ACC | Précision ≥95% | ✅ Implémenté |
| NFR-VIT | Latence ≤30s offline | ✅ Implémenté |
| NFR-DIS | Autonomie 8h | ✅ Implémenté |
| NFR-LOC | Localisation ≤2m | ✅ Implémenté |
| NFR-ROB | Conditions dégradées | ✅ Implémenté |
| NFR-SEC | AES-256 + TLS 1.3 | ✅ Implémenté |
| NFR-INT | FHIR R4 + HL7 v2.5 | ✅ Implémenté |
| NFR-UX | Mains libres | ✅ Implémenté |
| NFR-REG | ISO/IEC/MDR | ✅ Implémenté |

---

## Auteurs

- Équipe RA-SSE - Développement initial

## Licence

Ce projet est sous licence propriétaire. Voir le fichier LICENSE pour plus de détails.
