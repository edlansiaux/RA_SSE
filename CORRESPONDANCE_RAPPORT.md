# Correspondance Rapport RA SSE ↔ Code Unity

Ce document établit la correspondance entre les éléments du rapport "Réalité Augmentée en Situations Sanitaires Exceptionnelles" et les scripts Unity implémentés.

## 1. Correspondance des Blocs Système (Section IV du rapport)

| Bloc ID | Nom du Bloc | Script(s) Unity | Statut |
|---------|-------------|-----------------|--------|
| **1** | Lunettes RA | `GameManager.cs`, `RescuerController.cs` | ✅ |
| **11** | Batterie | `BatteryManager.cs` | ✅ |
| **111** | Chargeur | `BatteryManager.cs` (SetCharging) | ✅ |
| **12** | Microphone | `VoiceCommandSimulator.cs` | ✅ |
| **13** | Caméra RGB | `ImageAnalysisModule.cs` (PerformRGBAnalysis) | ✅ |
| **14** | Caméra Thermique | `ImageAnalysisModule.cs` (PerformThermalAnalysis) | ✅ |
| **15** | Module de Géolocalisation | `NavigationSystem.cs` | ✅ |
| **16** | Module Connexion sans fil | `BatteryManager.cs` (ConnectionState) | ✅ |
| **17** | Module retour vers ambulance | `NavigationSystem.cs` (StartNavigation) | ✅ |
| **18** | Capteurs | `ImageAnalysisModule.cs` | ✅ |
| **19** | Module_Analyse_Image | `ImageAnalysisModule.cs` | ✅ |
| **191** | Système Localisation victimes | `ImageAnalysisModule.cs` (PerformFrameAnalysis) | ✅ |
| **192** | Système Analyse constantes | `ImageAnalysisModule.cs` (AnalyzeVictimVitals) | ✅ |
| **2** | Module_Classification_START | `StartTriageSystem.cs` | ✅ |
| **3** | Interface_RA | `ARInterfaceController.cs`, `HUDController.cs` | ✅ |
| **4** | Système_Coordination_SMA | `SMACoordinationSystem.cs` | ✅ |
| **41** | Base_Donnees_Patient | `PatientRecordSystem.cs`, `SMACoordinationSystem.cs` | ✅ |
| **42** | Informations_Hôpitaux | `HospitalCoordinationSystem.cs`, `SMACoordinationSystem.cs` | ✅ |

## 2. Correspondance des Exigences Fonctionnelles (REQ)

| ID Exigence | Description | Script(s) Implémentant | Méthode(s) Clé(s) |
|-------------|-------------|------------------------|-------------------|
| **REQ-1** | Détection et localisation victimes (multi-cibles) | `ImageAnalysisModule.cs` | `PerformFrameAnalysis()`, `OnMultiTargetDetection` |
| **REQ-2** | Estimation constantes vitales par analyse d'image | `ImageAnalysisModule.cs` | `AnalyzeVictimVitals()`, `PerformRGBAnalysis()`, `PerformThermalAnalysis()` |
| **REQ-3** | Classification START | `StartTriageSystem.cs` | `CalculateStartCategory()` |
| **REQ-4** | Décision orientation selon START et capacités | `SMACoordinationSystem.cs`, `HospitalCoordinationSystem.cs` | `DetermineOptimalHospital()`, `GetBestHospital()` |
| **REQ-5** | Affichage consignes évacuation + premiers secours | `FirstAidGuidanceModule.cs`, `ARInterfaceController.cs` | `GetFullGuidanceText()`, `GetProtocolForCategory()` |
| **REQ-6** | Guidage vers ambulance affectée (AR) | `NavigationSystem.cs`, `ARInterfaceController.cs` | `StartNavigation()`, `UpdateNavigationCoroutine()` |
| **REQ-7** | Création dossier d'intervention complet | `PatientRecordSystem.cs`, `FHIRExportModule.cs` | `CreateRecord()`, `ExportFullIntervention()` |

## 3. Correspondance des Exigences Non Fonctionnelles (NFR)

| ID | Description | Script(s) | Implémentation | Vérification |
|----|-------------|-----------|----------------|--------------|
| **NFR-ACC** | Exactitude ≥95%, faux positifs ≤5% | `ImageAnalysisModule.cs`, `RequirementsComplianceMonitor.cs` | `detectionAccuracy`, `falsePositiveRate` | `EvaluateRequirement()` |
| **NFR-VIT** | Latence ≤30s offline, ≤10s online | `ImageAnalysisModule.cs`, `RequirementsComplianceMonitor.cs` | `analysisLatency`, `AnalysisMetrics` | `RecordTriageLatency()` |
| **NFR-ROB** | Fonctionne faible luminosité, fumée, pluie | `ImageAnalysisModule.cs` | `EnvironmentConditions`, `CalculateEnvironmentFactor()` | Classe `LightLevel` |
| **NFR-SEC** | Données chiffrées, journalisation, RGPD | `FHIRExportModule.cs`, `BatteryManager.cs` | `encryptionEnabled`, `auditLoggingEnabled` | `LogExport()` |
| **NFR-INT** | Export FHIR/HL7 | `FHIRExportModule.cs` | `ExportPatientFHIR()`, `ExportPatientHL7()` | Standards FHIR R4, HL7 v2.5 |
| **NFR-UX** | Mains libres, commande vocale | `VoiceCommandSimulator.cs`, `RescuerController.cs` | Commandes clavier simulant vocal | `ProcessVoiceInput()` |
| **NFR-LOC** | Guidage ≤2m précision | `NavigationSystem.cs` | NavMesh pathfinding | `DistanceToDestination` |
| **NFR-AVA** | Autonomie ≥8h + mode offline | `BatteryManager.cs`, `OfflineModeManager.cs` | `targetAutonomy`, `IsOfflineMode` | `MeetsAutonomyRequirement()` |
| **NFR-REG** | Conformité DM, ISO 14971, IEC 62304 | `RequirementsComplianceMonitor.cs` | Documentation | Non testable automatiquement |

## 4. Correspondance Diagramme de Séquence

### Séquence 1: Classification START (diagramme rapport)

```
Utilisateur → Lunettes RA → Module Analyse Image → Module Classification START → Interface RA
```

**Implémentation Unity:**
```csharp
// RescuerController.cs
InteractWithVictim(victim) 
    → ARInterfaceController.ShowVictimInfo(victim)
    → ImageAnalysisModule.AnalyzeVictimVitals(victim)
    → StartTriageSystem.CalculateStartCategory(vitals)
    → ARInterfaceController.ShowTriageResult(category)
```

### Séquence 2: Choix de l'action à réaliser (diagramme rapport)

```
Système → SMA → Base Données → Hôpitaux → Ambulances → Interface RA
```

**Implémentation Unity:**
```csharp
// SMACoordinationSystem.cs
CoordinateEvacuation(victim)
    → PatientDatabase.RegisterPatient()
    → DetermineOptimalHospital()
    → HospitalInfoSystem.ReserveBed()
    → AmbulanceManager.AssignAmbulance()
    → SendHospitalNotification()
    → NavigationSystem.StartNavigation()
```

## 5. Structure des Fichiers par Module

```
Scripts/
├── Core/                          # Bloc 1, 2
│   ├── GameManager.cs
│   ├── StartTriageSystem.cs       # REQ-3
│   ├── RescuerController.cs
│   ├── VoiceCommandSimulator.cs   # NFR-UX
│   └── EnumDefinitions.cs
│
├── ImageAnalysis/                 # Bloc 19, 191, 192
│   └── ImageAnalysisModule.cs     # REQ-1, REQ-2, NFR-ACC, NFR-VIT, NFR-ROB
│
├── Hardware/                      # Bloc 11, 16
│   └── BatteryManager.cs          # NFR-AVA (autonomie + offline)
│
├── Medical/                       # REQ-5
│   └── FirstAidGuidanceModule.cs
│
├── Interoperability/              # NFR-INT, NFR-SEC
│   └── FHIRExportModule.cs
│
├── Coordination/                  # Bloc 4, 41, 42
│   └── SMACoordinationSystem.cs   # REQ-4
│
├── Compliance/                    # Vérification NFR
│   └── RequirementsComplianceMonitor.cs
│
├── Hospital/                      # Bloc 42
│   ├── HospitalCoordinationSystem.cs
│   └── AmbulanceManager.cs
│
├── Navigation/                    # Bloc 15, 17
│   └── NavigationSystem.cs        # REQ-6, NFR-LOC
│
├── Data/                          # Bloc 41
│   ├── PatientRecordSystem.cs     # REQ-7
│   └── ScriptableObjects.cs
│
├── AR/                            # Bloc 3
│   └── ARInterfaceController.cs
│
├── UI/
│   ├── HUDController.cs
│   ├── MainMenuController.cs
│   └── PauseAndResultsUI.cs
│
├── Victim/
│   ├── VictimController.cs
│   └── VictimSpawner.cs
│
├── Training/
│   └── TutorialManager.cs
│
└── Audio/
    └── AudioManager.cs
```

## 6. Flux de Données Principal (conforme au diagramme contexte)

```
┌─────────────────────────────────────────────────────────────────┐
│                         LUNETTES RA                              │
│  ┌──────────┐    ┌────────────────┐    ┌───────────────────┐   │
│  │ Caméras  │───→│ ImageAnalysis  │───→│ StartTriageSystem │   │
│  │ RGB+Therm│    │ Module         │    │                   │   │
│  └──────────┘    └────────────────┘    └─────────┬─────────┘   │
│                         │                         │              │
│                         ▼                         ▼              │
│              ┌─────────────────┐      ┌───────────────────┐    │
│              │ ARInterface     │      │ FirstAidGuidance  │    │
│              │ Controller      │←─────│ Module            │    │
│              └────────┬────────┘      └───────────────────┘    │
│                       │                                         │
└───────────────────────┼─────────────────────────────────────────┘
                        │
                        ▼
        ┌───────────────────────────────────────┐
        │      SYSTÈME COORDINATION SMA          │
        │  ┌─────────────┐  ┌────────────────┐  │
        │  │ Patient     │  │ Hospital       │  │
        │  │ Database    │  │ InfoSystem     │  │
        │  └─────────────┘  └────────────────┘  │
        └───────────────────┬───────────────────┘
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
        ┌──────────┐ ┌───────────┐ ┌────────────┐
        │ FHIR/HL7 │ │ Hôpitaux  │ │ Ambulances │
        │ Export   │ │           │ │            │
        └──────────┘ └───────────┘ └────────────┘
```

## 7. Validation de Conformité

Pour vérifier la conformité aux exigences du rapport, utiliser:

```csharp
// Dans Unity, accéder au rapport de conformité:
RequirementsComplianceMonitor.Instance.GenerateComplianceReport()
```

Ce rapport affiche en temps réel:
- État de chaque exigence (REQ-1 à REQ-7, NFR-*)
- Métriques de performance (exactitude, latence, autonomie)
- Statut de conformité global

## 8. Tests Recommandés

| Test | Exigence(s) | Commande |
|------|-------------|----------|
| Détection multi-cibles | REQ-1, NFR-ACC | Spawner 10+ victimes, vérifier détection |
| Latence triage | NFR-VIT | Mesurer temps entre interaction et classification |
| Mode offline | NFR-AVA | `BatteryManager.Instance.ForceOfflineMode()` |
| Export FHIR | NFR-INT | `FHIRExportModule.Instance.ExportPatientFHIR(victim)` |
| Guidage précision | NFR-LOC | Vérifier distance finale vs destination |
| Conditions dégradées | NFR-ROB | Modifier `EnvironmentConditions.lightLevel` |

---

*Document généré pour le projet RA SSE - Janvier 2026*
*Conforme au rapport "Réalité Augmentée en Situations Sanitaires Exceptionnelles"*
