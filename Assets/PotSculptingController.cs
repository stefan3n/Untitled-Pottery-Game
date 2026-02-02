using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotSculptingController : MonoBehaviour
{
    [Header("VR Controllers")]
    [SerializeField] private Transform leftControllerTransform;
    [SerializeField] private Transform rightControllerTransform;

    [Header("Visual Hands (Rubber Banding)")]
    [SerializeField] private Transform leftHandVisual;
    [SerializeField] private Transform rightHandVisual;
    [SerializeField] private float handRadiusCollision = 0.05f;

    [SerializeField] private InputActionProperty leftTriggerAction;
    [SerializeField] private InputActionProperty rightTriggerAction;

    [SerializeField] private Potter pottery;
    [SerializeField] private float sculptSpeed = 0.25f;

    [Header("Sculpting Shape")]
    [SerializeField] private int neighborEffectRange = 5;

    [Header("Pull Settings")]
    [SerializeField] private float pullHeightSpeed = 0.2f;
    [SerializeField] private float splitThreshold = 0.3f;

    [Header("Selector (ring highlight)")]
    [SerializeField] private GameObject selector;
    [SerializeField] private Color hoverColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color activeColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private float baseSelectorHeight = 0.1f;
    [SerializeField] private float selectorRadiusTolerance = 0.15f;

    [Header("Pot State")]
    [SerializeField] private RotatePot rotatePot;

    private bool leftTriggerPressed;
    private bool rightTriggerPressed;
    private bool isPulling;
    private int selectedRing;
    private Renderer selectorRenderer;

    private bool isRightHandInsidePot = false;
    private bool isLeftHandInsidePot = false;

    private void OnEnable()
    {
        if (leftTriggerAction.action != null) leftTriggerAction.action.Enable();
        if (rightTriggerAction.action != null) rightTriggerAction.action.Enable();
    }

    private void OnDisable()
    {
        if (leftTriggerAction.action != null) leftTriggerAction.action.Disable();
        if (rightTriggerAction.action != null) rightTriggerAction.action.Disable();
    }

    private void Start()
    {
        if (selector != null)
        {
            selector.SetActive(false);
            selectorRenderer = selector.GetComponent<Renderer>();
        }

        if (pottery == null) Debug.LogError("Main Potter is missing!");

        if (rightHandVisual == null) Debug.LogWarning("Right Hand Visual missing.");
        if (leftHandVisual == null) Debug.LogWarning("Left Hand Visual missing.");
    }

    private void Update()
    {
        float lVal = leftTriggerAction.action?.ReadValue<float>() ?? 0f;
        leftTriggerPressed = lVal > 0.5f;

        float rVal = rightTriggerAction.action?.ReadValue<float>() ?? 0f;
        rightTriggerPressed = rVal > 0.5f;

        UpdateHandVisualPosition(rightControllerTransform, rightHandVisual, ref isRightHandInsidePot);
        UpdateHandVisualPosition(leftControllerTransform, leftHandVisual, ref isLeftHandInsidePot);

        Vector3 rightParams = rightHandVisual ? rightHandVisual.position : rightControllerTransform.position;
        // Vector3 leftParams = leftHandVisual ? leftHandVisual.position : leftControllerTransform.position;

        HandleHandHover(rightParams, rightTriggerPressed);

        // if (selector.activeSelf == false)
        // {
        //      HandleHandHover(leftParams, leftTriggerPressed);
        // }

        HandleHandSculpt(rightControllerTransform, rightParams, rightTriggerPressed, isRightHandInsidePot);
        // HandleHandSculpt(leftControllerTransform, leftParams, leftTriggerPressed, isLeftHandInsidePot);

        HandleTwoHandPull();
    }

    private void UpdateHandVisualPosition(Transform realTransform, Transform visualTransform, ref bool isInsideState)
    {
        if (!visualTransform || !realTransform || !pottery) return;
        if (pottery.ringsCount == 0 || pottery.ringHeights == null) return;

        Vector3 realWorldPos = realTransform.position;
        Vector3 localPos = pottery.transform.InverseTransformPoint(realWorldPos);
        float localY = localPos.y;
        float distFromCenter = new Vector2(localPos.x, localPos.z).magnitude;

        float topY = pottery.ringHeights[pottery.ringsCount - 1];
        float bottomY = pottery.ringHeights[0];

        if (localY > topY)
        {
            float topRadius = pottery.ringsRadius[pottery.ringsCount - 1];
            isInsideState = distFromCenter < topRadius;

            visualTransform.position = realWorldPos;
            visualTransform.rotation = realTransform.rotation;
            return;
        }
        if (localY < bottomY)
        {
            isInsideState = false;

            visualTransform.position = realWorldPos;
            visualTransform.rotation = realTransform.rotation;
            return;
        }

        int ringIndex = GetRingIndexFromLocalY(localY);
        if (ringIndex < 0) ringIndex = 0;
        if (ringIndex >= pottery.ringsCount) ringIndex = pottery.ringsCount - 1;

        float potRadiusAtHeight = pottery.ringsRadius[ringIndex];

        Vector2 finalXZ = new Vector2(localPos.x, localPos.z);
        Vector2 dir = finalXZ.normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        if (isInsideState)
        {
            float innerBoundary = Mathf.Max(0.001f, potRadiusAtHeight - handRadiusCollision);

            if (distFromCenter > innerBoundary)
            {
                finalXZ = dir * innerBoundary;
            }
        }
        else
        {
            float outerBoundary = potRadiusAtHeight + handRadiusCollision;

            if (distFromCenter < outerBoundary)
            {
                finalXZ = dir * outerBoundary;
            }
        }

        Vector3 finalLocal = new Vector3(finalXZ.x, localY, finalXZ.y);
        visualTransform.position = pottery.transform.TransformPoint(finalLocal);
        visualTransform.rotation = realTransform.rotation;
    }

    private void HandleHandHover(Vector3 handPosition, bool isTriggerPressed)
    {
        if (!pottery || pottery.ringHeights == null || pottery.ringsCount == 0)
        {
            HideSelector();
            return;
        }

        Vector3 local = pottery.transform.InverseTransformPoint(handPosition);

        int ringIndex = GetRingIndexFromLocalY(local.y);
        if (ringIndex < 0 || ringIndex >= pottery.ringsCount)
        {
            HideSelector();
            return;
        }

        float radiusAtRing = pottery.ringsRadius[ringIndex];
        float handRadius = new Vector2(local.x, local.z).magnitude;

        float distanceToSurface = Mathf.Abs(handRadius - radiusAtRing);

        if (distanceToSurface > (selectorRadiusTolerance + handRadiusCollision + 0.1f))
        {
            HideSelector();
             return;
        }

        selectedRing = ringIndex;
        ShowSelector(isTriggerPressed);
    }

    private void HandleHandSculpt(Transform realTransform, Vector3 visualPosition, bool isTriggerPressed, bool isInside)
    {
        if (!rotatePot || !rotatePot.IsRotating()) return;
        if (!isTriggerPressed) return;
        if (!pottery || pottery.ringHeights == null || pottery.ringsCount == 0) return;

        Vector3 localPoint = pottery.transform.InverseTransformPoint(visualPosition);

        int ringIndex = GetRingIndexFromLocalY(localPoint.y);
        if (ringIndex < 0 || ringIndex >= pottery.ringsCount)
            return;

        float currentRadius = pottery.ringsRadius[ringIndex];
        float visualRadius = new Vector2(localPoint.x, localPoint.z).magnitude;

        float distanceToSurface = Mathf.Abs(visualRadius - currentRadius);

        if (distanceToSurface > (selectorRadiusTolerance + handRadiusCollision + 0.1f))
            return;

        Vector3 realLocal = pottery.transform.InverseTransformPoint(realTransform.position);
        float realDist = new Vector2(realLocal.x, realLocal.z).magnitude;

        float direction = 0f;

        if (isInside)
        {
            if (realDist > currentRadius - handRadiusCollision * 0.5f)
            {
                direction = 1f;
            }
        }
        else
        {
            if (realDist < currentRadius + handRadiusCollision * 0.5f)
            {
                direction = -1f;
            }
        }

        if (direction == 0f) return;

        var baseSpeed = sculptSpeed;
        if (isPulling) baseSpeed /= 2;

        float deltaBase = direction * baseSpeed * Time.deltaTime;

        int range = neighborEffectRange; 
        float sigma = 2.0f; 

        for (int i = -range; i <= range; i++)
        {
            int targetIndex = ringIndex + i;
            if (targetIndex < 0 || targetIndex >= pottery.ringsCount) continue;

            float dist = Mathf.Abs(i);
            
            float weight = Mathf.Exp(-(dist * dist) / (2f * sigma * sigma));
            
            float ringCurrentRadius = pottery.ringsRadius[targetIndex];
            float newRadius = ringCurrentRadius + deltaBase * weight;
            newRadius = Mathf.Clamp(newRadius, pottery.minRingRadius, pottery.maxRingRadius);
            
            pottery.ringsRadius[targetIndex] = newRadius;
        }

        pottery.MarkModified();
    }

    private void HandleTwoHandPull()
    {
        if (!pottery || pottery.ringHeights == null || pottery.ringsCount < 2)
            return;

        // if (!leftTriggerPressed || !rightTriggerPressed)
        if (!rightTriggerPressed)
        {
            isPulling = false;
            return;
        }

        if (!leftControllerTransform || !rightControllerTransform)
            return;

        float maxHandsDistance = selectorRadiusTolerance * 2f;
        float handsDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
        if (handsDistance > maxHandsDistance)
        {
            isPulling = false;
            return;
        }

        Vector3 leftLocal = pottery.transform.InverseTransformPoint(leftControllerTransform.position);
        Vector3 rightLocal = pottery.transform.InverseTransformPoint(rightControllerTransform.position);

        int leftSegment = GetRingIndexFromLocalY(leftLocal.y);
        int rightSegment = GetRingIndexFromLocalY(rightLocal.y);

        if (leftSegment < 0 || leftSegment >= pottery.ringsCount - 1) return;
        if (rightSegment < 0 || rightSegment >= pottery.ringsCount - 1) return;

        int segmentIndex = leftSegment;
        if (segmentIndex >= pottery.ringsRadius.Length) return;

        float radius = pottery.ringsRadius[segmentIndex];

        float leftRadius = new Vector2(leftLocal.x, leftLocal.z).magnitude;
        float rightRadius = new Vector2(rightLocal.x, rightLocal.z).magnitude;

        float leftDistanceToSurface = Mathf.Abs(leftRadius - radius);
        float rightDistanceToSurface = Mathf.Abs(rightRadius - radius);

        if (leftDistanceToSurface > selectorRadiusTolerance || rightDistanceToSurface > selectorRadiusTolerance)
        {
            isPulling = false;
            return;
        }

        bool leftInside = leftRadius < radius;
        bool rightOutside = rightRadius >= radius;
        bool rightInside = rightRadius < radius;
        bool leftOutside = leftRadius >= radius;

        if (!((leftInside && rightOutside) || (rightInside && leftOutside)))
        {
            isPulling = false;
            return;
        }

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
