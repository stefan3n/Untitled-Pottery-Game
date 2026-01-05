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

    // Stare interna: false = jucam, true = vedem rezultatul
    private bool isShowingResult = false;

    private void Start()
    {
        if (targetPotDisplay != null)
        {
            targetPotDisplay.isStatic = true;
            if (targetPotDisplay.GetComponent<Rigidbody>())
                targetPotDisplay.GetComponent<Rigidbody>().isKinematic = true;
        }

        // Mesajul e ascund la inceput
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

        targetPotDisplay.SetRadiiData(level.targetRadii);
        targetPotDisplay.GenerateMesh();
    }

    public void EvaluateAndShowResult()
    {
        if (isShowingResult) return; 

        float accuracy = CalculateAccuracy();

        // Afisez UI-ul
        if (resultPanel != null && resultText != null)
        {
            resultPanel.SetActive(true);
            resultText.text = $"Accuracy: {(accuracy * 100):F0}%\n<size=70%>- press any key to restart -</size>";
        }

        isShowingResult = true;
        Debug.Log($"Evalation finished. Accuracy: {accuracy}");
    }

    public void RestartGame()
    {
        // Ascund UI-ul
        if (resultPanel != null) resultPanel.SetActive(false);

        // Resetez vasul
        if (playerPot != null) playerPot.ResetPot();

        isShowingResult = false;
        Debug.Log("Game restarted.");
    }

    [Header("Difficulty Settings")]
    [Tooltip("90% => 100% score")]
    [Range(0.8f, 1.0f)]
    public float completionThreshold = 0.90f; 

    // Raza default a vasului 
    private const float DEFAULT_START_RADIUS = 0.5f;

    public float CalculateAccuracy()
    {
        if (playerPot == null || currentLevel == null) return 0f;
        if (playerPot.ringsRadius.Length != currentLevel.targetRadii.Length) return 0f;

        float totalInitialDiff = 0f;
        float totalCurrentDiff = 0f;

        int ringCount = playerPot.ringsRadius.Length;

        for (int i = 0; i < ringCount; i++)
        {
            float targetR = currentLevel.targetRadii[i];
            float playerR = playerPot.ringsRadius[i];

            // Cat de gresit e vasul daca nu este modificat
            totalInitialDiff += Mathf.Abs(DEFAULT_START_RADIUS - targetR);

            // Cat de gresit e vasul acum
            totalCurrentDiff += Mathf.Abs(playerR - targetR);
        }

        if (totalInitialDiff < 0.001f) return 1.0f;

        float errorRatio = totalCurrentDiff / totalInitialDiff;

        float score = 1.0f - errorRatio;

        score = Mathf.Max(0f, score);

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

            // AICI ADAUGAM SI PENTRU ORICE TASTA DE PE CONTROLLER
        }
    }
}