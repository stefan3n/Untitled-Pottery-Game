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
        interactable.hoverEntered.AddListener(a => Debug.Log("[WheelToggle] hover entered"));
        interactable.hoverExited.AddListener(a => Debug.Log("[WheelToggle] hover exited"));
        interactable.selectEntered.AddListener(a => Debug.Log("[WheelToggle] select entered"));
        interactable.selectExited.AddListener(a => Debug.Log("[WheelToggle] select exited"));
        interactable.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        interactable.activated.RemoveListener(OnActivated);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Debug.Log("[WheelToggleScript] activated fired!");

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