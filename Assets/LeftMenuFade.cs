using UnityEngine;
public class LeftMenuFade : MonoBehaviour
{
    [Header("Assign Your Controller Transform (left hand)")]
    public Transform controllerTransform;

    [Header("Assign Your CanvasGroup component (on menu)")]
    public CanvasGroup menuCanvasGroup;

    [Header("Pot Settings")]
    public Transform potTransform;
    public float minDistanceToPot = 0.5f;
    
    [Header("Sensitivity")]
    [Range(0f, 1f)]
    public float activationThreshold = 0.5f;

    public float smoothSpeed = 10f;

    void Update()
    {
        if (!controllerTransform || !menuCanvasGroup) return;

        if (potTransform)
        {
            float distanceToPot = Vector3.Distance(controllerTransform.position, potTransform.position);
            if (distanceToPot < minDistanceToPot)
            {
                menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, 0f, Time.deltaTime * smoothSpeed);
                return;
            }
        }
        
        Vector3 palmNormal = controllerTransform.right; 

        float dot = Vector3.Dot(palmNormal, Vector3.up);

        float targetAlpha = 0f;
        if (dot > activationThreshold)
        {
            targetAlpha = Mathf.InverseLerp(activationThreshold, 1f, dot);
        }

        menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, targetAlpha, Time.deltaTime * smoothSpeed);
    }
}