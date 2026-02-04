<<<<<<< Updated upstream
using System.Collections;
using System.Globalization;
=======
using System.Collections.Generic;
>>>>>>> Stashed changes
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public sealed class PotWorkflowController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Potter pottery;
    [SerializeField] private ShelfManager shelfManager;

    [Tooltip("Optional. Lasa gol pentru Explore Mode.")]
    [SerializeField] private TargetManager targetManager;

<<<<<<< Updated upstream
    [Header("UI")]
    [SerializeField] private FeedbackUI feedbackUI; 

    [Header("Input")]
=======
    [Header("Interaction Settings")]
    [Tooltip("Drag the Right Hand Controller object here")]
    public Transform rightHandTransform;

    [Tooltip("Set this to 'PotLayer' (or Everything if layers are not set up)")]
    public LayerMask potLayerMask;
    public float interactionDistance = 10.0f;

    [Header("Input Actions")]
>>>>>>> Stashed changes
    [SerializeField] private InputActionProperty saveAction;
    [SerializeField] private InputActionProperty submitAction;

    [SerializeField] private InputActionProperty loadAction;

    private bool wasSavePressed = false;
    private bool wasLoadPressed = false;
    private bool wasSubmitPressed = false;

    private ShelfPot currentHoveredShelfPot = null;
    private bool isLoading = false;

    private Vector3 originalPotPosition;

    private List<UnityEngine.XR.InputDevice> inputDevices = new List<UnityEngine.XR.InputDevice>();

    void Update()
    {
<<<<<<< Updated upstream
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
=======
        HandleRaycastLogic();
        HandleControllerInput();
>>>>>>> Stashed changes
    }

    private void HandleRaycastLogic()
    {
<<<<<<< Updated upstream
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
=======
        if (rightHandTransform == null) return;

        RaycastHit hit;
        if (Physics.Raycast(rightHandTransform.position, rightHandTransform.forward, out hit, interactionDistance, potLayerMask))
>>>>>>> Stashed changes
        {
            ShelfPot pot = hit.collider.GetComponentInParent<ShelfPot>();

            if (pot != null)
            {
<<<<<<< Updated upstream
                shelfManager.SavePotToShelf(pottery);

                if (feedbackUI != null)
                {
                    feedbackUI.ShowMessage("Pot Saved!", 3f); 
                }
=======
                if (currentHoveredShelfPot != pot)
                {
                    if (currentHoveredShelfPot != null) currentHoveredShelfPot.SetHighlight(false);
                    currentHoveredShelfPot = pot;
                    currentHoveredShelfPot.SetHighlight(true);
                }
            }
            else
            {
                if (currentHoveredShelfPot != null)
                {
                    currentHoveredShelfPot.SetHighlight(false);
                    currentHoveredShelfPot = null;
                }
            }
        }
        else
        {
            if (currentHoveredShelfPot != null)
            {
                currentHoveredShelfPot.SetHighlight(false);
                currentHoveredShelfPot = null;
>>>>>>> Stashed changes
            }
        }
    }

    public void SavePotByButton()
    {
<<<<<<< Updated upstream
        float loadValue = loadAction.action?.ReadValue<float>() ?? 0f;
        bool isLoadPressed = loadValue > 0.5f;

        if (isLoadPressed && !wasLoadPressed && !isLoading)
=======
        if (shelfManager != null && pottery != null)
>>>>>>> Stashed changes
        {
            shelfManager.SavePotToShelf(pottery);
        }
    }

    private void HandleControllerInput()
    {
        float saveValue = saveAction.action?.ReadValue<float>() ?? 0f;
        bool isSaveDown = saveValue > 0.5f || (Keyboard.current != null && Keyboard.current.kKey.isPressed);

        if (isSaveDown && !wasSavePressed)
        {
            SavePotByButton();
        }
        wasSavePressed = isSaveDown;


        bool isGripPressedVR = false;

        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);

        if (inputDevices.Count > 0)
        {
            UnityEngine.XR.InputDevice rightController = inputDevices[0];

            rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out isGripPressedVR);
        }

        bool isLoadDown = isGripPressedVR || (Keyboard.current != null && Keyboard.current.mKey.isPressed);

        if (isLoadDown && !wasLoadPressed)
        {
            if (currentHoveredShelfPot != null)
            {
                StartCoroutine(LoadPotSafeRoutine(currentHoveredShelfPot));
            }
        }
        wasLoadPressed = isLoadDown;

<<<<<<< Updated upstream
    private void HandleSubmit()
    {
        if (submitAction.action == null) return;

        float val = submitAction.action.ReadValue<float>();
        bool isPressed = val > 0.5f;
=======
        float submitVal = submitAction.action?.ReadValue<float>() ?? 0f;
        bool isSubmitDown = submitVal > 0.5f || (Keyboard.current != null && Keyboard.current.enterKey.isPressed);
>>>>>>> Stashed changes

        if (isSubmitDown && !wasSubmitPressed)
        {
<<<<<<< Updated upstream
            if (targetManager != null)
            {
                targetManager.EvaluateAndShowResult();
            }
=======
            if (targetManager) targetManager.EvaluateAndShowResult();
>>>>>>> Stashed changes
        }
        wasSubmitPressed = isSubmitDown;
    }


    public void ReportHandEnter(Collider other)
    {
<<<<<<< Updated upstream
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
=======
        Debug.Log($"Loading pot from slot {shelfPot.ShelfSlotIndex}...");

        pottery.LoadPotData(
            shelfPot.GetRadii(),
            shelfPot.GetHeights(),
            shelfPot.GetTextureOutside(),
            shelfPot.GetTextureInside(),
            shelfPot.WasPainted
        );

        shelfManager.FreeSlot(shelfPot.ShelfSlotIndex);
        shelfPot.SetHighlight(false);
        Destroy(shelfPot.gameObject);
        currentHoveredShelfPot = null;
>>>>>>> Stashed changes
    }
}