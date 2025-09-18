using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [Header("Text Sequence")]
    [Tooltip("Assign text GameObjects to show in order.")]
    public GameObject[] textObjects;

    [Header("Audio")]
    public AudioSource lobbyAudio;

    [Header("Scene Settings")]
    [Tooltip("Name of the next scene to load after sequence.")]
    public string nextSceneName;

    [Header("Sequence Settings")]
    public float textInterval = 2f;
    public float fadeDuration = 2f;
    [Range(0f, 1f)]
    public float initialAlpha = 1f;

    public FadeController fadeController;

    void Awake()
    {
        // Hide all texts initially
        foreach (var go in textObjects)
            if (go) go.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(LobbySequenceRoutine());
    }

    IEnumerator LobbySequenceRoutine()
    {
        if (fadeController != null)
        {
            fadeController.FadeIn((textObjects.Length-1) * textInterval);
        }
        else
        {
            Debug.LogWarning("FadeController reference not set in LobbyController.");
        }
        for (int i = 0; i < textObjects.Length; i++)
        {
            // Show current text
            if (textObjects[i]) textObjects[i].SetActive(true);
            // Special: Fade audio between text 3 and 4 (index 2 and 3)
            if (i == 2 && lobbyAudio != null)
            {
                StartCoroutine(FadeAudioRoutine(lobbyAudio, lobbyAudio.volume, 0f, textInterval));
            }
            if (i == 3 && lobbyAudio != null)
            {
                lobbyAudio.volume = 0f;
            }
            // Hide previous text except last
            if (i > 0 && i < textObjects.Length)
            {
                if (textObjects[i - 1])
                    textObjects[i - 1].SetActive(false);
            }
            yield return new WaitForSeconds(textInterval);
        }
        // Keep last text active
        // Fade out quad using FadeController
        
        // Wait a bit before loading next scene
        yield return new WaitForSeconds(fadeDuration + 1f);
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator FadeAudioRoutine(AudioSource audio, float from, float to, float duration)
    {
        float t = 0f;
        audio.volume = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            audio.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        audio.volume = to;
    }
}