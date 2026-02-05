using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public RectTransform containerRect;
    public TextMeshProUGUI messageText;


    [Header("Settings")]
    public float displayDuration = 2.0f;
    public float animationSpeed = 8.0f;

    private Coroutine currentRoutine;

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            containerRect.localScale = Vector3.zero;
        }
    }

    public void ShowNotification(string message)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        messageText.text = message;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
            containerRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        containerRect.localScale = Vector3.one;

        yield return new WaitForSeconds(displayDuration);

        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer);
            containerRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, timer);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        containerRect.localScale = Vector3.zero;
    }
}