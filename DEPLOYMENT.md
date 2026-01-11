# Guide de Déploiement - RA-SSE Simulateur de Triage

## Prérequis

### Environnement de Développement
- **Unity**: 2022.3 LTS ou supérieur
- **IDE**: Visual Studio 2022 / JetBrains Rider
- **.NET**: .NET Standard 2.1

### Pour Android (RealWear)
- Android SDK API Level 28-33
- Android NDK r21e ou supérieur
- Java JDK 11

### Packages Unity Requis
```json
{
  "com.unity.inputsystem": "1.7.0",
  "com.unity.textmeshpro": "3.0.6",
  "com.unity.ai.navigation": "1.1.5",
  "com.unity.xr.arfoundation": "5.1.2",
  "com.unity.localization": "1.4.5"
}
```

---

## Déploiement Rapide

### 1. Cloner le Projet

```bash
git clone [repository-url]
cd UnityProject_RA_SSE
```

### 2. Ouvrir dans Unity

1. Ouvrir Unity Hub
2. Cliquer "Add" et sélectionner le dossier `UnityProject_RA_SSE`
3. Ouvrir avec Unity 2022.3 LTS

### 3. Importer les Packages

Unity importera automatiquement les packages depuis `Packages/manifest.json`.

Si des erreurs apparaissent:
```
Window → Package Manager → [+] → Add package from git URL
```

### 4. Configurer le Build

#### Android (RealWear Navigator 520)
```
File → Build Settings → Android
Player Settings:
  - Company Name: RASSE
  - Product Name: RA-SSE Simulateur Triage
  - Package Name: com.rasse.triagesimulator
  - Minimum API Level: 28
  - Target API Level: 33
  - Scripting Backend: IL2CPP
  - Target Architectures: ARM64
```

#### Windows Standalone
```
File → Build Settings → PC, Mac & Linux Standalone
  - Architecture: x86_64
  - Scripting Backend: Mono ou IL2CPP
```

### 5. Build

#### Via Menu
```
RA-SSE → Build → Android (RealWear)
RA-SSE → Build → Windows Standalone
RA-SSE → Build → All Platforms
```

#### Via Script
```csharp
RASSE.Editor.BuildConfiguration.BuildAndroid();
RASSE.Editor.BuildConfiguration.BuildWindows();
```

---

## Structure des Builds

```
Builds/
├── Android/
│   └── RA-SSE.apk          # APK pour RealWear
├── Windows/
│   ├── RA-SSE.exe          # Exécutable Windows
│   ├── RA-SSE_Data/        # Données du jeu
│   └── UnityPlayer.dll
└── WebGL/
    └── RA-SSE-WebGL/       # Build WebGL
```

---

## Installation sur RealWear

### Via ADB
```bash
adb install -r Builds/Android/RA-SSE.apk
```

### Via RealWear Companion
1. Connecter le RealWear au PC
2. Ouvrir RealWear Companion
3. Glisser-déposer l'APK

### Permissions Requises
L'application demandera:
- 📷 Caméra (détection victimes)
- 🎤 Microphone (commandes vocales)
- 📍 Localisation (navigation)

---

## Configuration Post-Déploiement

### Paramètres Recommandés

| Paramètre | Valeur | Description |
|-----------|--------|-------------|
| Qualité Graphique | Medium | Équilibre performance/visuel |
| Résolution | 1920x1080 | Native RealWear |
| Volume Voix | 100% | Commandes vocales |
| Mode Hors-ligne | Activé | Fonctionnement sans réseau |

### Commandes Vocales

| Commande | Action |
|----------|--------|
| "RASSE" | Activer l'écoute |
| "Triage rouge" | Classifier en urgence absolue |
| "Triage jaune" | Classifier en urgence relative |
| "Triage vert" | Classifier en impliqué |
| "Suivant" | Passer à la victime suivante |
| "Évacuer" | Demander évacuation |
| "Menu" | Ouvrir le menu pause |

---

## Vérification du Déploiement

### Checklist

- [ ] L'application se lance sans erreur
- [ ] Le menu principal s'affiche correctement
- [ ] La scène de formation charge
- [ ] Les victimes apparaissent
- [ ] Le triage fonctionne
- [ ] Les commandes vocales répondent
- [ ] L'interface AR s'affiche
- [ ] Les sons sont audibles
- [ ] Les sauvegardes fonctionnent

### Tests de Performance

| Métrique | Cible | Méthode de Test |
|----------|-------|-----------------|
| FPS | ≥30 | Stats Unity |
| Latence Triage | ≤30s | Chronomètre |
| Autonomie | ≥8h | Test batterie |
| Précision | ≥95% | Tests unitaires |

---

## Dépannage

### Erreurs Courantes

#### "Scene not found"
```
Vérifier Build Settings → Scenes In Build
Toutes les scènes doivent être cochées
```

#### "Missing script"
```
Reimporter les assets:
Assets → Reimport All
```

#### "AR not working"
```
Vérifier que ARCore est installé sur le device
Player Settings → XR → ARCore activé
```

#### "Voice commands not responding"
```
Vérifier permissions microphone
Settings → Apps → RA-SSE → Permissions
```

### Logs

```bash
# Android
adb logcat -s Unity

# Windows
%USERPROFILE%\AppData\LocalLow\RASSE\RA-SSE Simulateur Triage\Player.log
```

---

## Support

- 📧 Email: support@rasse-project.fr
- 📖 Documentation: https://docs.rasse-project.fr
- 🐛 Issues: https://github.com/rasse-project/issues

---

## Versions

| Version | Date | Notes |
|---------|------|-------|
| 1.0.0 | 2026-01-10 | Release initiale |

---

*Dernière mise à jour: Janvier 2026*
