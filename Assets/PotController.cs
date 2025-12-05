using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotController : MonoBehaviour
{
    [Header("VR Controller")]
    [SerializeField] private Transform controllerTransform;

    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private Potter pottery;
    [SerializeField] private float sculptSpeed = 2f;
    
    [Header("Selector (ring highlight)")]
    [SerializeField] private GameObject selector;
    [SerializeField] private Color hoverColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color activeColor = new Color(0f, 1f, 0f, 0.5f);
    
    private bool triggerPressed;
    private int selectedRing;
    private Renderer selectorRenderer;
    
    private void Start()
    {
        if (selector != null)
        {
            selector.SetActive(false);
            selectorRenderer = selector.GetComponent<Renderer>();
        }
    }

    
    private void Update()
    {
        float triggerValue = triggerAction.action?.ReadValue<float>() ?? 0f;
        triggerPressed = triggerValue > 0.5f;
    }

    private void OnTriggerStay(Collider other)
    {
    if (other.gameObject != pottery.gameObject) return;

    Vector3 worldPoint = controllerTransform.position;
    Vector3 localPoint = pottery.transform.InverseTransformPoint(worldPoint);

    int ringIndex = GetRingIndexFromLocalY(localPoint.y);
    if (ringIndex < 0 || ringIndex >= pottery.ringsCount)
    {
        HideSelector();
        return;
    }
    
    selectedRing = ringIndex;
    
    ShowSelector(triggerPressed);
    
    if (!triggerPressed)
            return;

    float currentRadius = pottery.ringsRadius[ringIndex];
    Vector2 localXZ = new Vector2(localPoint.x, localPoint.z);
    float contactRadius = localXZ.magnitude;

    bool touchingFromOutside = contactRadius >= currentRadius;

    float direction = touchingFromOutside ? -1f : 1f;

    float newRadius = currentRadius + direction * sculptSpeed * Time.deltaTime;

    pottery.ringsRadius[ringIndex] = newRadius;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != pottery.gameObject)
            return;

        HideSelector();
    }

    private int GetRingIndexFromLocalY(float localY)
    {
        for (int i = 0; i < pottery.ringsCount; i++)
        {
            float ringY = i * pottery.ringHeight;
            float nextRingY = (i + 1) * pottery.ringHeight;
            if (localY >= ringY && localY < nextRingY)
                return i;
        }
        if (localY < 0f) return 0;
        if (localY > pottery.ringsCount * pottery.ringHeight) return pottery.ringsCount - 1;
        return -1;
    }
    
    private void ShowSelector(bool isActive)
    {
        if (selector == null || pottery == null)
            return;

        selector.SetActive(true);

        // Position selector around the selected ring, like in your raycast version
        float localSelectorY = selectedRing * pottery.ringHeight + pottery.ringHeight * 0.5f;
        Vector3 localPosition = new Vector3(0, localSelectorY, 0);
        Vector3 worldPosition = pottery.transform.TransformPoint(localPosition);

        selector.transform.position = worldPosition;
        selector.transform.rotation = pottery.transform.rotation;

        // Scale based on ring radius
        float radius = pottery.ringsRadius[selectedRing];
        selector.transform.localScale = new Vector3(
            radius * 2.5f,
            pottery.ringHeight * 0.5f,
            radius * 2.5f
        );

        if (selectorRenderer != null && selectorRenderer.material != null)
        {
            selectorRenderer.material.color = isActive ? activeColor : hoverColor;
        }
    }

    private void HideSelector()
    {
        if (selector != null)
            selector.SetActive(false);
    }
}
