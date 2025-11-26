using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Lab09Scene"); 
    }

    public void LoadGame()
    {
        Debug.Log("Load game pressed");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        Debug.Log("Open settings");
    }
}
