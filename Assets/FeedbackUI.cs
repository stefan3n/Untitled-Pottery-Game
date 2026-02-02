using System.Collections;
using TMPro; 
using UnityEngine;

public class FeedbackUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI messageText;

    private Coroutine hideCoroutine;

    private void Start()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message, float duration = 5f)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.gameObject.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideTextAfterDelay(duration));
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        messageText.text = "";
        messageText.gameObject.SetActive(false);
        hideCoroutine = null;
    }
}