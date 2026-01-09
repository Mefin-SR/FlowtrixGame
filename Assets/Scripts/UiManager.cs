using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }


    public void PauseGame()
    {
        Time.timeScale = 0f;  // Stops in-game time
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;  // Resumes normal time
    }
}
