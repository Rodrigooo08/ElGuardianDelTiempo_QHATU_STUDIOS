using UnityEngine;

public class BGMManager : MonoBehaviour
{
    // Patrón Singleton para mantener una sola instancia en todo el juego
    public static BGMManager Instance { get; private set; }

    [Header("Audio")]
    public AudioClip backgroundMusic; // Arrastra aquí tu composición
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        // Verificar si ya existe otra instancia para evitar duplicados
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Hace que este objeto no se destruya al cargar una nueva escena
        DontDestroyOnLoad(gameObject);

        // Configurar o añadir el AudioSource automáticamente
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigureAudioSource();
    }

    void ConfigureAudioSource()
    {
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;          // Activa el loop infinito
        audioSource.playOnAwake = true;   // Empieza a sonar al iniciar
        audioSource.volume = volume;

        if (backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Métodos útiles por si necesitas cambiar el volumen o la música desde otro script
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
