using UnityEngine;
using UnityEngine.SceneManagement;
using AirshipsAndAirIslands.Events;

namespace AirshipsAndAirIslands.Audio
{
    /// <summary>
    /// Centralized audio playback helper. Attach one instance in a persistent scene (e.g., a bootstrapper or an Audio GameObject).
    /// Exposes simple Play methods for common UI/game sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip enemyEncounterClip;
        [SerializeField] private AudioClip healClip;
        [SerializeField] private AudioClip purchaseClip;
        [SerializeField] private AudioClip buildClip;

        [Header("Music Clips")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip cityMusic;
        [SerializeField] private AudioClip mapMusic;

        [Header("Auto-assign")]
        [Tooltip("If enabled, the AudioManager will add a default click sound behaviour to Buttons that don't already have a custom sound marker.")]
        [SerializeField] private bool autoAssignUIButtonSound = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // Persist the AudioManager across scene loads so one instance services the whole game
            DontDestroyOnLoad(gameObject);

            // Listen for scene loads so we can auto-assign click SFX to buttons created in new scenes
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Play appropriate music for the scene
            PlayMusicForScene(scene.name);
            
            // Re-run assignment to pick up buttons in newly loaded scenes
            AssignClickToButtons();
        }

        private void PlayMusicForScene(string sceneName)
        {
            AudioClip targetClip = null;
            string lowerScene = sceneName.ToLower();

            // Determine which music to play
            if (lowerScene.Contains("mainmenu"))
            {
                targetClip = mainMenuMusic;
            }
            else if (lowerScene.Contains("battle"))
            {
                // For battle scenes, check encounter type to decide between battle and boss music
                targetClip = (GameState.Instance != null && GameState.Instance.CurrentEncounterType == BattleEncounterType.Encounter3)
                    ? bossMusic
                    : battleMusic;
            }
            else if (lowerScene.Contains("city"))
            {
                targetClip = cityMusic;
            }
            else if (lowerScene.Contains("shiprooms") || lowerScene.Contains("map") || lowerScene.Contains("shiprooms_new"))
            {
                // Map and ShipRooms use the same music (mapMusic)
                targetClip = mapMusic;
            }

            Debug.Log($"PlayMusicForScene: Scene={sceneName}, targetClip={targetClip?.name}, currentClip={musicSource.clip?.name}");

            // Only switch if we're changing to a different clip
            if (targetClip != null && musicSource.clip != targetClip)
            {
                Debug.Log($"Switching music from {musicSource.clip?.name} to {targetClip.name}");
                musicSource.Stop();
                musicSource.clip = targetClip;
                musicSource.Play();
            }
            else if (targetClip == null)
            {
                Debug.LogWarning($"PlayMusicForScene: No music clip found for scene '{sceneName}'");
            }
        }

        private void Start()
        {
            if (autoAssignUIButtonSound)
            {
                AssignClickToButtons();
            }
        }

        private void AssignClickToButtons()
        {
            try
            {
                #if UNITY_2023_1_OR_NEWER
                var buttons = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsSortMode.None);
                #else
                var buttons = FindObjectsOfType<UnityEngine.UI.Button>();
                #endif
                foreach (var btn in buttons)
                {
                    if (btn == null) continue;

                    // Skip if already has explicit custom sound marker or already has the helper
                    if (btn.GetComponent<UIButtonHasCustomSound>() != null) continue;
                    if (btn.GetComponent<UIButtonSound>() != null) continue;

                    // Inspect persistent onClick listeners for obvious audio methods
                    var evt = btn.onClick;
                    var persistentCount = evt.GetPersistentEventCount();
                    var foundAudioListener = false;
                    for (var i = 0; i < persistentCount; i++)
                    {
                        var method = evt.GetPersistentMethodName(i);
                        var target = evt.GetPersistentTarget(i);
                        if (!string.IsNullOrEmpty(method))
                        {
                            // If method name indicates it's playing a sound, skip
                            if (method.StartsWith("Play") || method.Contains("Sound") || method.Contains("Audio") || method.Contains("Heal") || method.Contains("Purchase") || method.Contains("Build"))
                            {
                                foundAudioListener = true;
                                break;
                            }
                        }

                        if (target != null)
                        {
                            var tname = target.GetType().Name;
                            if (tname.Contains("Audio") || tname.Contains("Sound"))
                            {
                                foundAudioListener = true;
                                break;
                            }
                        }
                    }

                    if (foundAudioListener) continue;

                    // Add default click helper
                    btn.gameObject.AddComponent<UIButtonSound>();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"AudioManager.AssignClickToButtons failed: {ex.Message}");
            }
        }

        public void PlayClick()
        {
            PlayOneShot(clickClip);
        }

        public void PlayEnemyEncounter()
        {
            PlayOneShot(enemyEncounterClip);
        }

        public void PlayHeal()
        {
            PlayOneShot(healClip);
        }

        public void PlayPurchase()
        {
            PlayOneShot(purchaseClip);
        }

        public void PlayBuild()
        {
            PlayOneShot(buildClip);
        }

        /// <summary>
        /// Plays the appropriate battle music based on the current encounter type.
        /// Boss music plays for Encounter3 (Enemy 3), regular battle music for others.
        /// </summary>
        public void PlayBattleMusic()
        {
            AudioClip targetClip = (GameState.Instance.CurrentEncounterType == BattleEncounterType.Encounter3) 
                ? bossMusic 
                : battleMusic;

            if (targetClip != null && musicSource.clip != targetClip)
            {
                musicSource.Stop();
                musicSource.clip = targetClip;
                musicSource.Play();
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Sets the volume for sound effects (0.0 to 1.0).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>
        /// Gets the current volume for sound effects (0.0 to 1.0).
        /// </summary>
        public float GetSFXVolume()
        {
            return sfxSource != null ? sfxSource.volume : 1f;
        }

        /// <summary>
        /// Sets the volume for music (0.0 to 1.0).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (musicSource != null)
            {
                musicSource.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>
        /// Gets the current volume for music (0.0 to 1.0).
        /// </summary>
        public float GetMusicVolume()
        {
            return musicSource != null ? musicSource.volume : 1f;
        }
    }
}
