using TMPro;
using UnityEngine;

public class DoorButtonController : MonoBehaviour
{
    [Header("UI & Movement Settings")] // Optional: pentru organizare
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Audio Settings")] // <--- SECTIUNE NOUA
    [SerializeField] private AudioSource audioSource; // Referinta la Audio Source
    [SerializeField] private AudioClip openSound;     // Sunetul de deschidere
    [SerializeField] private AudioClip closeSound;    // Sunetul de inchidere

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

        // Verificare de siguranta (optional)
        if (audioSource == null)
        {
            // Incearca sa gaseasca AudioSource pe acelasi obiect daca nu a fost setat manual
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Aceasta functie pare a fi un toggle simplu de text, 
    // dar logica principala e in ToggleDoorState, asa ca nu punem sunet aici.
    public void DoorIsOpen()
    {
        isOpen = !isOpen;
        buttonText.text = isOpen ? START_TEXT : STOP_TEXT;
    }

    public void ToggleDoorState()
    {
        if (isAnimating || doorHinge == null) return;

        isOpen = !isOpen;

        // --- LOGICA DE SUNET ADAUGATA AICI ---
        if (audioSource != null)
        {
            if (isOpen)
            {
                // Daca usa s-a deschis (isOpen e true), redam sunetul de deschidere
                if (openSound != null) audioSource.PlayOneShot(openSound);
            }
            else
            {
                // Daca usa s-a inchis (isOpen e false), redam sunetul de inchidere
                if (closeSound != null) audioSource.PlayOneShot(closeSound);
            }
        }
        // -------------------------------------

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