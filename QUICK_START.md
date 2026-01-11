# 🚀 Guide de Démarrage Rapide - RA-SSE

## Prérequis

- **Unity 2022.3 LTS** ou supérieur
- **Git** (optionnel, pour le contrôle de version)
- **Android SDK** (pour le build RealWear)

## Installation en 5 minutes

### 1. Cloner/Copier le projet

```bash
# Option A: Copier le dossier
cp -r UnityProject_RA_SSE ~/UnityProjects/

# Option B: Git clone (si configuré)
git clone https://github.com/your-repo/ra-sse.git
```

### 2. Ouvrir dans Unity Hub

1. Ouvrir **Unity Hub**
2. Cliquer sur **Add** → **Add project from disk**
3. Sélectionner le dossier `UnityProject_RA_SSE`
4. Unity détectera automatiquement la version requise

### 3. Première exécution

1. Attendre la fin de l'importation des assets
2. Ouvrir la scène `Scenes/MainMenu.unity`
3. Cliquer sur **Play** ▶️

## Structure du Projet

```
UnityProject_RA_SSE/
├── 📁 Scenes/           → 5 scènes (Menu, Training, 3 Scénarios)
├── 📁 Scripts/          → 47 scripts C#
├── 📁 Prefabs/          → 12 prefabs
├── 📁 ScriptableObjects/ → 27 assets de données
├── 📁 Materials/        → 5 matériaux
├── 📁 ProjectSettings/  → Configuration Unity
└── 📁 Packages/         → Dépendances
```

## Scènes Disponibles

| Scène | Description | Difficulté |
|-------|-------------|------------|
| `MainMenu` | Menu principal | - |
| `TrainingScene` | Tutoriel/Formation | ⭐ |
| `Scenario_IndustrialExplosion` | Explosion industrielle | ⭐⭐⭐ |
| `Scenario_TrainAccident` | Accident ferroviaire | ⭐⭐⭐ |
| `Scenario_BuildingCollapse` | Effondrement bâtiment | ⭐⭐⭐⭐ |

## Commandes de Base

### Clavier (Mode Debug)

| Touche | Action |
|--------|--------|
| `WASD` | Déplacement |
| `E` | Interagir |
| `V` | Commande vocale |
| `T` | Confirmer triage |
| `Tab` | Victime suivante |
| `P` | Pause |
| `R` | Toggle AR |

### Commandes Vocales (Simulées)

- **"ROUGE"** - Classer urgence absolue
- **"JAUNE"** - Classer urgence relative
- **"VERT"** - Classer impliqué
- **"NOIR"** - Classer décédé
- **"SUIVANT"** - Victime suivante
- **"ÉVACUER"** - Demander évacuation

## Build pour RealWear

### Configuration

1. **File** → **Build Settings**
2. Sélectionner **Android**
3. **Player Settings**:
   - Company Name: `RASSE`
   - Product Name: `RA-SSE Simulateur`
   - Package Name: `com.rasse.triagesimulator`
   - Minimum API Level: **28** (Android 9)
   - Target API Level: **33** (Android 13)

### Build

```
File → Build Settings → Build
```

## Personnalisation

### Ajouter un scénario

1. Créer un nouveau `ScenarioProfileSO`:
   - `Assets/Create/RASSE/Scenario Profile`

2. Créer la scène:
   - Dupliquer une scène existante
   - Modifier l'environnement
   - Configurer les spawn points

3. Ajouter au Build Settings

### Modifier les protocoles

Les protocoles sont dans `ScriptableObjects/TriageProtocols/`:
- `TriageProtocol_START.asset` - Protocole START standard

### Ajouter des victimes

Créer de nouveaux `VictimProfileSO`:
- `Assets/Create/RASSE/Victim Profile`

## Dépannage

### "Missing Script" dans les prefabs

1. Vérifier que les namespaces sont corrects
2. Réimporter: `Assets → Reimport All`

### Scène ne charge pas

1. Vérifier le Build Settings
2. Scène ajoutée à la liste?

### Performance faible

1. Réduire Quality Settings
2. Désactiver les effets post-processing
3. Vérifier le Target FPS (60 recommandé)

## Support

- 📖 Documentation complète: `README.md`
- 🗺️ Mapping rapport: `CORRESPONDANCE_RAPPORT.md`
- 📝 Changelog: `CHANGELOG.md`

## Conformité

Ce simulateur implémente:
- ✅ Protocole START de triage médical
- ✅ Exigences REQ-1 à REQ-7
- ✅ NFR-* (Performance, Sécurité, UX)
- ✅ Normes ISO 14971, IEC 62304, MDR 2017/745

---

**Bon entraînement!** 🏥🚑
