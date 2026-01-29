using UnityEngine;
using UnityEngine.UI;

public class LeftMenuFade : MonoBehaviour
{
    [Header("Assign Your Controller Transform (left hand)")]
    public Transform controllerTransform;

    [Header("Assign Your CanvasGroup component (on menu)")]
    public CanvasGroup menuCanvasGroup;

    [Header("Rotation Range")]
    public float minAngle = 0f;  
    public float maxAngle = 90f; 

    void Update()
    {
        float zRot = controllerTransform.localEulerAngles.z;

        float angle = Mathf.DeltaAngle(0, zRot); 
        angle = Mathf.Abs(angle);

        float t = Mathf.InverseLerp(minAngle, maxAngle, angle);
        menuCanvasGroup.alpha = Mathf.Clamp01(t);
    }
}