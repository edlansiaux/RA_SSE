# Guide d'Installation - RA-SSE Simulateur de Triage

## 📋 Checklist de Configuration

### Étape 1: Création du Projet Unity
- [ ] Ouvrir Unity Hub
- [ ] Créer un nouveau projet 3D (Unity 2022.3 LTS recommandé)
- [ ] Nommer le projet "RA_SSE_Simulator"

### Étape 2: Import des Packages
- [ ] Window → Package Manager
- [ ] Installer **TextMeshPro** (Essentials)
- [ ] Installer **AI Navigation** (pour NavMesh)
- [ ] Optionnel: Installer **Cinemachine** (pour caméra avancée)

### Étape 3: Import des Scripts
- [ ] Copier le dossier `Scripts/` dans `Assets/Scripts/`
- [ ] Attendre la compilation
- [ ] Vérifier qu'il n'y a pas d'erreurs dans la Console

### Étape 4: Utiliser les Outils d'Éditeur
- [ ] Menu **RA-SSE → Ouvrir Configuration**
- [ ] Cliquer sur "Créer Structure de Dossiers"
- [ ] Cliquer sur "Créer Tags et Layers"
- [ ] Cliquer sur "Créer Prefab Victime"
- [ ] Cliquer sur "Créer Prefab Ambulance"
- [ ] Cliquer sur "Créer Markers AR"

### Étape 5: Configuration de la Scène Principale
- [ ] Menu **RA-SSE → Créer Scène Principale**
- [ ] Ouvrir la scène `Assets/Scenes/MainGameScene.unity`

### Étape 6: Configuration du GameManager
1. Sélectionner le GameObject "GameManager"
2. Ajouter les composants:
   - [ ] `GameManager`
   - [ ] `VictimSpawner`
   - [ ] `HospitalCoordinationSystem`
   - [ ] `AmbulanceManager`
   - [ ] `PatientRecordSystem`
   - [ ] `StartTriageSystem`

3. Configurer les références:
   - [ ] Assigner le prefab Victim au VictimSpawner
   - [ ] Assigner le prefab Ambulance au AmbulanceManager

### Étape 7: Configuration du Joueur
1. Créer un GameObject vide nommé "Player"
2. Ajouter les composants:
   - [ ] `CharacterController`
   - [ ] `RescuerController`
3. Configuration:
   - [ ] Tag: "Rescuer"
   - [ ] Position: (0, 1, -5)
4. Créer une Camera comme enfant:
   - [ ] Position locale: (0, 0.6, 0)
   - [ ] Tag: MainCamera

### Étape 8: Configuration de l'UI
1. Sélectionner "UICanvas"
2. Ajouter les composants:
   - [ ] `ARInterfaceController`
   - [ ] `HUDController`
   - [ ] `PauseMenuController`
3. Créer la hiérarchie UI (voir section UI ci-dessous)

### Étape 9: Configuration du NavMesh
1. Sélectionner le sol (Floor/Ground)
2. Window → AI → Navigation
3. Onglet "Bake":
   - [ ] Agent Radius: 0.5
   - [ ] Agent Height: 2
   - [ ] Max Slope: 45
4. [ ] Cliquer sur "Bake"

### Étape 10: Configuration Audio
1. Créer un GameObject "AudioManager"
2. Ajouter le composant `AudioManager`
3. Optionnel: Assigner des AudioClips

---

## 🖼️ Structure UI Détaillée

### Canvas Principal (UICanvas)
```
UICanvas
├── ARPanel
│   ├── VictimInfoPanel
│   │   ├── PatientIDText (TMP)
│   │   ├── NameText (TMP)
│   │   ├── AgeText (TMP)
│   │   ├── VitalsContainer
│   │   │   ├── RespirationText (TMP)
│   │   │   ├── HeartRateText (TMP)
│   │   │   ├── SpO2Text (TMP)
│   │   │   └── GCSText (TMP)
│   │   └── InjuryText (TMP)
│   │
│   ├── TriagePanel
│   │   ├── SuggestedCategoryText (TMP)
│   │   ├── SuggestedCategoryImage
│   │   └── ButtonsContainer
│   │       ├── ValidateButton
│   │       ├── RedButton
│   │       ├── YellowButton
│   │       ├── GreenButton
│   │       └── BlackButton
│   │
│   ├── NavigationPanel
│   │   ├── DistanceText (TMP)
│   │   ├── DirectionText (TMP)
│   │   ├── ETAText (TMP)
│   │   └── DirectionArrow (Image)
│   │
│   └── StatsPanel
│       ├── TimerText (TMP)
│       ├── DetectedText (TMP)
│       ├── TriagedText (TMP)
│       └── CategoryCounters
│           ├── RedCount (TMP)
│           ├── YellowCount (TMP)
│           ├── GreenCount (TMP)
│           └── BlackCount (TMP)
│
├── HUDPanel
│   ├── Crosshair (Image)
│   ├── CompassBar
│   ├── Minimap (RawImage)
│   └── NotificationContainer
│
├── PauseMenu
│   ├── PausePanel
│   │   ├── ResumeButton
│   │   ├── SettingsButton
│   │   ├── RestartButton
│   │   └── QuitButton
│   └── ConfirmPanel
│       ├── MessageText (TMP)
│       ├── YesButton
│       └── NoButton
│
└── ResultsPanel
    ├── TitleText (TMP)
    ├── ScoreText (TMP)
    ├── GradeText (TMP)
    ├── StatsContainer
    └── ButtonsContainer
```

---

## 🎮 Test du Projet

### Test Basique
1. [ ] Lancer le jeu (Play)
2. [ ] Vérifier que le joueur peut se déplacer (WASD)
3. [ ] Vérifier que la caméra suit la souris
4. [ ] Vérifier qu'il n'y a pas d'erreurs dans la Console

### Test du Triage
1. [ ] Approcher une victime (si spawn automatique activé)
2. [ ] Appuyer sur E pour interagir
3. [ ] Vérifier l'affichage des infos AR
4. [ ] Tester les touches de triage (1, 2, 3, 4)
5. [ ] Vérifier le changement de couleur de la victime

### Test de Navigation
1. [ ] Activer la navigation vers une ambulance
2. [ ] Vérifier l'affichage du chemin
3. [ ] Vérifier les indicateurs de direction

---

## 🔧 Résolution des Problèmes Courants

### Erreur: "Namespace not found"
- Vérifier que tous les scripts sont dans le bon dossier
- Vérifier les directives `using` en haut des scripts

### Erreur: "NullReferenceException"
- Vérifier que toutes les références sont assignées dans l'Inspector
- Utiliser les logs Debug pour identifier le composant manquant

### Le joueur traverse le sol
- Vérifier que le CharacterController est configuré
- Vérifier que le sol a un Collider

### NavMesh ne fonctionne pas
- Vérifier que le NavMesh est "baked"
- Vérifier que le sol est marqué comme "Walkable"
- Vérifier que NavigationSystem a une référence NavMeshAgent

### UI n'apparaît pas
- Vérifier que le Canvas est en mode "Screen Space - Overlay"
- Vérifier que les panels sont actifs
- Vérifier l'ordre des layers UI

---

## 📁 Fichiers de Configuration Recommandés

### Input Manager (Edit → Project Settings → Input Manager)
Les axes par défaut devraient fonctionner, mais vérifier:
- Horizontal (A/D)
- Vertical (W/S)
- Mouse X
- Mouse Y

### Quality Settings
Pour de meilleures performances pendant le développement:
- Réduire la qualité des ombres
- Désactiver les réflexions en temps réel

### Player Settings
- Company Name: Votre nom
- Product Name: RA-SSE Simulator
- Default Screen Width: 1920
- Default Screen Height: 1080

---

## ✅ Validation Finale

Avant de considérer l'installation complète:

- [ ] Le projet compile sans erreurs
- [ ] Le joueur peut se déplacer
- [ ] Les victimes apparaissent
- [ ] L'interface AR s'affiche
- [ ] Le triage fonctionne
- [ ] La navigation fonctionne
- [ ] Le menu pause fonctionne
- [ ] Le son fonctionne (si configuré)

---

## 📞 Support

En cas de problème:
1. Vérifier la Console Unity pour les erreurs
2. Consulter le README.md pour la documentation
3. Vérifier que tous les scripts sont correctement assignés
4. Tester chaque système individuellement
