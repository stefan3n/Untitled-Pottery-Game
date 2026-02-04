using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject about;
    // Am scos 'options' page pentru ca nu mai avem buton pentru ea

    [Header("Main Menu Buttons")]
    public Button exploreButton; // Fostul Start Button
    public Button journeyButton; // Fostul Option Button
    public Button aboutButton;
    public Button quitButton;

    public List<Button> returnButtons;

    void Start()
    {
        EnableMainMenu();

        // Conectam butoanele la noile functii
        if (exploreButton) exploreButton.onClick.AddListener(StartExplore);
        if (journeyButton) journeyButton.onClick.AddListener(StartJourney);

        if (aboutButton) aboutButton.onClick.AddListener(EnableAbout);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);

        // Butoanele de intoarcere (din About inapoi la Meniu)
        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Jocul s-a inchis (merge doar in Build, nu in Editor)");
    }

    // --- NOILE FUNCTII DE PORNIRE ---

    public void StartExplore()
    {
        HideAll();
        // Asigura-te ca scena se numeste exact "ExploreMode"
        SceneTransitionManager.singleton.GoToSceneAsync("ExploreMode");
    }

    public void StartJourney()
    {
        HideAll();
        // Asigura-te ca scena se numeste exact "JourneyMode"
        SceneTransitionManager.singleton.GoToSceneAsync("JourneyMode");
    }

    // --- GESTIONARE PAGINI (UI) ---

    public void HideAll()
    {
        mainMenu.SetActive(false);
        if (about) about.SetActive(false);
    }

    public void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        if (about) about.SetActive(false);
    }

    public void EnableAbout()
    {
        mainMenu.SetActive(false);
        if (about) about.SetActive(true);
    }
}