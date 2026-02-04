using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetManager : MonoBehaviour
{
    [Header("Setup")]
    public Potter playerPot;
    public Potter targetPotDisplay;

    [Header("UI")]
    public GameObject resultPanel; // Canvas-ul cu text
    public TextMeshProUGUI resultText; // Textul

    [Header("Level Data")]
    public LevelData currentLevel;

    [Header("Input")]
    [Tooltip("Butonul de pe controller pentru a da Restart la final (ex: Trigger sau A)")]
    public InputActionProperty restartAction;

    [Header("Difficulty Settings")]
    [Tooltip("90% => 100% score")]
    [Range(0.8f, 1.0f)]
    public float completionThreshold = 0.90f;

    // Raza default a vasului 
    private const float DEFAULT_START_RADIUS = 0.3f;

    // Stare interna: false = jucam, true = vedem rezultatul
    private bool isShowingResult = false;

    private void OnEnable()
    {
        if (restartAction.action != null) restartAction.action.Enable();
    }

    private void OnDisable()
    {
        if (restartAction.action != null) restartAction.action.Disable();
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

        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
    }

    public void LoadLevel(LevelData level)
    {
        currentLevel = level;

        if (targetPotDisplay == null) return;

        float[] heights = targetPotDisplay.GetHeightsData();

        if (heights == null || heights.Length != level.targetRadii.Length)
        {
            heights = new float[level.targetRadii.Length];
            for (int i = 0; i < heights.Length; i++)
            {
                heights[i] = i * targetPotDisplay.baseRingHeight;
            }
        }

        // Incarcam datele (false la final pentru geometry lock)
        targetPotDisplay.LoadPotData(level.targetRadii, heights, null, null, false);
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

        // --- FIX AICI: Am inlocuit FullReset() cu ResetPot() ---
        if (playerPot != null) playerPot.ResetPot();

        isShowingResult = false;
        Debug.Log("Game restarted.");
    }

    public float CalculateAccuracy()
    {
        if (playerPot == null || currentLevel == null) return 0f;

        float[] playerRadii = playerPot.ringsRadius;
        float[] targetRadii = currentLevel.targetRadii;

        int minLength = Mathf.Min(playerRadii.Length, targetRadii.Length);
        int maxLength = Mathf.Max(playerRadii.Length, targetRadii.Length);

        float totalInitialDiff = 0f;
        float totalCurrentDiff = 0f;

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

        if (score >= completionThreshold)
        {
            return 1.0f;
        }

        return score;
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

            if (restartAction.action != null && restartAction.action.WasPressedThisFrame())
            {
                RestartGame();
            }
        }
    }
}