using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.Audio
{
    /// <summary>
    /// AudioManager - Gestionnaire audio centralisé
    /// Gère la musique, les effets sonores et les sons d'ambiance
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("=== SOURCES AUDIO ===")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambianceSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource uiSource;

        [Header("=== MUSIQUES ===")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip tensionMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;

        [Header("=== AMBIANCES ===")]
        [SerializeField] private AudioClip cityAmbiance;
        [SerializeField] private AudioClip emergencyAmbiance;
        [SerializeField] private AudioClip hospitalAmbiance;
        [SerializeField] private AudioClip rainAmbiance;
        [SerializeField] private AudioClip windAmbiance;

        [Header("=== EFFETS SONORES - INTERFACE ===")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip notification;
        [SerializeField] private AudioClip alertCritical;
        [SerializeField] private AudioClip alertWarning;
        [SerializeField] private AudioClip alertInfo;
        [SerializeField] private AudioClip menuOpen;
        [SerializeField] private AudioClip menuClose;

        [Header("=== EFFETS SONORES - TRIAGE ===")]
        [SerializeField] private AudioClip triageStart;
        [SerializeField] private AudioClip triageConfirm;
        [SerializeField] private AudioClip triageRed;
        [SerializeField] private AudioClip triageYellow;
        [SerializeField] private AudioClip triageGreen;
        [SerializeField] private AudioClip triageBlack;
        [SerializeField] private AudioClip scanVictim;

        [Header("=== EFFETS SONORES - VICTIMES ===")]
        [SerializeField] private AudioClip[] victimMoans;
        [SerializeField] private AudioClip[] victimCries;
        [SerializeField] private AudioClip[] victimHelp;
        [SerializeField] private AudioClip victimDetected;
        [SerializeField] private AudioClip victimHeartbeat;
        [SerializeField] private AudioClip victimFlatline;

        [Header("=== EFFETS SONORES - AMBULANCE ===")]
        [SerializeField] private AudioClip ambulanceSiren;
        [SerializeField] private AudioClip ambulanceArrival;
        [SerializeField] private AudioClip ambulanceDeparture;
        [SerializeField] private AudioClip stretcherMove;
        [SerializeField] private AudioClip doorOpen;
        [SerializeField] private AudioClip doorClose;

        [Header("=== EFFETS SONORES - NAVIGATION ===")]
        [SerializeField] private AudioClip waypointReached;
        [SerializeField] private AudioClip destinationReached;
        [SerializeField] private AudioClip turnLeft;
        [SerializeField] private AudioClip turnRight;
        [SerializeField] private AudioClip goStraight;

        [Header("=== EFFETS SONORES - COMMANDES VOCALES ===")]
        [SerializeField] private AudioClip voiceCommandRecognized;
        [SerializeField] private AudioClip voiceCommandError;
        [SerializeField] private AudioClip voiceListening;

        [Header("=== EFFETS SONORES - JOUEUR ===")]
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private AudioClip[] footstepsRun;
        [SerializeField] private AudioClip breathingNormal;
        [SerializeField] private AudioClip breathingHeavy;

        [Header("=== PARAMÈTRES ===")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ambianceVolume = 0.3f;
        [SerializeField] private float voiceVolume = 1f;
        [SerializeField] private float crossfadeDuration = 2f;

        // État
        private AudioClip currentMusic;
        private Coroutine musicFadeCoroutine;
        private Dictionary<string, float> cooldowns = new Dictionary<string, float>();

        #region Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadVolumeSettings();
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (ambianceSource == null)
            {
                ambianceSource = gameObject.AddComponent<AudioSource>();
                ambianceSource.loop = true;
                ambianceSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }

            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
            }
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            ambianceVolume = PlayerPrefs.GetFloat("AmbianceVolume", 0.3f);
            voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);

            ApplyVolumeSettings();
        }
        #endregion

        #region Volume Control
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            ApplyVolumeSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            ApplyVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            ApplyVolumeSettings();
        }

        public void SetAmbianceVolume(float volume)
        {
            ambianceVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("AmbianceVolume", ambianceVolume);
            ApplyVolumeSettings();
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
            ApplyVolumeSettings();
        }

        private void ApplyVolumeSettings()
        {
            if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
            if (ambianceSource != null) ambianceSource.volume = ambianceVolume * masterVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
            if (voiceSource != null) voiceSource.volume = voiceVolume * masterVolume;
            if (uiSource != null) uiSource.volume = sfxVolume * masterVolume;
        }
        #endregion

        #region Music
        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic);
        }

        public void PlayTensionMusic()
        {
            PlayMusic(tensionMusic);
        }

        public void PlayVictoryMusic()
        {
            PlayMusic(victoryMusic, false);
        }

        public void PlayDefeatMusic()
        {
            PlayMusic(defeatMusic, false);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || clip == currentMusic) return;

            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip, loop));
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
        {
            float startVolume = musicSource.volume;
            
            // Fade out
            float elapsed = 0f;
            while (elapsed < crossfadeDuration / 2)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / (crossfadeDuration / 2));
                yield return null;
            }

            // Change clip
            musicSource.clip = newClip;
            musicSource.loop = loop;
            currentMusic = newClip;
            musicSource.Play();

            // Fade in
            elapsed = 0f;
            float targetVolume = musicVolume * masterVolume;
            while (elapsed < crossfadeDuration / 2)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0, targetVolume, elapsed / (crossfadeDuration / 2));
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (fadeOut)
            {
                if (musicFadeCoroutine != null)
                    StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
                currentMusic = null;
            }
        }

        private IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;
            
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / crossfadeDuration);
                yield return null;
            }

            musicSource.Stop();
            currentMusic = null;
        }
        #endregion

        #region Ambiance
        public void PlayCityAmbiance()
        {
            PlayAmbiance(cityAmbiance);
        }

        public void PlayEmergencyAmbiance()
        {
            PlayAmbiance(emergencyAmbiance);
        }

        public void PlayAmbiance(AudioClip clip)
        {
            if (clip == null) return;
            
            ambianceSource.clip = clip;
            ambianceSource.Play();
        }

        public void StopAmbiance()
        {
            ambianceSource.Stop();
        }

        public void SetAmbianceForWeather(Core.WeatherType weather)
        {
            switch (weather)
            {
                case Core.WeatherType.Rain:
                case Core.WeatherType.Storm:
                    PlayAmbiance(rainAmbiance);
                    break;
                case Core.WeatherType.Snow:
                    PlayAmbiance(windAmbiance);
                    break;
                default:
                    PlayCityAmbiance();
                    break;
            }
        }
        #endregion

        #region UI Sounds
        public void PlayButtonClick()
        {
            PlayUI(buttonClick);
        }

        public void PlayButtonHover()
        {
            PlayUI(buttonHover, 0.5f);
        }

        public void PlayNotification()
        {
            PlayUI(notification);
        }

        public void PlayAlertCritical()
        {
            PlayUI(alertCritical);
        }

        public void PlayAlertWarning()
        {
            PlayUI(alertWarning);
        }

        public void PlayAlertInfo()
        {
            PlayUI(alertInfo);
        }

        public void PlayMenuOpen()
        {
            PlayUI(menuOpen);
        }

        public void PlayMenuClose()
        {
            PlayUI(menuClose);
        }

        private void PlayUI(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null || uiSource == null) return;
            uiSource.PlayOneShot(clip, volumeMultiplier);
        }
        #endregion

        #region Triage Sounds
        public void PlayTriageStart()
        {
            PlaySFX(triageStart);
        }

        public void PlayTriageConfirm()
        {
            PlaySFX(triageConfirm);
        }

        public void PlayTriageCategory(Core.StartCategory category)
        {
            AudioClip clip = category switch
            {
                Core.StartCategory.Red => triageRed,
                Core.StartCategory.Yellow => triageYellow,
                Core.StartCategory.Green => triageGreen,
                Core.StartCategory.Black => triageBlack,
                _ => triageConfirm
            };
            PlaySFX(clip);
        }

        public void PlayScanVictim()
        {
            PlaySFX(scanVictim);
        }
        #endregion

        #region Victim Sounds
        public void PlayVictimMoan()
        {
            PlayRandomSFX(victimMoans);
        }

        public void PlayVictimCry()
        {
            PlayRandomSFX(victimCries);
        }

        public void PlayVictimHelp()
        {
            PlayRandomSFX(victimHelp);
        }

        public void PlayVictimDetected()
        {
            PlaySFX(victimDetected);
        }

        public void PlayHeartbeat()
        {
            PlaySFX(victimHeartbeat);
        }

        public void PlayFlatline()
        {
            PlaySFX(victimFlatline);
        }

        public void PlayVictimSoundAt(Vector3 position, AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume * masterVolume);
        }
        #endregion

        #region Ambulance Sounds
        public void PlayAmbulanceSiren()
        {
            PlaySFXWithCooldown("siren", ambulanceSiren, 5f);
        }

        public void PlayAmbulanceArrival()
        {
            PlaySFX(ambulanceArrival);
        }

        public void PlayAmbulanceDeparture()
        {
            PlaySFX(ambulanceDeparture);
        }

        public void PlayStretcher()
        {
            PlaySFX(stretcherMove);
        }

        public void PlayDoorOpen()
        {
            PlaySFX(doorOpen);
        }

        public void PlayDoorClose()
        {
            PlaySFX(doorClose);
        }
        #endregion

        #region Navigation Sounds
        public void PlayWaypointReached()
        {
            PlaySFX(waypointReached);
        }

        public void PlayDestinationReached()
        {
            PlaySFX(destinationReached);
        }

        public void PlayNavigationDirection(Navigation.NavigationDirection direction)
        {
            AudioClip clip = direction switch
            {
                Navigation.NavigationDirection.Left => turnLeft,
                Navigation.NavigationDirection.SharpLeft => turnLeft,
                Navigation.NavigationDirection.Right => turnRight,
                Navigation.NavigationDirection.SharpRight => turnRight,
                _ => goStraight
            };
            PlaySFXWithCooldown("nav_" + direction, clip, 3f);
        }
        #endregion

        #region Voice Command Sounds
        public void PlayVoiceRecognized()
        {
            PlaySFX(voiceCommandRecognized);
        }

        public void PlayVoiceError()
        {
            PlaySFX(voiceCommandError);
        }

        public void PlayVoiceListening()
        {
            PlaySFX(voiceListening);
        }
        #endregion

        #region Player Sounds
        public void PlayFootstep()
        {
            PlayRandomSFXWithCooldown("footstep", footsteps, 0.3f, 0.7f);
        }

        public void PlayFootstepRun()
        {
            PlayRandomSFXWithCooldown("footstep_run", footstepsRun, 0.2f, 0.8f);
        }
        #endregion

        #region Core SFX Methods
        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volumeMultiplier);
        }

        public void PlayRandomSFX(AudioClip[] clips, float volumeMultiplier = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            PlaySFX(clip, volumeMultiplier);
        }

        public void PlaySFXWithCooldown(string id, AudioClip clip, float cooldown, float volumeMultiplier = 1f)
        {
            if (clip == null) return;
            
            if (cooldowns.TryGetValue(id, out float lastPlayed))
            {
                if (Time.time - lastPlayed < cooldown) return;
            }

            cooldowns[id] = Time.time;
            PlaySFX(clip, volumeMultiplier);
        }

        public void PlayRandomSFXWithCooldown(string id, AudioClip[] clips, float cooldown, float volumeMultiplier = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            
            if (cooldowns.TryGetValue(id, out float lastPlayed))
            {
                if (Time.time - lastPlayed < cooldown) return;
            }

            cooldowns[id] = Time.time;
            PlayRandomSFX(clips, volumeMultiplier);
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeMultiplier);
        }
        #endregion

        #region Voice Announcements
        public void SpeakAnnouncement(AudioClip announcement)
        {
            if (announcement == null || voiceSource == null) return;

            // Réduire temporairement le volume des autres sons
            StartCoroutine(PlayAnnouncementWithDucking(announcement));
        }

        private IEnumerator PlayAnnouncementWithDucking(AudioClip announcement)
        {
            // Duck other audio
            float originalMusicVolume = musicSource.volume;
            float originalAmbianceVolume = ambianceSource.volume;

            musicSource.volume *= 0.3f;
            ambianceSource.volume *= 0.3f;

            voiceSource.PlayOneShot(announcement);

            yield return new WaitForSeconds(announcement.length);

            // Restore volumes
            musicSource.volume = originalMusicVolume;
            ambianceSource.volume = originalAmbianceVolume;
        }
        #endregion
    }
}
