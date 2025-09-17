using UnityEngine;
using UnityEngine.SceneManagement;

public class VRSceneSwitcher : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        // Optional: Fade out or play transition animation here
        SceneManager.LoadScene(sceneName);    
    }
}