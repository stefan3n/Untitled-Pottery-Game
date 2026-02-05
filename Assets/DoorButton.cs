using TMPro;
using UnityEngine;

public class DoorButtonController : MonoBehaviour
{
    [Header("UI & Movement Settings")] 
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Audio Settings")] 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;  
    [SerializeField] private AudioClip closeSound;  

    private const string START_TEXT = "Open door";
    private const string STOP_TEXT = "Close door";
    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        if (buttonText != null)
        {
            buttonText.text = START_TEXT;
        }

        if (doorHinge != null)
        {
            closedRotation = doorHinge.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void DoorIsOpen()
    {
        isOpen = !isOpen;
        buttonText.text = isOpen ? START_TEXT : STOP_TEXT;
    }

    public void ToggleDoorState()
    {
        if (isAnimating || doorHinge == null) return;

        isOpen = !isOpen;

        if (audioSource != null)
        {
            if (isOpen)
            {
                if (openSound != null) audioSource.PlayOneShot(openSound);
            }
            else
            {
                if (closeSound != null) audioSource.PlayOneShot(closeSound);
            }
        }

        if (buttonText != null)
        {
            buttonText.text = isOpen ? STOP_TEXT : START_TEXT;
        }

        StopAllCoroutines();
        StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
    }

    private void OnDisable()
    {
        isAnimating = false;

        if (doorHinge != null)
        {
            Quaternion targetRotation = isOpen
                ? closedRotation * Quaternion.Euler(0, openAngle, 0)
                : closedRotation;
            doorHinge.localRotation = targetRotation;
        }
    }

    private System.Collections.IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isAnimating = true;
        float elapsed = 0f;
        Quaternion initialRotation = doorHinge.localRotation;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            doorHinge.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return null;
        }

        doorHinge.localRotation = targetRotation;
        isAnimating = false;
    }
}