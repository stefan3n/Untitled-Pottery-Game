using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetManager : MonoBehaviour
{
    [Header("Setup")]
    public Potter playerPot;
    public Potter targetPotDisplay;

    [Header("UI")]
<<<<<<< Updated upstream
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
=======
    public GameObject resultPanel; 
    public TextMeshProUGUI resultText; 
>>>>>>> Stashed changes

    [Header("Level Data")]
    public LevelData currentLevel;

<<<<<<< Updated upstream
    private bool isShowingResult = false;

    [Header("Difficulty Settings")]
    [Tooltip("Cat de mult poti gresi (in metri) si sa fie considerat perfect.")]
    public float toleranceMargin = 0.03f; // 3cm toleranta
=======
    [Header("Input")]
    [Tooltip("Butonul de pe controller pentru a da Restart la final (ex: Trigger sau A)")]
    public InputActionProperty restartAction;

    [Header("Difficulty Settings")]
    [Tooltip("90% => 100% score")]
    [Range(0.8f, 1.0f)]
    public float completionThreshold = 0.90f;

    private const float DEFAULT_START_RADIUS = 0.3f;

    private bool isShowingResult = false;

    private void OnEnable()
    {
        if (restartAction.action != null) restartAction.action.Enable();
    }

    private void OnDisable()
    {
        if (restartAction.action != null) restartAction.action.Disable();
    }
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
        targetPotDisplay.LoadPotData(level.targetRadii, level.targetHeights);
        targetPotDisplay.GenerateMesh();
=======
        float[] heights = targetPotDisplay.GetHeightsData();

        if (heights == null || heights.Length != level.targetRadii.Length)
        {
            heights = new float[level.targetRadii.Length];
            for (int i = 0; i < heights.Length; i++)
            {
                heights[i] = i * targetPotDisplay.baseRingHeight;
            }
        }

        targetPotDisplay.LoadPotData(level.targetRadii, heights, null, null, false);
>>>>>>> Stashed changes
    }

    public void EvaluateAndShowResult()
    {
        if (isShowingResult) return;

        float accuracy = CalculateAccuracy();

        if (resultPanel != null && resultText != null)
        {
            resultPanel.SetActive(true);
            resultText.text = $"Accuracy: {(accuracy * 100):F0}%\n<size=70%>- press Trigger/A to restart -</size>";
        }

        isShowingResult = true;
        Debug.Log($"Evaluation finished. Accuracy: {accuracy}");
    }

    public void RestartGame()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
        if (playerPot != null) playerPot.ResetPot();

        isShowingResult = false;
        Debug.Log("Game restarted.");
    }

    public float CalculateAccuracy()
    {
        if (playerPot == null || currentLevel == null) return 0f;
<<<<<<< Updated upstream
=======

        float[] playerRadii = playerPot.ringsRadius;
        float[] targetRadii = currentLevel.targetRadii;

        int minLength = Mathf.Min(playerRadii.Length, targetRadii.Length);
        int maxLength = Mathf.Max(playerRadii.Length, targetRadii.Length);
>>>>>>> Stashed changes

        
        float totalError = 0f;
        int samplePoints = 0;

<<<<<<< Updated upstream
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
=======
        for (int i = 0; i < minLength; i++)
        {
            float targetR = targetRadii[i];
            float playerR = playerRadii[i];

            totalInitialDiff += Mathf.Abs(DEFAULT_START_RADIUS - targetR);
            totalCurrentDiff += Mathf.Abs(playerR - targetR);
        }

        if (maxLength > minLength)
        {
            float penaltyPerRing = 0.1f;
            totalCurrentDiff += (maxLength - minLength) * penaltyPerRing;
        }

        if (totalInitialDiff < 0.001f) totalInitialDiff = 1f;

        float errorRatio = totalCurrentDiff / totalInitialDiff;
        float score = 1.0f - errorRatio;

        score = Mathf.Clamp01(score);
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
=======

            if (restartAction.action != null && restartAction.action.WasPressedThisFrame())
            {
                RestartGame();
            }
>>>>>>> Stashed changes
        }
    }
}