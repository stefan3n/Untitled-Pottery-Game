using UnityEngine;

public class LeftMenuFade : MonoBehaviour
{
    [Header("Assign Your Controller Transform (left hand)")]
    public Transform controllerTransform;
    
    [Header("Assign Your CanvasGroup component (on menu)")]
    public CanvasGroup menuCanvasGroup;

    [Header("Pot Settings")]
    public Transform potTransform;
    public float minDistanceToPot = 0.2f;

    [Header("Sensitivity")]
    [Range(0f, 1f)]
    public float activationThreshold = 0.3f;

    [Tooltip("The dot product value at which alpha becomes 1.0. Lower this to reach max opacity easier.")]
    [Range(0f, 1f)]
    public float fullOpacityThreshold = 0.8f;
    
    [Header("Interaction")]
    [Tooltip("The execution of interaction (clicks/hovers) is disabled below this alpha value.")]
    [Range(0f, 1f)]
    public float interactionAlphaThreshold = 0.5f;

    public float smoothSpeed = 10f;

    void Update()
    {
        if (!controllerTransform || !menuCanvasGroup) return;

        bool isTooCloseToPot = false;
        if (potTransform)
        {
            float distanceToPot = Vector3.Distance(controllerTransform.position, potTransform.position);
            if (distanceToPot < minDistanceToPot)
            {
                isTooCloseToPot = true;
            }
        }

        if (isTooCloseToPot)
        {
            menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, 0f, Time.deltaTime * smoothSpeed);
            UpdateInteraction();
            return;
        }

        Vector3 palmNormal = controllerTransform.right;

        float dotUp = Vector3.Dot(palmNormal, Vector3.up);
        

        float targetAlpha = 0f;

        if (dotUp > activationThreshold)
        {
            targetAlpha = Mathf.InverseLerp(activationThreshold, fullOpacityThreshold, dotUp);
        }

        menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, targetAlpha, Time.deltaTime * smoothSpeed);
        UpdateInteraction();
    }
    
    private void UpdateInteraction()
    {
        bool isInteractable = menuCanvasGroup.alpha >= interactionAlphaThreshold;
        menuCanvasGroup.interactable = isInteractable;
        menuCanvasGroup.blocksRaycasts = isInteractable;
    }
}