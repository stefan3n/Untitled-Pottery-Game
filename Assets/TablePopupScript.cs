using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class TablePopupScript : MonoBehaviour
{
    [Header("Popup UI")]
    [SerializeField] private GameObject popupRoot;

    [Header("Optional")]
    [Tooltip("Automatically hide popup when the object is activated (trigger).")]
    [SerializeField] private bool hideOnActivate = false;

    private XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    private void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        // interactable.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
        // interactable.activated.RemoveListener(OnActivated);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (popupRoot != null)
            popupRoot.SetActive(true);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (hideOnActivate && popupRoot != null)
            popupRoot.SetActive(false);
    }
}