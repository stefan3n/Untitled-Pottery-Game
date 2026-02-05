using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TargetManager : MonoBehaviour
{
    [Header("Setup References")]
    public Potter playerPot;
    public Potter targetPotDisplay;

    [Header("UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Game Progression")]
    public List<LevelData> gameLevels; 
    public string menuSceneName = "MenuScene"; 
    [Range(0.1f, 1.0f)] public float passingAccuracy = 0.75f; 

    [Header("Input")]
    public InputActionProperty actionButton;

    [Header("Difficulty Settings (Algorithm)")]
    [Tooltip("Cât poți greși (în metri) fără penalizare. 0.04 = 4cm toleranță.")]
    public float toleranceMargin = 0.04f;

    [Tooltip("Eroarea medie la care scorul devine 0. 0.15 = 15cm eroare medie.")]
    public float maxAcceptableAvgError = 0.15f;

    // State
    private int currentLevelIndex = 0;
    private bool isShowingResult = false;
    private bool hasPassedCurrentLevel = false;

    private void OnEnable()
    {
        if (actionButton.action != null) actionButton.action.Enable();
    }

    private void OnDisable()
    {
        if (actionButton.action != null) actionButton.action.Disable();
    }

    private void Start()
    {
        if (targetPotDisplay != null)
        {
            targetPotDisplay.isStatic = true;
            if (targetPotDisplay.GetComponent<Rigidbody>())
                targetPotDisplay.GetComponent<Rigidbody>().isKinematic = true;
        }

        if (resultPanel != null) resultPanel.SetActive(false);

        StartLevel(0);
    }

    private void StartLevel(int index)
    {
        if (gameLevels == null || gameLevels.Count == 0) return;

        currentLevelIndex = index;
        isShowingResult = false;
        hasPassedCurrentLevel = false;
        if (resultPanel) resultPanel.SetActive(false);

        if (playerPot) playerPot.FullReset();

        LoadTargetPotData(gameLevels[index]);

        Debug.Log($"START LEVEL {index + 1}: {gameLevels[index].levelName}");
    }

    private void LoadTargetPotData(LevelData level)
    {
        if (targetPotDisplay == null) return;

        float[] heights = level.targetHeights;

        if (heights == null || heights.Length != level.targetRadii.Length)
        {
            heights = new float[level.targetRadii.Length];
            for (int i = 0; i < heights.Length; i++)
                heights[i] = i * targetPotDisplay.baseRingHeight;
        }

        targetPotDisplay.LoadPotData(level.targetRadii, heights, null, null, true);
    }


    public void EvaluateAndShowResult()
    {
        if (isShowingResult) return; 

        float accuracy = CalculateAccuracy();
        hasPassedCurrentLevel = accuracy >= passingAccuracy;

        if (resultPanel != null && resultText != null)
        {
            resultPanel.SetActive(true);

            string color = hasPassedCurrentLevel ? "green" : "red";
            string status = hasPassedCurrentLevel ? "LEVEL PASSED!" : "TRY AGAIN";
            string instruction = hasPassedCurrentLevel ? "Press Trigger for Next Level" : "Press Trigger to Restart";

            if (hasPassedCurrentLevel && currentLevelIndex >= gameLevels.Count - 1)
            {
                status = "GAME COMPLETED!";
                instruction = "Press Trigger for Menu";
            }

            resultText.text = $"Accuracy: {(accuracy * 100):F0}%\n" +
                              $"<color={color}>{status}</color>\n" +
                              $"<size=60%>{instruction}</size>";
        }

        isShowingResult = true;
    }


    public float CalculateAccuracy()
    {
        if (playerPot == null || gameLevels.Count == 0) return 0f;

        float[] targetRadii = gameLevels[currentLevelIndex].targetRadii;
        float[] playerRadii = playerPot.ringsRadius;

        int minLength = Mathf.Min(playerRadii.Length, targetRadii.Length);
        int maxLength = Mathf.Max(playerRadii.Length, targetRadii.Length);

        float totalError = 0f;

        for (int i = 0; i < minLength; i++)
        {
            float diff = Mathf.Abs(playerRadii[i] - targetRadii[i]);

            float adjustedDiff = Mathf.Max(0f, diff - toleranceMargin);

            totalError += adjustedDiff;
        }

        if (maxLength > minLength)
        {
            int extraRings = maxLength - minLength;
            totalError += extraRings * 0.01f;
        }

        float avgError = totalError / maxLength;

        float score = 1.0f - (avgError / maxAcceptableAvgError);

        return Mathf.Clamp01(score);
    }


    private void Update()
    {
        if (isShowingResult)
        {
            bool inputDetected = false;

     
            if (actionButton.action != null && actionButton.action.WasPressedThisFrame()) inputDetected = true;
 
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) inputDetected = true;

            if (inputDetected)
            {
                HandlePostGameInput();
            }
        }
    }

    private void HandlePostGameInput()
    {
        if (hasPassedCurrentLevel)
        {
            int nextIndex = currentLevelIndex + 1;

            if (nextIndex < gameLevels.Count)
            {
                StartLevel(nextIndex);
            }
            else
            {
                Debug.Log("Joc terminat! Încărcare meniu...");
                SceneManager.LoadScene(menuSceneName);
            }
        }
        else
        {
            Debug.Log("Restart Level...");
            StartLevel(currentLevelIndex);
        }
    }
}