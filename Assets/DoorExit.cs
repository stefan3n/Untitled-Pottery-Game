using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit : MonoBehaviour
{
    public void GoToMenu()
    {
        Debug.Log("Exiting to Menu...");
        SceneManager.LoadScene("Menu");
    }
}