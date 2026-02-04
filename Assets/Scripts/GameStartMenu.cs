using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject about;

    [Header("Main Menu Buttons")]
    public Button exploreButton; 
    public Button journeyButton; 
    public Button aboutButton;
    public Button quitButton;

    public List<Button> returnButtons;

    void Start()
    {
        EnableMainMenu();

        if (exploreButton) exploreButton.onClick.AddListener(StartExplore);
        if (journeyButton) journeyButton.onClick.AddListener(StartJourney);

        if (aboutButton) aboutButton.onClick.AddListener(EnableAbout);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);

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


    public void StartExplore()
    {
        HideAll();
        SceneTransitionManager.singleton.GoToSceneAsync("ExploreMode");
    }

    public void StartJourney()
    {
        HideAll();
        SceneTransitionManager.singleton.GoToSceneAsync("JourneyMode");
    }

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