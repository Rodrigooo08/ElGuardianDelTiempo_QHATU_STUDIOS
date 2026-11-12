using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManagerDialogo : MonoBehaviour
{
    [Header("Secuencia de Imagenes")]
    public Sprite[] storyImages;
    public string[] narrationTexts; // Frases para cada imagen

    [Header("Elementos a usar")]
    public Image backgroundImage; //componente imagen de fondo
    public TextMeshProUGUI narrationTMP; //componente Txt para mostrar narrativa

    [Header("Configuración")]
    public float imageDuration = 5f;
    public string nextSceneName = "Level01";

    private int currentIndex = 0;
    private float timer = 0f;


    void Start()
    {
        if (storyImages.Length > 0)
            backgroundImage.sprite = storyImages[0];

        if (narrationTexts.Length > 0)
            narrationTMP.text = narrationTexts[0];
    }

    void Update()
    {
        if (storyImages.Length == 0 || backgroundImage == null) return;

        timer += Time.deltaTime;

        if (timer >= imageDuration)
        {
            currentIndex++;
            timer = 0f;

            if (currentIndex < storyImages.Length)
            {
                //actualizacion de imagen
                backgroundImage.sprite = storyImages[currentIndex];
                //actualizacion de texto
                if (currentIndex < narrationTexts.Length)
                    narrationTMP.text = narrationTexts[currentIndex];
                else
                    narrationTMP.text = "";
            }
            else
            {
                LoadNextScene();
            }
        }
    }
    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void SkipScene()
    {
        LoadNextScene();
    }

}
