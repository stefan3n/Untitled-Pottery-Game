using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

public sealed class PotWorkflowController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Potter pottery;
    [SerializeField] private ShelfManager shelfManager;
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private PaintBrush paintTip;
    [SerializeField] private Button buttonPaint;
    [SerializeField] private GameObject canvasPaint;
    [SerializeField] private GameObject paintBrush;

    
    [Header("UI Feedback")]
    public NotificationManager notificationManager;

    [Header("Interaction Settings")]
    [Tooltip("Drag the Right Hand Controller object here")]
    public Transform rightHandTransform;

    [Tooltip("Set this to 'PotLayer' (or Everything if layers are not set up)")]
    public LayerMask potLayerMask;
    public float interactionDistance = 10.0f;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty saveAction;
    [SerializeField] private InputActionProperty submitAction;

    [SerializeField] private InputActionProperty loadAction;

    private bool wasSavePressed = false;
    private bool wasLoadPressed = false;
    private bool wasSubmitPressed = false;

    private ShelfPot currentHoveredShelfPot = null;

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
            if (currentHoveredShelfPot)
            {
                LoadPotFromShelf(currentHoveredShelfPot);
            }
        }
        wasLoadPressed = isLoadDown;

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

        if (notificationManager)
        {
            notificationManager.ShowNotification("Pot Loaded!");
        }

        canvasPaint.SetActive(true);
        buttonPaint.gameObject.SetActive(false);
        paintBrush.SetActive(true);
        paintTip.gameObject.SetActive(true);
        // pottery.SetPaintTexture();
    }
}