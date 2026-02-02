using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetManager : MonoBehaviour
{
    [Header("Setup")]
    public Potter playerPot;
    public Potter targetPotDisplay;

    [Header("UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Level Data")]
    public LevelData currentLevel;

    private bool isShowingResult = false;

    [Header("Difficulty Settings")]
    [Tooltip("Cat de mult poti gresi (in metri) si sa fie considerat perfect.")]
    public float toleranceMargin = 0.03f; // 3cm toleranta

    private void Start()
    {
        if (targetPotDisplay != null)
        {
            targetPotDisplay.isStatic = true;
            if (targetPotDisplay.GetComponent<Rigidbody>())
                targetPotDisplay.GetComponent<Rigidbody>().isKinematic = true;
        }

        if (resultPanel != null) resultPanel.SetActive(false);

        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
    }

    public void LoadLevel(LevelData level)
    {
        currentLevel = level;

        if (targetPotDisplay == null) return;

        targetPotDisplay.LoadPotData(level.targetRadii, level.targetHeights);
        targetPotDisplay.GenerateMesh();
    }

    public void EvaluateAndShowResult()
    {
        if (isShowingResult) return;

        float accuracy = CalculateAccuracy();

        if (resultPanel != null && resultText != null)
        {
            resultPanel.SetActive(true);
            resultText.text = $"Accuracy: {(accuracy * 100):F0}%\n<size=70%>- press any key to restart -</size>";
        }

        isShowingResult = true;
        Debug.Log($"Evaluation finished. Accuracy: {accuracy}");
    }

    public void RestartGame()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (playerPot != null) playerPot.ResetPot();

        isShowingResult = false;
        Debug.Log("Game restarted.");
    }

    public float CalculateAccuracy()
    {
        if (playerPot == null || currentLevel == null) return 0f;

        
        float totalError = 0f;
        int samplePoints = 0;

        float playerHeight = playerPot.GetTotalHeight();
        float targetHeight = targetPotDisplay.GetTotalHeight();

        float heightDiff = Mathf.Abs(playerHeight - targetHeight);
        totalError += heightDiff * 2.0f; 

        for (int i = 0; i < currentLevel.targetHeights.Length; i++)
        {
            float h = currentLevel.targetHeights[i];
            float targetR = currentLevel.targetRadii[i];

            float playerR = GetPlayerRadiusAtHeight(h);

            float diff = Mathf.Abs(playerR - targetR);

            if (diff <= toleranceMargin) diff = 0f;

            totalError += diff;
            samplePoints++;
        }

        float avgError = (samplePoints > 0) ? totalError / samplePoints : 1.0f;

        float score = 1.0f - (avgError / 0.15f);

        return Mathf.Clamp01(score);
    }

    private float GetPlayerRadiusAtHeight(float targetH)
    {
        if (targetH < playerPot.ringHeights[0])
            return playerPot.ringsRadius[0];

        if (targetH > playerPot.ringHeights[playerPot.ringsCount - 1])
            return playerPot.ringsRadius[playerPot.ringsCount - 1];

        for (int i = 0; i < playerPot.ringsCount - 1; i++)
        {
            float h1 = playerPot.ringHeights[i];
            float h2 = playerPot.ringHeights[i + 1];

            if (targetH >= h1 && targetH <= h2)
            {
                float t = (targetH - h1) / (h2 - h1);

                float r1 = playerPot.ringsRadius[i];
                float r2 = playerPot.ringsRadius[i + 1];

                return Mathf.Lerp(r1, r2, t);
            }
        }

        return 0f; 
    }

    private void Update()
    {
        if (isShowingResult)
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                RestartGame();
                return;
            }
        }
    }
}