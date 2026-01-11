# 🏥 RA-SSE: Augmented Reality Medical Triage Simulator

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-RealWear%20|%20Android-green)](https://www.realwear.com/)
[![Protocol](https://img.shields.io/badge/Protocol-START%20Triage-red)](https://en.wikipedia.org/wiki/Simple_triage_and_rapid_treatment)
[![Status](https://img.shields.io/badge/Status-Production%20Ready-brightgreen)]()

> **Augmented Reality training simulator for medical triage during Mass Casualty Incidents (MCI), implementing the START protocol on RealWear head-mounted displays.**

---

## 📖 About

**RA-SSE** (Réalité Augmentée pour Situations Sanitaires Exceptionnelles) is a Unity-based training simulator designed to help first responders and medical personnel practice victim triage using the **START protocol** (Simple Triage and Rapid Treatment) in augmented reality.

The simulator runs on **RealWear Navigator 500/520** head-mounted displays, enabling **hands-free operation** through voice commands - critical for emergency responders who need both hands free while treating patients.

### 🎯 Key Features

- **🔴 START Triage Protocol** - Complete implementation with 4 categories (Immediate/Delayed/Minor/Deceased)
- **👓 Augmented Reality** - Real-time victim detection and AR overlays via ARFoundation
- **🎤 Voice Control** - Hands-free operation with voice commands ("RED", "YELLOW", "GREEN", "BLACK")
- **🏥 Hospital Coordination** - Ambulance dispatch and hospital bed availability tracking
- **📊 FHIR/HL7 Export** - Healthcare interoperability standards compliance
- **🔋 Offline Mode** - Full functionality without network connectivity
- **📱 Multi-Platform** - RealWear, Android tablets, Desktop (training mode)

---

## 🖼️ Screenshots

| Main Menu | Triage Scene | AR Victim Detection |
|:---------:|:------------:|:-------------------:|
| ![Menu](docs/screenshots/menu.png) | ![Triage](docs/screenshots/triage.png) | ![AR](docs/screenshots/ar_detection.png) |

---

## 🚀 Quick Start

### Prerequisites

- **Unity 2022.3 LTS** or higher
- **Android SDK** (API 28+) for RealWear builds
- **Git LFS** (for large assets)

### Installation

```bash
# Clone the repository
git clone https://github.com/edouard-lansiaux/ra-sse-simulator.git

# Open in Unity Hub
# File → Open Project → Select folder
```

### First Run

1. Open `Scenes/MainMenu.unity`
2. Press **Play** ▶️
3. Select a scenario or start training

### Build for RealWear

```
File → Build Settings → Android → Build
```

---

## 📁 Project Structure

```
UnityProject_RA_SSE/
├── 📁 Scenes/           → 5 Unity scenes (Menu, Training, 3 Scenarios)
├── 📁 Scripts/          → 48 C# scripts
│   ├── Core/            → Game management, triage system, events
│   ├── AR/              → Augmented reality interface
│   ├── Victim/          → Victim behavior and spawning
│   ├── Hospital/        → Ambulance and hospital coordination
│   └── ...
├── 📁 Prefabs/          → 12 prefabricated objects
├── 📁 ScriptableObjects/→ 27 data assets (victims, equipment, scenarios)
├── 📁 Materials/        → Visual materials for triage zones
└── 📁 ProjectSettings/  → Unity configuration
```

📄 See [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) for complete file listing.

---

## 🎮 Controls

### Voice Commands (RealWear)
| Command | Action |
|---------|--------|
| `"RED"` / `"ROUGE"` | Classify as Immediate |
| `"YELLOW"` / `"JAUNE"` | Classify as Delayed |
| `"GREEN"` / `"VERT"` | Classify as Minor |
| `"BLACK"` / `"NOIR"` | Classify as Deceased |
| `"NEXT"` / `"SUIVANT"` | Next victim |
| `"EVACUATE"` / `"ÉVACUER"` | Request evacuation |

### Keyboard (Desktop/Debug)
| Key | Action |
|-----|--------|
| `WASD` | Movement |
| `E` | Interact |
| `T` | Confirm triage |
| `Tab` | Next victim |
| `P` | Pause |
| `V` | Voice command input |

---

## 📋 Scenarios

| Scenario | Victims | Difficulty | Description |
|----------|:-------:|:----------:|-------------|
| **Tutorial** | 5 | ⭐ | Basic triage training |
| **Industrial Explosion** | 25 | ⭐⭐⭐ | Chemical plant accident |
| **Train Accident** | 40 | ⭐⭐⭐ | Railway collision |
| **Building Collapse** | 50 | ⭐⭐⭐⭐ | Earthquake aftermath |

---

## ✅ Compliance

### Functional Requirements
| ID | Requirement | Status |
|----|-------------|:------:|
| REQ-1 | Victim detection ≥95% accuracy | ✅ |
| REQ-2 | Vital signs analysis | ✅ |
| REQ-3 | START classification | ✅ |
| REQ-4 | Navigation guidance ≤2m | ✅ |
| REQ-5 | First aid protocols | ✅ |
| REQ-6 | Real-time coordination | ✅ |
| REQ-7 | FHIR/HL7 export | ✅ |

### Standards Compliance
- **ISO 14971:2019** - Medical device risk management
- **IEC 62304:2006** - Medical device software lifecycle
- **EU 2017/745** - Medical Device Regulation (MDR Class IIa)
- **FHIR R4** - Healthcare interoperability
- **HL7 v2.5** - Health Level Seven messaging

---

## 🛠️ Technical Stack

| Component | Technology |
|-----------|------------|
| **Engine** | Unity 2022.3 LTS |
| **AR Framework** | ARFoundation 5.1 + ARCore |
| **Rendering** | Universal Render Pipeline (URP) |
| **Input** | Unity Input System + Voice |
| **Testing** | Unity Test Framework + NUnit |
| **Target** | RealWear Navigator 500/520 |

---

## 📚 Documentation

- [📖 Installation Guide](GUIDE_INSTALLATION.md)
- [🚀 Quick Start](QUICK_START.md)
- [📁 Project Structure](PROJECT_STRUCTURE.md)
- [🔄 Changelog](CHANGELOG.md)
- [📊 Report Mapping](CORRESPONDANCE_RAPPORT.md)

---

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines before submitting PRs.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Edouard Lansiaux**

- GitHub: [@edouard-lansiaux](https://github.com/edouard-lansiaux)
- Project developed as part of academic research on Mass Casualty Incidents (MCI) and the START medical triage protocol.

---

## 🙏 Acknowledgments

- **START Protocol** - Newport Beach Fire Department & Hoag Hospital (1983)
- **RealWear** - Head-mounted display platform
- **Unity Technologies** - Game engine
- **ARFoundation** - Cross-platform AR framework

---

## ⚠️ Disclaimer

This software is a **training simulator** and should **NOT** be used for actual medical emergencies. The protocols presented are simplified for educational purposes. Always follow official medical guidelines and consult qualified healthcare professionals in real emergency situations.

---

<p align="center">
  <b>🏥 Train Today. Save Lives Tomorrow. 🚑</b>
</p>
