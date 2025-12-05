using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject MenuCanvas;
    public GameObject SettingsCanvas;

    private bool settingsActive = false;

    public void StartGame()
    {
        SceneManager.LoadScene("Lab09Scene"); 
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Lab09Scene");
    }

    public void QuitGame()
    {
        Debug.Log("Quit game pressed");
        Application.Quit();
    }

    public void OpenSettings()
    {
        settingsActive = !settingsActive;

        MenuCanvas.SetActive(!settingsActive);
        SettingsCanvas.SetActive(settingsActive);

        Debug.Log("Settings active: " + settingsActive);
    }
}
