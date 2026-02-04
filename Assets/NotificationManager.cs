using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public RectTransform containerRect;
    public TextMeshProUGUI messageText;

    // Am sters referintele la Icon

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

    // Am simplificat functia, nu mai cere sprite
    public void ShowNotification(string message)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        // 1. Setup Text
        messageText.text = message;

        // (Nu mai setam iconita aici)

        // 2. Animatie Intrare
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

        // 3. Asteptare
        yield return new WaitForSeconds(displayDuration);

        // 4. Animatie Iesire
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