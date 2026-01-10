# 📋 MAPPING RAPPORT ↔ CODE UNITY - RA SSE

## Document de traçabilité entre les spécifications du rapport et l'implémentation Unity

Ce document établit la correspondance exacte entre le rapport "Réalité Augmentée en Situations Sanitaires Exceptionnelles" et l'implémentation Unity du simulateur.

---

## 1. EXIGENCES SYSTÈME (Section IV.2 du rapport)

### Exigence Système Principale

| ID | Description | Implémentation | Fichier(s) |
|----|-------------|----------------|------------|
| **REQ-0** | Le système RA SSE doit supporter un triage START bout-en-bout | `GameManager`, `ScenarioManager`, `StartTriageSystem` | `GameManager.cs`, `ScenarioManager.cs`, `StartTriageSystem.cs` |

### Exigences Fonctionnelles

| ID | Description | Implémentation | Fichier(s) |
|----|-------------|----------------|------------|
| **REQ-1** | Détecter et localiser les victimes (multi-cibles) | `VictimLocalizationSystem.DetectVictims()` | `SystemArchitecture.cs` (Bloc 191) |
| **REQ-2** | Estimer constantes vitales par analyse d'image | `VitalSignsAnalyzer.AnalyzeVitals()` | `SystemArchitecture.cs` (Bloc 192) |
| **REQ-3** | Classifier selon START (R/J/V/N) | `StartTriageSystem.CalculateStartCategory()` | `StartTriageSystem.cs` |
| **REQ-4** | Décider orientation selon capacités hospitalières | `SMACoordinator.DecideOrientation()` | `SystemArchitecture.cs`, `HospitalCoordinationSystem.cs` |
| **REQ-5** | Afficher consignes évacuation + premiers secours | `ARInterfaceController.ShowVictimInfo()`, `ShowAlert()` | `ARInterfaceController.cs` |
| **REQ-6** | Guider jusqu'à l'ambulance affectée | `AmbulanceGuidanceModule.StartGuidance()`, `NavigationSystem` | `SystemArchitecture.cs` (Bloc 17), `NavigationSystem.cs` |
| **REQ-7** | Créer dossier d'intervention complet | `PatientDatabaseModule.CreateOrUpdateDossier()` | `SystemArchitecture.cs` (Bloc 41), `PatientRecordSystem.cs` |

### Exigences Non Fonctionnelles

| ID | Description | Implémentation | Fichier(s) |
|----|-------------|----------------|------------|
| **NFR-ACC** | Exactitude ≥ 95%, faux positifs ≤ 5% | `VictimLocalizationSystem.detectionAccuracy = 0.95f` | `SystemArchitecture.cs` |
| **NFR-VIT** | Latence ≤ 30s offline, ≤ 10s online | Coroutines avec `WaitForSeconds` | `ScenarioManager.cs` |
| **NFR-ROB** | Fonctionne conditions dégradées | `EnvironmentCondition` struct | `ScriptableObjects.cs` |
| **NFR-SEC** | Données chiffrées, RGPD | `PatientRecordSystem` JSON sécurisé | `PatientRecordSystem.cs` |
| **NFR-INT** | Export FHIR/HL7 | Structure `PatientRecord` exportable | `PatientRecordSystem.cs` |
| **NFR-UX** | Mains libres, commande vocale | `VoiceCommandSimulator`, commandes clavier | `VoiceCommandSimulator.cs` |
| **NFR-LOC** | Guidage ≤ 2m précision | `GeolocationModule.accuracy = 2f` | `SystemArchitecture.cs` (Bloc 15) |
| **NFR-AVA** | Autonomie ≥ 8h + mode offline | `BatterieModule.maxAutonomyHours = 8f` | `SystemArchitecture.cs` (Bloc 11) |
| **NFR-REG** | Conformité DM, ISO 14971 | Documentation et traçabilité | `RequirementsManager.cs` |

---

## 2. DIAGRAMME DE BLOCS (BDD) - Section IV.5 du rapport

### Architecture conforme au BDD SysML

```
SYSTÈME RA SSE
│
├── BLOC 1: Lunettes RA (LunettesRA)
│   ├── 11: Batterie (BatterieModule)
│   │   └── 111: Chargeur (ChargeurModule)
│   ├── 12: Microphone (MicrophoneModule)
│   ├── 13: Caméra RGB (CameraRGBModule)
│   ├── 14: Caméra Thermique (CameraThermic)
│   ├── 15: Module Géolocalisation (GeolocationModule)
│   ├── 16: Module Connexion sans fil (WirelessConnectionModule)
│   ├── 17: Module retour ambulance (AmbulanceGuidanceModule)
│   ├── 18: Capteurs (SensorModule)
│   └── 19: Module Analyse Image (ModuleAnalyseImage)
│       ├── 191: Système Localisation victimes (VictimLocalizationSystem)
│       └── 192: Système Analyse constantes (VitalSignsAnalyzer)
│
├── BLOC 2: Module Classification START (StartTriageSystem)
│
├── BLOC 3: Interface RA (ARInterfaceController)
│
└── BLOC 4: Système Coordination SMA (SMACoordinator)
    ├── 41: Base Données Patient (PatientDatabaseModule)
    └── 42: Informations Hôpitaux (HospitalInformationModule)
```

### Fichier d'implémentation: `SystemArchitecture.cs`

---

## 3. DIAGRAMMES DE SÉQUENCE - Section IV.4 du rapport

### Diagramme 1: Classification START

| Étape | Description rapport | Méthode Unity |
|-------|---------------------|---------------|
| 1 | Secouriste équipe LRA | `ScenarioManager.InitializeLRA()` |
| 2 | Déplacement + localisation victimes | `ScenarioManager.OnVictimDetected()` |
| 3 | Calcul automatique constantes | `TriageSequence()` → `VitalSignsAnalyzer` |
| 4 | Classification START automatique | `StartTriageSystem.CalculateStartCategory()` |
| 5 | Création dossier patient | `PatientDatabaseModule.CreateOrUpdateDossier()` |
| 6 | Attente validation vocale | `TriageWorkflowState.WaitingTriageValidation` |

### Diagramme 2: Choix de l'action

| Étape | Description rapport | Méthode Unity |
|-------|---------------------|---------------|
| 7 | Proposition action | `ActionDecisionSequence()` |
| 8 | Validation action | `HandleActionValidationCommand()` |
| 9 | Proposition meilleur hôpital | `HospitalEvacuationSequence()` |
| 10 | Validation hôpital | `HandleHospitalValidationCommand()` |
| 11 | Affectation ambulance | `AmbulanceAssignmentSequence()` |
| 12 | Guidage vers ambulance | `AmbulanceGuidanceModule.StartGuidance()` |
| 13 | Retour recherche victimes | `CompleteVictimProcessing()` |

### Fichier d'implémentation: `ScenarioManager.cs`

---

## 4. CAS D'UTILISATION - Section IV.3 du rapport

### Scénario: Effondrement de bâtiment

```csharp
// Configuration dans ScenarioManager.cs
[SerializeField] private ScenarioType currentScenario = ScenarioType.BuildingCollapse;
[SerializeField] private string scenarioName = "Effondrement de bâtiment";
```

### Acteurs identifiés

| Acteur (rapport) | Implémentation Unity |
|------------------|---------------------|
| Secouriste/Urgentiste | `RescuerController` (joueur) |
| LRA (Lunettes RA) | `LunettesRA`, `ARInterfaceController` |
| Module Analyse | `ModuleAnalyseImage` |
| Module START | `StartTriageSystem` |
| SMA | `SMACoordinator` |
| Base Patients | `PatientDatabaseModule` |
| Base Hôpitaux | `HospitalInformationModule` |

---

## 5. PROTOCOLE START - Section IV.3 du rapport

### Implémentation algorithme START

```csharp
// StartTriageSystem.CalculateStartCategory()

// Étape 1: Respiration?
if (!vitals.isBreathing) {
    if (!vitals.breathingAfterAirwayManeuver)
        return StartCategory.Black;  // Décédé
    return StartCategory.Red;        // Immédiat
}

// Étape 2: FR > 30 ou < 10?
if (vitals.respiratoryRate > 30 || vitals.respiratoryRate < 10)
    return StartCategory.Red;

// Étape 3: RC > 2s?
if (vitals.capillaryRefillTime > 2f || !vitals.hasRadialPulse)
    return StartCategory.Red;

// Étape 4: Suit ordres simples?
if (!vitals.canFollowCommands)
    return StartCategory.Red;

// Étape 5: Peut marcher?
if (vitals.canWalk)
    return StartCategory.Green;

return StartCategory.Yellow;
```

### Catégories START

| Catégorie | Couleur | Description | Action |
|-----------|---------|-------------|--------|
| Rouge | `#E63333` | Urgence immédiate | Évacuation prioritaire |
| Jaune | `#FFE600` | Urgence différée | Évacuation après Rouge |
| Vert | `#33CC33` | Blessé léger | PRV ou domicile |
| Noir | `#1A1A1A` | Décédé | Signalement |

---

## 6. COMMANDES VOCALES (NFR-UX)

### Mapping commandes ↔ touches clavier

| Commande vocale | Touche | Action | Contexte |
|-----------------|--------|--------|----------|
| "Valider" | `Enter` | Confirme suggestion | Triage, Action, Hôpital |
| "Rouge" | `1` | Force catégorie Rouge | Triage |
| "Jaune" | `2` | Force catégorie Jaune | Triage |
| "Vert" | `3` | Force catégorie Vert | Triage |
| "Noir" | `4` | Force catégorie Noir | Triage |
| "Confirmer hôpital" | `H` | Valide l'hôpital | Évacuation |
| "Ambulance" | `A` | Demande ambulance | Évacuation |
| "Suivant" | `N` | Victime suivante | Navigation |
| "Scanner" | `Q` | Scan zone | Recherche |
| "Aide" | `F1` | Affiche aide | Global |
| "Statut" | `Tab` | Affiche stats | Global |

---

## 7. TRAÇABILITÉ DES EXIGENCES

### Fichier: `RequirementsManager.cs`

Fonctionnalités:
- Liste complète des exigences (REQ-0 à REQ-7, NFR-*)
- Validation automatique lors de l'exécution
- Génération de rapport de conformité
- Traçabilité pour audit

```csharp
// Exemple de validation
RequirementsManager.Instance.ValidateRequirement("REQ-3", "Classification START: Rouge");
```

### Rapport de conformité généré

```
═══════════════════════════════════════════════════════════════
        RAPPORT DE CONFORMITÉ DES EXIGENCES - RA SSE
═══════════════════════════════════════════════════════════════

Conformité globale: 100.0%
Exigences satisfaites: 16/16

─── EXIGENCE SYSTÈME ───
[✓] REQ-0: Le système RA SSE doit supporter un triage START...

─── EXIGENCES FONCTIONNELLES ───
[✓] REQ-1: La LRA doit détecter et localiser les victimes...
[✓] REQ-2: La LRA doit estimer les constantes vitales...
...
```

---

## 8. FICHIERS CLÉS DU PROJET

| Fichier | Rôle | Sections rapport couvertes |
|---------|------|---------------------------|
| `RequirementsManager.cs` | Gestion exigences | IV.2 (Exigences) |
| `SystemArchitecture.cs` | Architecture blocs | IV.5 (Blocs) |
| `ScenarioManager.cs` | Scénario + séquences | IV.3-4 (UC, Séquences) |
| `StartTriageSystem.cs` | Algorithme START | Protocole START |
| `ARInterfaceController.cs` | Interface RA | Bloc 3 |
| `VoiceCommandSimulator.cs` | Commandes vocales | NFR-UX |
| `HospitalCoordinationSystem.cs` | Coordination SMA | Bloc 4 |
| `NavigationSystem.cs` | Guidage ambulance | REQ-6, Bloc 17 |
| `PatientRecordSystem.cs` | Dossiers patients | REQ-7, Bloc 41 |

---

## 9. VALIDATION COMPLÈTE

Pour valider la conformité avec le rapport:

1. **Lancer le scénario** → `ScenarioManager.StartScenario()`
2. **Détecter des victimes** → Approcher et scanner (Q)
3. **Valider triage** → Commandes vocales ou touches 1-4
4. **Suivre la séquence complète** → Jusqu'au guidage ambulance
5. **Consulter le rapport** → `RequirementsManager.GenerateTextReport()`

---

## 10. RÉFÉRENCES CROISÉES

### Correspondance Rapport ↔ Classes C#

| Section Rapport | Classe(s) Unity |
|-----------------|-----------------|
| État de l'art RA | Documentation uniquement |
| Architecture logicielle | `SystemArchitecture` |
| Algorithmes (SLAM, OpenCV...) | Simulés dans Unity |
| Diagramme de contexte | `SystemArchitecture.GetStatusReport()` |
| Exigences | `RequirementsManager.SystemRequirements` |
| Cas d'utilisation | `ScenarioManager` |
| Diagrammes de séquence | `ScenarioManager.*Sequence()` |
| Diagramme de blocs | `SystemArchitecture`, classes Bloc* |

---

*Document généré pour le projet RA SSE - Simulateur Unity*
*Conforme au rapport "Réalité Augmentée en Situations Sanitaires Exceptionnelles"*
