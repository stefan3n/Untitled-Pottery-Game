using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public sealed class PotWorkflowController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Potter pottery;
    [SerializeField] private ShelfManager shelfManager;
    [SerializeField] private TargetManager targetManager;

    [Header("Interaction Settings")]
    [Tooltip("Drag the Right Hand Controller object here")]
    public Transform rightHandTransform;

    [Tooltip("Set this to 'PotLayer' (or Everything if layers are not set up)")]
    public LayerMask potLayerMask;
    public float interactionDistance = 10.0f;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty saveAction;
    [SerializeField] private InputActionProperty submitAction;

    // Lasam variabila ca sa nu crape inspectorul, dar o ignoram pentru functionalitatea Load
    [SerializeField] private InputActionProperty loadAction;

    private bool wasSavePressed = false;
    private bool wasLoadPressed = false;
    private bool wasSubmitPressed = false;

    private ShelfPot currentHoveredShelfPot = null;

    // Lista explicita UnityEngine.XR
    private List<UnityEngine.XR.InputDevice> inputDevices = new List<UnityEngine.XR.InputDevice>();

    void Update()
    {
        HandleRaycastLogic();
        HandleControllerInput();
    }

    private void HandleRaycastLogic()
    {
        if (rightHandTransform == null) return;

        RaycastHit hit;
        if (Physics.Raycast(rightHandTransform.position, rightHandTransform.forward, out hit, interactionDistance, potLayerMask))
        {
            ShelfPot pot = hit.collider.GetComponentInParent<ShelfPot>();

            if (pot != null)
            {
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
            }
        }
    }

    public void SavePotByButton()
    {
        if (shelfManager != null && pottery != null)
        {
            shelfManager.SavePotToShelf(pottery);
        }
    }

    private void HandleControllerInput()
    {
        // --- SAVE (Input System SAU Tasta K) ---
        float saveValue = saveAction.action?.ReadValue<float>() ?? 0f;
        bool isSaveDown = saveValue > 0.5f || (Keyboard.current != null && Keyboard.current.kKey.isPressed);

        if (isSaveDown && !wasSavePressed)
        {
            SavePotByButton();
        }
        wasSavePressed = isSaveDown;

        // --- LOAD (HARDCODED VR GRIP + Tasta M) ---

        bool isGripPressedVR = false;

        // 1. Cerem sistemului XR toate dispozitivele Right Hand
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);

        // 2. Daca am gasit controllerul drept
        if (inputDevices.Count > 0)
        {
            UnityEngine.XR.InputDevice rightController = inputDevices[0];

            // 3. Citim direct butonul GRIP (Folosind explicit UnityEngine.XR.CommonUsages)
            // --- FIX AICI: UnityEngine.XR.CommonUsages ---
            rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out isGripPressedVR);
        }

        // Combinam: E apasat Grip-ul VR SAU tasta M?
        bool isLoadDown = isGripPressedVR || (Keyboard.current != null && Keyboard.current.mKey.isPressed);

        if (isLoadDown && !wasLoadPressed)
        {
            if (currentHoveredShelfPot != null)
            {
                LoadPotFromShelf(currentHoveredShelfPot);
            }
        }
        wasLoadPressed = isLoadDown;

        // --- SUBMIT (Input System SAU Enter) ---
        float submitVal = submitAction.action?.ReadValue<float>() ?? 0f;
        bool isSubmitDown = submitVal > 0.5f || (Keyboard.current != null && Keyboard.current.enterKey.isPressed);

        if (isSubmitDown && !wasSubmitPressed)
        {
            if (targetManager) targetManager.EvaluateAndShowResult();
        }
        wasSubmitPressed = isSubmitDown;
    }

    private void LoadPotFromShelf(ShelfPot shelfPot)
    {
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
    }
}