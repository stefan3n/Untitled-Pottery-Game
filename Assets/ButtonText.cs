using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WheelButtonController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private AudioSource audioSource;
    private bool isWheelSpinning = false;

    private const string START_TEXT = "Start Wheel";
    private const string STOP_TEXT = "Stop Wheel";

    void Start()
    {
        if (buttonText != null)
        {
            buttonText.text = START_TEXT;
        }
    }

    public void ToggleWheelState()
    {
        isWheelSpinning = !isWheelSpinning;

        if (buttonText != null)
        {
            buttonText.text = isWheelSpinning ? STOP_TEXT : START_TEXT;
        }
        if (isWheelSpinning)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
}