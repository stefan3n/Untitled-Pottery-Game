using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotController : MonoBehaviour
{
    [Header("VR Controller")]
    [SerializeField] private Transform controllerTransform;

    [SerializeField] private InputActionProperty triggerAction; // Sculpt
    [SerializeField] private Potter pottery; // Vasul principal de pe masa
    [SerializeField] private float sculptSpeed = 2f;

    [Header("Selector (ring highlight)")]
    [SerializeField] private GameObject selector;
    [SerializeField] private Color hoverColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color activeColor = new Color(0f, 1f, 0f, 0.5f);

    [Header("Saving & Loading")]
    [SerializeField] private ShelfManager shelfManager;
    [SerializeField] private InputActionProperty saveAction;
    [SerializeField] private InputActionProperty loadAction;

    [Header("Game Flow")]
    [SerializeField] private TargetManager targetManager; 
    [SerializeField] private InputActionProperty submitAction; 

    private bool wasSavePressed = false;
    private bool wasLoadPressed = false;
    private bool wasSubmitPressed = false;

    private bool triggerPressed;
    private int selectedRing;
    private Renderer selectorRenderer;

    private ShelfPot currentHoveredShelfPot = null;

    private void Start()
    {
        if (selector != null)
        {
            selector.SetActive(false);
            selectorRenderer = selector.GetComponent<Renderer>();
        }

        if (shelfManager == null) UnityEngine.Debug.LogError("ShelfManager is missing!");
        if (pottery == null) UnityEngine.Debug.LogError("Main Potter is missing!");
    }

    private void OnEnable()
    {
        if (saveAction.action != null) saveAction.action.Enable();
        if (loadAction.action != null) loadAction.action.Enable();
        if (submitAction.action != null) submitAction.action.Enable();
    }

    private void Update()
    {
        float triggerValue = triggerAction.action?.ReadValue<float>() ?? 0f;
        triggerPressed = triggerValue > 0.5f;

        HandleSaving();
        HandleLoading();
        HandleSubmit();

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            PrintRadiiToConsole();
        }
    }

    // Folosim functia asta cand dorim sa cunoastem razele vasului curent
    private void PrintRadiiToConsole()
    {
        if (pottery == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("new float[] { ");

        for (int i = 0; i < pottery.ringsRadius.Length; i++)
        {
            // Luam raza curenta
            float val = pottery.ringsRadius[i];

            string valString = val.ToString("F2", CultureInfo.InvariantCulture);

            sb.Append(valString + "f");

            if (i < pottery.ringsRadius.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(" };");

        UnityEngine.Debug.Log("--- COPY LEVEL DATA ---");
        UnityEngine.Debug.Log(sb.ToString());
        UnityEngine.Debug.Log("---------------------------");
    }

    private void HandleSaving()
    {
        float saveValue = saveAction.action?.ReadValue<float>() ?? 0f;
        bool isSavePressed = saveValue > 0.5f;

        if (isSavePressed && !wasSavePressed)
        {
            if (shelfManager != null && pottery != null)
            {
                shelfManager.SavePotToShelf(pottery);
            }
        }
        wasSavePressed = isSavePressed;
    }

    private void HandleLoading()
    {
        float loadValue = loadAction.action?.ReadValue<float>() ?? 0f;
        bool isLoadPressed = loadValue > 0.5f;

        if (isLoadPressed && !wasLoadPressed)
        {
            if (currentHoveredShelfPot != null)
            {
                LoadPotFromShelf(currentHoveredShelfPot);
            }
        }
        wasLoadPressed = isLoadPressed;
    }

    private void HandleSubmit()
    {
        float val = submitAction.action?.ReadValue<float>() ?? 0f;
        bool isPressed = val > 0.5f;

        if (isPressed && !wasSubmitPressed)
        {
            if (targetManager != null)
            {
                targetManager.EvaluateAndShowResult();
            }
        }
        wasSubmitPressed = isPressed;
    }

    private void LoadPotFromShelf(ShelfPot shelfPot)
    {
        UnityEngine.Debug.Log("Loading pot from shelf...");

        float[] newData = shelfPot.GetData();

        pottery.SetRadiiData(newData);

        shelfManager.FreeSlot(shelfPot.ShelfSlotIndex);

        shelfPot.SetHighlight(false);
        Destroy(shelfPot.gameObject);

        currentHoveredShelfPot = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        ShelfPot hitPot = other.GetComponent<ShelfPot>();
        if (hitPot != null)
        {
            if (currentHoveredShelfPot != null)
                currentHoveredShelfPot.SetHighlight(false);

            currentHoveredShelfPot = hitPot;
            currentHoveredShelfPot.SetHighlight(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<ShelfPot>() != null) return;

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
        ShelfPot hitPot = other.GetComponent<ShelfPot>();
        if (hitPot != null && hitPot == currentHoveredShelfPot)
        {
            currentHoveredShelfPot.SetHighlight(false);
            currentHoveredShelfPot = null;
        }

        if (other.gameObject == pottery.gameObject)
        {
            HideSelector();
        }
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
        if (selector == null || pottery == null) return;

        selector.SetActive(true);
        float localSelectorY = selectedRing * pottery.ringHeight + pottery.ringHeight * 0.5f;
        Vector3 worldPosition = pottery.transform.TransformPoint(new Vector3(0, localSelectorY, 0));

        selector.transform.position = worldPosition;
        selector.transform.rotation = pottery.transform.rotation;

        float radius = pottery.ringsRadius[selectedRing];
        selector.transform.localScale = new Vector3(radius * 2.5f, pottery.ringHeight * 0.5f, radius * 2.5f);

        if (selectorRenderer != null && selectorRenderer.material != null)
            selectorRenderer.material.color = isActive ? activeColor : hoverColor;
    }

    private void HideSelector()
    {
        if (selector != null) selector.SetActive(false);
    }
}