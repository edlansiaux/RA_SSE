# 🚑 RA-SSE - Simulateur de Triage Médical en Réalité Augmentée

## 📋 Description

**RA-SSE** (Réalité Augmentée - Situations Sanitaires Exceptionnelles) est un simulateur 3D développé sous Unity permettant de s'entraîner au protocole START (Simple Triage And Rapid Treatment) utilisé par les services de secours lors d'accidents à victimes multiples.

Le simulateur reproduit l'interface d'un système de lunettes à réalité augmentée destiné aux secouristes, offrant :
- Détection et localisation des victimes
- Analyse des signes vitaux
- Classification START automatisée
- Guidage vers les ambulances
- Création de dossiers patients
- Coordination hospitalière

---

## 🎮 Fonctionnalités

### Protocole START
- **4 catégories de triage** : Rouge (immédiat), Jaune (différé), Vert (mineur), Noir (décédé)
- Algorithme complet basé sur : respiration, fréquence respiratoire, circulation, neurologie, mobilité
- Suggestion automatique avec possibilité de correction manuelle
- Validation vocale ou par interface

### Interface AR Simulée
- Affichage des signes vitaux en temps réel
- Indicateurs colorés selon la gravité
- Panneau de triage interactif
- Navigation guidée avec flèches directionnelles
- Mini-carte et boussole
- Notifications et alertes

### Gestion des Victimes
- Génération procédurale selon la difficulté
- 16 types de blessures différentes
- Détérioration progressive des états
- Animations et sons contextuels

### Coordination Hospitalière
- 5 hôpitaux avec spécialités différentes
- Algorithme de sélection optimale
- Gestion des capacités en temps réel
- Réservation de lits

### Système d'Ambulances
- 3 types : VSAV, VLI, SMUR
- Assignation intelligente selon gravité
- États et déplacements automatiques
- ETA et suivi en temps réel

### Navigation
- Guidage NavMesh vers les destinations
- Affichage du chemin en 3D
- Instructions directionnelles
- Marqueurs visuels

---

## 📁 Structure du Projet

```
UnityProject_RA_SSE/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # Gestionnaire principal
│   │   ├── StartTriageSystem.cs    # Protocole START
│   │   ├── RescuerController.cs    # Contrôle joueur FPS
│   │   ├── VoiceCommandSimulator.cs # Commandes vocales
│   │   └── EnumDefinitions.cs      # Enums et structures
│   │
│   ├── Victim/
│   │   ├── VictimController.cs     # Contrôle individuel victime
│   │   └── VictimSpawner.cs        # Génération des victimes
│   │
│   ├── AR/
│   │   └── ARInterfaceController.cs # Interface AR complète
│   │
│   ├── Hospital/
│   │   ├── HospitalCoordinationSystem.cs # Coordination hôpitaux
│   │   └── AmbulanceManager.cs     # Gestion ambulances
│   │
│   ├── Navigation/
│   │   └── NavigationSystem.cs     # Guidage NavMesh
│   │
│   ├── Data/
│   │   ├── PatientRecordSystem.cs  # Dossiers patients JSON
│   │   └── ScriptableObjects.cs    # Données configurables
│   │
│   ├── UI/
│   │   ├── UIManager.cs            # Gestionnaire UI
│   │   ├── MainMenuController.cs   # Menu principal
│   │   ├── HUDController.cs        # Interface en jeu
│   │   └── PauseAndResultsUI.cs    # Pause et résultats
│   │
│   ├── Audio/
│   │   └── AudioManager.cs         # Gestionnaire audio
│   │
│   └── Training/
│       └── TutorialManager.cs      # Système de tutoriel
│
├── Prefabs/                        # (À créer)
├── ScriptableObjects/              # (À créer)
├── Scenes/                         # (À créer)
└── Documentation/
```

---

## 🚀 Installation

### Prérequis
- Unity 2022.3 LTS ou plus récent
- TextMeshPro (package Unity)
- Navigation AI (package Unity)

### Étapes

1. **Créer un nouveau projet Unity 3D**

2. **Importer les scripts**
   - Copiez le dossier `Scripts/` dans `Assets/Scripts/`

3. **Installer les packages requis**
   - Window → Package Manager
   - Installer : TextMeshPro, AI Navigation

4. **Créer les Prefabs nécessaires**
   - Victim Prefab (avec VictimController)
   - Ambulance Prefab (capsule/modèle 3D)
   - Markers (cubes colorés pour AR)
   - Notification Prefab (UI)

5. **Configurer la scène principale**
   - Créer un terrain ou environnement
   - Ajouter NavMeshSurface et bake
   - Placer les spawn points

6. **Ajouter les Managers**
   - Créer un GameObject "GameManager" avec les scripts :
     - GameManager
     - VictimSpawner
     - HospitalCoordinationSystem
     - AmbulanceManager
     - PatientRecordSystem
     - AudioManager

7. **Configurer le joueur**
   - Créer un GameObject "Player" avec :
     - RescuerController
     - CharacterController
     - Camera (enfant)
   - Tag : "Rescuer"

8. **Configurer l'UI**
   - Créer un Canvas avec :
     - ARInterfaceController
     - HUDController
     - PauseMenuController

---

## 🎮 Contrôles

### Déplacement
| Touche | Action |
|--------|--------|
| WASD | Se déplacer |
| Shift | Courir |
| Souris | Regarder |

### Interaction
| Touche | Action |
|--------|--------|
| E | Interagir avec victime |
| Q | Scanner la zone |

### Triage (Commandes Vocales Simulées)
| Touche | Commande |
|--------|----------|
| Enter | Valider |
| 1 | Rouge |
| 2 | Jaune |
| 3 | Vert |
| 4 | Noir |

### Navigation
| Touche | Action |
|--------|--------|
| N | Victime suivante |
| H | Confirmer hôpital |
| A | Appeler ambulance |

### Système
| Touche | Action |
|--------|--------|
| Escape | Pause |
| Tab | Statut global |
| F1 | Aide |

---

## 📊 Protocole START

```
                    La victime respire-t-elle ?
                            │
              ┌─────────────┼─────────────┐
              │             │             │
            NON        Manœuvre VA       OUI
              │             │             │
              │      Respire après ?      │
              │             │             │
         ┌────┴────┐   ┌───┴───┐         │
        NOIR      OUI        NON         │
         │          │          │         │
         │       ROUGE      NOIR         │
         │                               │
         │         FR > 30 ou < 10 ?     │
         │                │              │
         │         ┌──────┴──────┐       │
         │        OUI           NON      │
         │         │              │      │
         │      ROUGE       RC > 2s ?    │
         │                     │         │
         │              ┌──────┴──────┐  │
         │             OUI           NON │
         │              │              │ │
         │           ROUGE    Suit ordres ? │
         │                         │     │
         │                  ┌──────┴─────┤
         │                 NON          OUI
         │                  │            │
         │               ROUGE    Peut marcher ?
         │                              │
         │                       ┌──────┴──────┐
         │                      OUI           NON
         │                       │             │
         │                     VERT         JAUNE
```

---

## ⚙️ Configuration

### Niveaux de Difficulté

| Niveau | Victimes | Temps | Particularités |
|--------|----------|-------|----------------|
| Tutoriel | 3 | Illimité | Guidé, indices |
| Facile | 5 | Illimité | Cas simples |
| Normal | 10 | 15 min | Cas variés |
| Difficile | 15 | 12 min | Détérioration rapide |
| Expert | 20+ | 10 min | Conditions difficiles |

### Types d'Hôpitaux

- **CHU** : Toutes spécialités, grande capacité
- **CH** : Trauma + Cardiologie
- **Clinique** : Soins de base, capacité limitée

### Types d'Ambulances

- **VSAV** : Transport basique (2 personnes)
- **VLI** : Avec infirmier (3 personnes)
- **SMUR** : Réanimation (médecin + infirmier)

---

## 📈 Système de Score

Le score final est calculé selon :

```
Score = (Précision × 40%) + (Temps × 25%) + (Évacuations × 20%) + (Survie × 15%)
```

### Grades

| Score | Grade |
|-------|-------|
| 9500+ | S+ |
| 9000+ | S |
| 8000+ | A |
| 7000+ | B |
| 6000+ | C |
| 5000+ | D |
| <5000 | F |

### Médailles

- 🏆 **Triage Parfait** : 100% de précision
- 💚 **Zéro Perte** : Aucun décès
- ⚡ **Rapide** : Sous le temps limite
- 🚑 **Évacuation Totale** : Toutes victimes évacuées

---

## 🔧 Personnalisation

### Créer un Nouveau Scénario

1. Assets → Create → RA SSE → Scenario Data
2. Configurer :
   - Nom et description
   - Nombre de victimes
   - Hôpitaux disponibles
   - Conditions environnementales
   - Objectifs

### Ajouter des Victimes Prédéfinies

1. Assets → Create → RA SSE → Victim Data
2. Définir :
   - Informations personnelles
   - Signes vitaux
   - Type de blessure
   - Position de spawn

---

## 🎯 Objectifs Pédagogiques

Ce simulateur permet aux apprenants de :

1. **Maîtriser le protocole START** en conditions réalistes
2. **Développer des automatismes** de triage rapide
3. **Gérer le stress** des situations d'urgence
4. **Coordonner les évacuations** avec les moyens disponibles
5. **Utiliser des interfaces AR** modernes

---

## 📝 Notes de Développement

### Architecture

Le projet suit une architecture modulaire avec :
- **Singleton Pattern** pour les managers
- **Event-Driven** pour la communication inter-systèmes
- **ScriptableObjects** pour les données configurables
- **Coroutines** pour les opérations asynchrones

### Performance

- Object pooling recommandé pour les victimes
- LOD pour les modèles 3D
- Occlusion culling pour les grandes scènes

### Extensions Possibles

- [ ] Mode multijoueur coopératif
- [ ] Scénarios de catastrophe naturelle
- [ ] Intégration VR native
- [ ] Export des rapports PDF
- [ ] IA pour victimes dynamiques

---

## 📄 Licence

Ce projet est développé à des fins éducatives pour la formation des secouristes.

---

## 👥 Crédits

Développé comme projet de simulation pour l'apprentissage du triage médical d'urgence.

Basé sur le protocole START (Simple Triage And Rapid Treatment) développé par le Newport Beach Fire Department et Hoag Hospital.

---

## 📞 Support

Pour toute question ou suggestion, consultez la documentation ou créez une issue dans le repository.
