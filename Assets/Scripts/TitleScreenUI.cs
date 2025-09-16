using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenUI : MonoBehaviour
{

    public void OnStartButton()
    {
        Debug.Log("Start button pressed. Attempting to load GameScene...");
        SceneManager.LoadScene("GameScene"); // Replace with your main scene name
    }

    public void OnQuitButton()
    {
        Debug.Log("Quit button pressed. Closing application...");
        Application.Quit();
    }
}