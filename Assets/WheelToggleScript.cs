using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

[RequireComponent(typeof(XRBaseInteractable))]
public class WheelToggleScript : MonoBehaviour
{
    [Header("Target Wheel (RotatePot)")]
    [SerializeField] private RotatePot wheel;

    [Header("Label")]
    [SerializeField] private TMP_Text label;

    private XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        UpdateLabel();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("[WheelToggleScript] selectEntered fired!");

        if (wheel != null)
        {
            wheel.ToggleWheel();
            UpdateLabel();
        }
        else
        {
            Debug.LogWarning("[WheelToggleScript] Wheel reference is null.");
        }
    }

    private void UpdateLabel()
    {
        if (label == null || wheel == null)
            return;

        label.text = wheel.IsRunning ? "Stop" : "Start";
    }
}
