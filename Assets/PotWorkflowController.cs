using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotWorkflowController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Potter pottery;
    [SerializeField] private ShelfManager shelfManager;

    [Tooltip("Optional. Lasa gol pentru Explore Mode.")]
    [SerializeField] private TargetManager targetManager;

    [Header("UI")]
    [SerializeField] private FeedbackUI feedbackUI; 

    [Header("Input")]
    [SerializeField] private InputActionProperty saveAction;
    [SerializeField] private InputActionProperty loadAction;
    [SerializeField] private InputActionProperty submitAction;

    private bool wasSavePressed = false;
    private bool wasLoadPressed = false;
    private bool wasSubmitPressed = false;

    private ShelfPot currentHoveredShelfPot = null;
    private bool isLoading = false;

    private Vector3 originalPotPosition;

    private void Start()
    {
        if (shelfManager == null) Debug.LogError("ShelfManager is missing!");
        if (pottery == null) Debug.LogError("Main Potter is missing!");

        if (pottery != null)
        {
            originalPotPosition = pottery.transform.position;
        }

        if (targetManager == null)
        {
            Debug.Log("Modul EXPLORE activat (TargetManager lipseste).");
        }
    }

    private void OnEnable()
    {
        if (saveAction.action != null) saveAction.action.Enable();
        if (loadAction.action != null) loadAction.action.Enable();

        if (targetManager != null && submitAction.action != null)
            submitAction.action.Enable();
    }

    private void Update()
    {
        HandleSaving();
        HandleLoading();

        if (targetManager != null)
        {
            HandleSubmit();
        }

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            PrintRadiiToConsole();
        }
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

                if (feedbackUI != null)
                {
                    feedbackUI.ShowMessage("Pot Saved!", 3f); 
                }
            }
        }
        wasSavePressed = isSavePressed;
    }

    private void HandleLoading()
    {
        float loadValue = loadAction.action?.ReadValue<float>() ?? 0f;
        bool isLoadPressed = loadValue > 0.5f;

        if (isLoadPressed && !wasLoadPressed && !isLoading)
        {
            if (currentHoveredShelfPot)
            {
                StartCoroutine(LoadPotSafeRoutine(currentHoveredShelfPot));
            }
        }
        wasLoadPressed = isLoadPressed;
    }

    private void HandleSubmit()
    {
        if (submitAction.action == null) return;

        float val = submitAction.action.ReadValue<float>();
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


    public void ReportHandEnter(Collider other)
    {
        if (isLoading) return;

        ShelfPot hitPot = other.GetComponentInParent<ShelfPot>();

        if (hitPot != null)
        {
            if (currentHoveredShelfPot != null)
                currentHoveredShelfPot.SetHighlight(false);

            currentHoveredShelfPot = hitPot;
            currentHoveredShelfPot.SetHighlight(true);
        }
    }

    public void ReportHandExit(Collider other)
    {
        ShelfPot hitPot = other.GetComponentInParent<ShelfPot>();

        if (hitPot != null && hitPot == currentHoveredShelfPot)
        {
            currentHoveredShelfPot.SetHighlight(false);
            currentHoveredShelfPot = null;
        }
    }

    private IEnumerator LoadPotSafeRoutine(ShelfPot shelfPot)
    {
        isLoading = true;
        
        float[] newRadii = shelfPot.GetRadii();
        float[] newHeights = shelfPot.GetHeights();
        int slotIndex = shelfPot.ShelfSlotIndex;

        shelfManager.FreeSlot(slotIndex);
        shelfPot.SetHighlight(false);
        Destroy(shelfPot.gameObject);
        currentHoveredShelfPot = null;

        Collider potCollider = pottery.GetComponent<Collider>();
        Rigidbody potRb = pottery.GetComponent<Rigidbody>();

        if (potCollider != null) potCollider.enabled = false;

        if (potRb != null)
        {
            potRb.isKinematic = true;

            potRb.linearVelocity = Vector3.zero;

            potRb.angularVelocity = Vector3.zero;
        }

        pottery.LoadPotData(newRadii, newHeights);

        yield return new WaitForEndOfFrame();

        pottery.transform.position = originalPotPosition;

        if (potCollider != null) potCollider.enabled = true;

        if (potRb != null)
        {
            potRb.isKinematic = false;
            potRb.WakeUp();
        }

        if (feedbackUI != null)
        {
            feedbackUI.ShowMessage("Pot Loaded!", 3f);
        }

        isLoading = false;
    }

    private void PrintRadiiToConsole()
    {
        if (!pottery) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("Radii: new float[] { ");
        for (int i = 0; i < pottery.ringsRadius.Length; i++)
        {
            sb.Append(pottery.ringsRadius[i].ToString("F2", CultureInfo.InvariantCulture));
            if (i < pottery.ringsRadius.Length - 1) sb.Append(", ");
        }
        sb.Append(" };\n");

        sb.Append("Heights: new float[] { ");
        for (int i = 0; i < pottery.ringHeights.Length; i++)
        {
            sb.Append(pottery.ringHeights[i].ToString("F2", CultureInfo.InvariantCulture));
            if (i < pottery.ringHeights.Length - 1) sb.Append(", ");
        }
        sb.Append(" };");

        Debug.Log("Copy Data:");
        Debug.Log(sb.ToString());
    }
}