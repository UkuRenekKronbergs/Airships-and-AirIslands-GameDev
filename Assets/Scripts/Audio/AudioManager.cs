using UnityEngine;
using UnityEngine.SceneManagement;

namespace AirshipsAndAirIslands.Audio
{
    /// <summary>
    /// Centralized audio playback helper. Attach one instance in a persistent scene (e.g., a bootstrapper or an Audio GameObject).
    /// Exposes simple Play methods for common UI/game sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip enemyEncounterClip;
        [SerializeField] private AudioClip healClip;
        [SerializeField] private AudioClip purchaseClip;
        [SerializeField] private AudioClip buildClip;

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
            // Re-run assignment to pick up buttons in newly loaded scenes
            AssignClickToButtons();
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

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }
    }
}
