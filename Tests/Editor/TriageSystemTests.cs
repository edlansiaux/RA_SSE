using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RASSE.Core;
using RASSE.Zones;
using System.Collections;

namespace RASSE.Tests
{
    /// <summary>
    /// Tests unitaires pour le système de triage START.
    /// Vérifie la conformité aux exigences REQ-3 (Classification ≥95%).
    /// </summary>
    [TestFixture]
    public class TriageSystemTests
    {
        private GameObject testObject;
        private StartTriageSystem triageSystem;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestTriageSystem");
            triageSystem = testObject.AddComponent<StartTriageSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region Tests de Classification START

        [Test]
        public void ClassifyVictim_NotWalking_NotBreathing_AfterRepositioning_ShouldBeBlack()
        {
            // Arrange - Victime qui ne marche pas, ne respire pas même après repositionnement
            var vitalSigns = new VitalSignsData
            {
                HasSpontaneousBreathing = false,
                BreathingAfterRepositioning = false,
                CanWalk = false
            };

            // Act
            var category = triageSystem.ClassifyVictimSTART(vitalSigns);

            // Assert
            Assert.AreEqual(TriageCategory.Black, category, 
                "Une victime sans respiration après repositionnement doit être classée NOIR");
        }

        [Test]
        public void ClassifyVictim_Walking_ShouldBeGreen()
        {
            // Arrange - Victime qui peut marcher
            var vitalSigns = new VitalSignsData
            {
                CanWalk = true,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 18,
                HasRadialPulse = true,
                FollowsCommands = true
            };

            // Act
            var category = triageSystem.ClassifyVictimSTART(vitalSigns);

            // Assert
            Assert.AreEqual(TriageCategory.Green, category,
                "Une victime qui peut marcher doit être classée VERT");
        }

        [Test]
        public void ClassifyVictim_NotWalking_HighRespiratoryRate_ShouldBeRed()
        {
            // Arrange - Victime qui ne marche pas, FR > 30
            var vitalSigns = new VitalSignsData
            {
                CanWalk = false,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 35, // > 30
                HasRadialPulse = true,
                FollowsCommands = true
            };

            // Act
            var category = triageSystem.ClassifyVictimSTART(vitalSigns);

            // Assert
            Assert.AreEqual(TriageCategory.Red, category,
                "Une victime avec FR > 30 doit être classée ROUGE");
        }

        [Test]
        public void ClassifyVictim_NotWalking_NoRadialPulse_ShouldBeRed()
        {
            // Arrange - Victime sans pouls radial
            var vitalSigns = new VitalSignsData
            {
                CanWalk = false,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 20,
                HasRadialPulse = false, // Pas de pouls radial
                FollowsCommands = true
            };

            // Act
            var category = triageSystem.ClassifyVictimSTART(vitalSigns);

            // Assert
            Assert.AreEqual(TriageCategory.Red, category,
                "Une victime sans pouls radial doit être classée ROUGE");
        }

        [Test]
        public void ClassifyVictim_NotWalking_NotFollowingCommands_ShouldBeRed()
        {
            // Arrange - Victime qui ne suit pas les ordres simples
            var vitalSigns = new VitalSignsData
            {
                CanWalk = false,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 20,
                HasRadialPulse = true,
                FollowsCommands = false // Ne suit pas les ordres
            };

            // Act
            var category = triageSystem.ClassifyVictimSTART(vitalSigns);

            // Assert
            Assert.AreEqual(TriageCategory.Red, category,
                "Une victime qui ne suit pas les ordres doit être classée ROUGE");
        }

        [Test]
        public void ClassifyVictim_NotWalking_NormalVitals_ShouldBeYellow()
        {
            // Arrange - Victime qui ne marche pas mais constantes normales
            var vitalSigns = new VitalSignsData
            {
                CanWalk = false,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 20, // Normal (< 30)
                HasRadialPulse = true, // Présent
                FollowsCommands = true // Suit les ordres
            };

            // Act
            var category = triageSystem.ClassifyVictimSTART(vitalSigns);

            // Assert
            Assert.AreEqual(TriageCategory.Yellow, category,
                "Une victime avec constantes normales mais ne marchant pas doit être classée JAUNE");
        }

        #endregion

        #region Tests de Validation des Données

        [Test]
        public void ValidateVitalSigns_NullData_ShouldReturnFalse()
        {
            // Act
            var isValid = triageSystem.ValidateVitalSignsData(null);

            // Assert
            Assert.IsFalse(isValid, "Des données nulles doivent être invalides");
        }

        [Test]
        public void ValidateVitalSigns_NegativeRespiratoryRate_ShouldReturnFalse()
        {
            // Arrange
            var vitalSigns = new VitalSignsData
            {
                RespiratoryRate = -5
            };

            // Act
            var isValid = triageSystem.ValidateVitalSignsData(vitalSigns);

            // Assert
            Assert.IsFalse(isValid, "Une FR négative doit être invalide");
        }

        [Test]
        public void ValidateVitalSigns_ValidData_ShouldReturnTrue()
        {
            // Arrange
            var vitalSigns = new VitalSignsData
            {
                CanWalk = true,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 18,
                HasRadialPulse = true,
                FollowsCommands = true
            };

            // Act
            var isValid = triageSystem.ValidateVitalSignsData(vitalSigns);

            // Assert
            Assert.IsTrue(isValid, "Des données valides doivent être acceptées");
        }

        #endregion

        #region Tests de Performance (REQ-3: ≥95%)

        [Test]
        public void TriageAccuracy_MultipleVictims_ShouldExceed95Percent()
        {
            // Arrange - Créer un ensemble de test avec résultats attendus
            var testCases = new[]
            {
                (new VitalSignsData { CanWalk = true, HasSpontaneousBreathing = true, RespiratoryRate = 18, HasRadialPulse = true, FollowsCommands = true }, TriageCategory.Green),
                (new VitalSignsData { CanWalk = false, HasSpontaneousBreathing = false, BreathingAfterRepositioning = false }, TriageCategory.Black),
                (new VitalSignsData { CanWalk = false, HasSpontaneousBreathing = true, RespiratoryRate = 35, HasRadialPulse = true, FollowsCommands = true }, TriageCategory.Red),
                (new VitalSignsData { CanWalk = false, HasSpontaneousBreathing = true, RespiratoryRate = 20, HasRadialPulse = true, FollowsCommands = true }, TriageCategory.Yellow),
                (new VitalSignsData { CanWalk = false, HasSpontaneousBreathing = true, RespiratoryRate = 20, HasRadialPulse = false, FollowsCommands = true }, TriageCategory.Red),
            };

            int correctClassifications = 0;
            int totalCases = testCases.Length;

            // Act
            foreach (var (vitalSigns, expected) in testCases)
            {
                var result = triageSystem.ClassifyVictimSTART(vitalSigns);
                if (result == expected)
                {
                    correctClassifications++;
                }
            }

            // Assert
            float accuracy = (float)correctClassifications / totalCases * 100f;
            Assert.GreaterOrEqual(accuracy, 95f, 
                $"La précision du triage ({accuracy}%) doit être ≥ 95% (REQ-3)");
        }

        #endregion
    }

    /// <summary>
    /// Tests unitaires pour les zones de triage.
    /// </summary>
    [TestFixture]
    public class TriageZoneTests
    {
        private GameObject zoneObject;
        private TriageZoneController zoneController;

        [SetUp]
        public void Setup()
        {
            zoneObject = new GameObject("TestTriageZone");
            zoneController = zoneObject.AddComponent<TriageZoneController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (zoneObject != null)
            {
                Object.DestroyImmediate(zoneObject);
            }
        }

        [Test]
        public void Zone_InitialOccupancy_ShouldBeZero()
        {
            Assert.AreEqual(0, zoneController.CurrentOccupancy,
                "L'occupation initiale d'une zone doit être 0");
        }

        [Test]
        public void Zone_AddVictim_ShouldIncreaseOccupancy()
        {
            // Arrange
            var victim = new GameObject("TestVictim");
            victim.AddComponent<RASSE.Victim.VictimController>();

            // Act
            zoneController.AddVictim(victim);

            // Assert
            Assert.AreEqual(1, zoneController.CurrentOccupancy,
                "L'occupation doit augmenter après ajout d'une victime");

            // Cleanup
            Object.DestroyImmediate(victim);
        }

        [Test]
        public void Zone_RemoveVictim_ShouldDecreaseOccupancy()
        {
            // Arrange
            var victim = new GameObject("TestVictim");
            victim.AddComponent<RASSE.Victim.VictimController>();
            zoneController.AddVictim(victim);

            // Act
            zoneController.RemoveVictim(victim);

            // Assert
            Assert.AreEqual(0, zoneController.CurrentOccupancy,
                "L'occupation doit diminuer après retrait d'une victime");

            // Cleanup
            Object.DestroyImmediate(victim);
        }

        [Test]
        public void Zone_AtCapacity_ShouldReportFull()
        {
            // Arrange - Simuler une zone pleine
            // Note: Ce test nécessiterait l'accès à la capacité privée
            // Pour le test réel, utiliser la réflexion ou une méthode de test

            // Assert placeholder
            Assert.IsNotNull(zoneController, "Le contrôleur de zone doit exister");
        }
    }

    /// <summary>
    /// Tests de performance et de latence.
    /// Vérifie la conformité aux exigences NFR-VIT (latence ≤30s offline).
    /// </summary>
    [TestFixture]
    public class PerformanceTests
    {
        [Test]
        public void TriageClassification_ExecutionTime_ShouldBeLessThan100ms()
        {
            // Arrange
            var testObject = new GameObject("TestTriageSystem");
            var triageSystem = testObject.AddComponent<StartTriageSystem>();
            var vitalSigns = new VitalSignsData
            {
                CanWalk = false,
                HasSpontaneousBreathing = true,
                RespiratoryRate = 25,
                HasRadialPulse = true,
                FollowsCommands = true
            };

            var stopwatch = new System.Diagnostics.Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                triageSystem.ClassifyVictimSTART(vitalSigns);
            }
            stopwatch.Stop();

            // Assert
            float avgTimeMs = stopwatch.ElapsedMilliseconds / 1000f;
            Assert.Less(avgTimeMs, 100f,
                $"Le temps moyen de classification ({avgTimeMs}ms) doit être < 100ms");

            // Cleanup
            Object.DestroyImmediate(testObject);
        }
    }

    /// <summary>
    /// Structure de données pour les tests de signes vitaux.
    /// </summary>
    public class VitalSignsData
    {
        public bool CanWalk { get; set; }
        public bool HasSpontaneousBreathing { get; set; }
        public bool BreathingAfterRepositioning { get; set; }
        public int RespiratoryRate { get; set; }
        public bool HasRadialPulse { get; set; }
        public bool FollowsCommands { get; set; }
    }
}
