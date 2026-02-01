using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotSculptingController : MonoBehaviour
{
    [Header("VR Controllers")]
    [SerializeField] private Transform leftControllerTransform;
    [SerializeField] private Transform rightControllerTransform;

    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private Potter pottery;
    [SerializeField] private float sculptSpeed = 0.25f;

    [Header("Pull Settings")]
    [SerializeField] private float pullHeightSpeed = 0.2f;
    [SerializeField] private float splitThreshold = 0.3f;

    [Header("Selector (ring highlight)")]
    [SerializeField] private GameObject selector;
    [SerializeField] private Color hoverColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color activeColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private float baseSelectorHeight = 0.1f;
    [SerializeField] private float selectorRadiusTolerance = 0.15f;

    private bool triggerPressed;
    private bool isPulling;
    private int selectedRing;
    private Renderer selectorRenderer;

    private void Start()
    {
        if (selector != null)
        {
            selector.SetActive(false);
            selectorRenderer = selector.GetComponent<Renderer>();
        }

        if (pottery == null) Debug.LogError("Main Potter is missing!");
    }

    private void Update()
    {
        float triggerValue = triggerAction.action?.ReadValue<float>() ?? 0f;
        triggerPressed = triggerValue > 0.5f;

        HandleRightHandHover();
        HandleRightHandSculpt();
        HandleTwoHandPull();
        
    }

    private void HandleRightHandHover()
    {
        if (!pottery || pottery.ringHeights == null || pottery.ringsCount == 0)
        {
            HideSelector();
            return;
        }

        if (!rightControllerTransform)
        {
            HideSelector();
            return;
        }

        Vector3 local = pottery.transform.InverseTransformPoint(rightControllerTransform.position);

        int ringIndex = GetRingIndexFromLocalY(local.y);
        if (ringIndex < 0 || ringIndex >= pottery.ringsCount)
        {
            HideSelector();
            return;
        }

        float radiusAtRing = pottery.ringsRadius[ringIndex];
        float handRadius = new Vector2(local.x, local.z).magnitude;

        float distanceToSurface = Mathf.Abs(handRadius - radiusAtRing);
        if (distanceToSurface > selectorRadiusTolerance)
        {
            HideSelector();
            return;
        }

        selectedRing = ringIndex;
        ShowSelector(triggerPressed);
    }

    private void HandleRightHandSculpt()
    {
        if (!triggerPressed) return;
        if (!pottery || pottery.ringHeights == null || pottery.ringsCount == 0) return;
        if (!rightControllerTransform) return;

        Vector3 localPoint = pottery.transform.InverseTransformPoint(rightControllerTransform.position);

        int ringIndex = GetRingIndexFromLocalY(localPoint.y);
        if (ringIndex < 0 || ringIndex >= pottery.ringsCount)
            return;

        float currentRadius = pottery.ringsRadius[ringIndex];
        Vector2 localXZ = new Vector2(localPoint.x, localPoint.z);
        float contactRadius = localXZ.magnitude;

        float distanceToSurface = Mathf.Abs(contactRadius - currentRadius);
        if (distanceToSurface > selectorRadiusTolerance)
            return;

        bool touchingFromOutside = contactRadius >= currentRadius;
        float direction = touchingFromOutside ? -1f : 1f;

        var speed = sculptSpeed;

        if (isPulling)
            speed /= 2;

        float newRadius = currentRadius + direction * speed * Time.deltaTime;
        pottery.ringsRadius[ringIndex] = newRadius;
        pottery.MarkModified();
    }

    private void HandleTwoHandPull()
    {
        if (!pottery || pottery.ringHeights == null || pottery.ringsCount < 2)
            return;

        if (!triggerPressed)
            return;

        if (!leftControllerTransform || !rightControllerTransform)
            return;

        float maxHandsDistance = selectorRadiusTolerance * 2f;
        float handsDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
        if (handsDistance > maxHandsDistance)
            return;

        Vector3 leftLocal = pottery.transform.InverseTransformPoint(leftControllerTransform.position);
        Vector3 rightLocal = pottery.transform.InverseTransformPoint(rightControllerTransform.position);

        int leftSegment = GetRingIndexFromLocalY(leftLocal.y);
        int rightSegment = GetRingIndexFromLocalY(rightLocal.y);

        if (leftSegment < 0 || leftSegment >= pottery.ringsCount - 1) return;
        if (rightSegment < 0 || rightSegment >= pottery.ringsCount - 1) return;

        int segmentIndex = leftSegment;

        float radius = pottery.ringsRadius[segmentIndex];

        float leftRadius = new Vector2(leftLocal.x, leftLocal.z).magnitude;
        float rightRadius = new Vector2(rightLocal.x, rightLocal.z).magnitude;

        float leftDistanceToSurface = Mathf.Abs(leftRadius - radius);
        float rightDistanceToSurface = Mathf.Abs(rightRadius - radius);

        if (leftDistanceToSurface > selectorRadiusTolerance) return;
        if (rightDistanceToSurface > selectorRadiusTolerance) return;

        bool leftInside = leftRadius < radius;
        bool rightOutside = rightRadius >= radius;

        if (!leftInside || !rightOutside)
            return;

        float bottomY = pottery.ringHeights[segmentIndex];
        float topY = pottery.ringHeights[segmentIndex + 1];
        float segmentHeight = Mathf.Abs(topY - bottomY);

        float deltaHeight = pullHeightSpeed * Time.deltaTime;

        float currentTotalHeight = pottery.GetTotalHeight();
        float proposedTotalHeight = currentTotalHeight + deltaHeight;
        if (proposedTotalHeight > pottery.maxPotHeight)
        {
            return;
        }

        segmentHeight += deltaHeight;

        float newTopY = bottomY + segmentHeight;
        float deltaY = newTopY - topY;

        for (int i = segmentIndex + 1; i < pottery.ringsCount; i++)
        {
            pottery.ringHeights[i] += deltaY;
        }

        if (segmentHeight > splitThreshold)
        {
            pottery.InsertRingBetween(segmentIndex, segmentIndex + 1);
        }

        isPulling = true;

        pottery.MarkModified();
    }

    private void OnTriggerExit(Collider other)
    {
        // Only responsible for hiding selector if leaving the pot area
        if (pottery != null && other.gameObject == pottery.gameObject)
        {
            HideSelector();
        }
    }

    private int GetRingIndexFromLocalY(float localY)
    {
        if (!pottery || pottery.ringHeights == null || pottery.ringHeights.Length == 0)
            return -1;

        int count = pottery.ringsCount;

        if (localY <= pottery.ringHeights[0])
            return 0;

        if (localY >= pottery.ringHeights[count - 1])
            return count - 1;

        for (int i = 0; i < count - 1; i++)
        {
            float ringY = pottery.ringHeights[i];
            float nextRingY = pottery.ringHeights[i + 1];

            float minY = Mathf.Min(ringY, nextRingY);
            float maxY = Mathf.Max(ringY, nextRingY);

            if (localY >= minY && localY < maxY)
                return i;
        }

        return -1;
    }

    private void ShowSelector(bool isActive)
    {
        if (!selector || !pottery || pottery.ringHeights == null)
            return;

        if (pottery.ringsCount <= 0)
            return;

        selector.SetActive(true);

        int idx = Mathf.Clamp(selectedRing, 0, pottery.ringsCount - 1);

        float thisY = pottery.ringHeights[idx];
        float localSelectorY = thisY;

        float segmentHeight = baseSelectorHeight;
        if (idx < pottery.ringsCount - 1)
        {
            float nextY = pottery.ringHeights[idx + 1];
            segmentHeight = Mathf.Max(baseSelectorHeight, Mathf.Abs(nextY - thisY));
            localSelectorY = 0.5f * (thisY + nextY);
        }

        Vector3 worldPosition = pottery.transform.TransformPoint(new Vector3(0f, localSelectorY, 0f));
        selector.transform.position = worldPosition;
        selector.transform.rotation = pottery.transform.rotation;

        float radius = pottery.ringsRadius[Mathf.Clamp(idx, 0, pottery.ringsRadius.Length - 1)];
        selector.transform.localScale = new Vector3(
            radius * 2.5f,
            segmentHeight * 0.5f,
            radius * 2.5f
        );

        if (selectorRenderer && selectorRenderer.material)
        {
            selectorRenderer.material.color = isActive ? activeColor : hoverColor;
        }
    }

    private void HideSelector()
    {
        if (selector) selector.SetActive(false);
    }
    
}
