using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManagerMusic02 : MonoBehaviour
{
    [Serializable]
    public struct SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
        [Range(0f, 1f)] public float volumeMultiplier;
    }

    // Patrón Singleton para mantener una sola instancia en todo el juego
    public static BGMManagerMusic02 Instance { get; private set; }

    [Header("Música por Escena")]
    [SerializeField] private SceneMusic[] sceneMusicList;
    [SerializeField] private AudioClip fallbackMusic; // Opcional: si una escena no está en el arreglo

    [Header("Configuración General")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [SerializeField] private bool stopIfNoClipAssigned = false;

    private AudioSource audioSource;
    private float currentClipVolumeMultiplier = 1f;

    private void Awake()
    {
        // Evitar duplicados entre escenas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar o añadir el AudioSource automáticamente
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigureAudioSource();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Reproducir la música correspondiente a la escena inicial
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void ConfigureAudioSource()
    {
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = masterVolume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        AudioClip targetClip = null;
        float multiplier = 1f;
        bool foundEntry = false;

        if (sceneMusicList != null)
        {
            for (int i = 0; i < sceneMusicList.Length; i++)
            {
                if (string.Equals(sceneMusicList[i].sceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    targetClip = sceneMusicList[i].musicClip;
                    multiplier = sceneMusicList[i].volumeMultiplier > 0f ? sceneMusicList[i].volumeMultiplier : 1f;
                    foundEntry = true;
                    break;
                }
            }
        }

        if (!foundEntry && fallbackMusic != null)
        {
            targetClip = fallbackMusic;
            multiplier = 1f;
        }

        currentClipVolumeMultiplier = multiplier;
        ApplyVolume();

        // Si ya está sonando exactamente el mismo clip, no lo reiniciamos desde cero
        if (audioSource.clip == targetClip && audioSource.isPlaying)
        {
            return;
        }

        if (targetClip != null)
        {
            audioSource.clip = targetClip;
            audioSource.Play();
        }
        else if (stopIfNoClipAssigned)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    public void SetVolume(float newVolume)
    {
        masterVolume = Mathf.Clamp01(newVolume);
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(masterVolume * currentClipVolumeMultiplier);
        }
    }
}
