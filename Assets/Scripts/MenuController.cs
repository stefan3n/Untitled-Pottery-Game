using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject MenuCanvas;       // Meniul principal
    public GameObject SettingsCanvas;   // Meniul de setari
    public GameObject SmallMenuCanvas;  // Meniul 'mod de joc' 
    public GameObject ExitCanvas;       // Meniul de confirmare iesire 
    public GameObject InstructionsCanvas; // Meniul de instructiuni

    private bool settingsActive = false;

    // Pentru quit din meniul principal (deschide ExitCanvas)
    public void QuitGame()
    {
        MenuCanvas.SetActive(false);
        ExitCanvas.SetActive(true);
    }

    // --- Pentru ExitCanvas ---

    // Apelat de butonul YES
    public void ConfirmQuit()
    {
        Debug.Log("Quit confirmed - Closing Application");
        Application.Quit();
    }

    // Apelat de butonul NO
    public void CancelQuit()
    {
        ExitCanvas.SetActive(false);
        MenuCanvas.SetActive(true);
    }

    // Pentru setari
    public void OpenSettings()
    {
        settingsActive = !settingsActive;
        MenuCanvas.SetActive(!settingsActive);
        SettingsCanvas.SetActive(settingsActive);
    }

    // --- Pentru meniul 'mod de joc' ---

    // Pentru SmallMenu 
    public void OpenSmallMenu()
    {
        MenuCanvas.SetActive(false);
        SmallMenuCanvas.SetActive(true);
    }

    // Pentru quit din meniul 'mod de joc'
    public void BackToMainMenu()
    {
        SmallMenuCanvas.SetActive(false);
        MenuCanvas.SetActive(true);
    }

    public void StartExplore()
    {
        Debug.Log("Explore mode started");
        SceneManager.LoadScene("ExploreMode");
    }

    public void StartJourney()
    {
        Debug.Log("Journey mode started");
        SceneManager.LoadScene("JourneyMode");
    }

    public void OpenInstructions()
    {
        SmallMenuCanvas.SetActive(false);
        InstructionsCanvas.SetActive(true);
    }

    // Apelat de butonul Exit din InstructionsCanvas
    public void CloseInstructions()
    {
        InstructionsCanvas.SetActive(false);
        SmallMenuCanvas.SetActive(true);
    }
}