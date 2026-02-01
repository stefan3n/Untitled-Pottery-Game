using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotWorkflowController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Potter pottery;
    [SerializeField] private ShelfManager shelfManager;
    [SerializeField] private TargetManager targetManager;

    [Header("Input")]
    [SerializeField] private InputActionProperty saveAction;
    [SerializeField] private InputActionProperty loadAction;
    [SerializeField] private InputActionProperty submitAction;

    private bool wasSavePressed = false;
    private bool wasLoadPressed = false;
    private bool wasSubmitPressed = false;

    private ShelfPot currentHoveredShelfPot = null;

    private void Start()
    {
        if (shelfManager == null) Debug.LogError("ShelfManager is missing!");
        if (pottery == null) Debug.LogError("Main Potter is missing!");
    }

    private void OnEnable()
    {
        if (saveAction.action != null) saveAction.action.Enable();
        if (loadAction.action != null) loadAction.action.Enable();
        if (submitAction.action != null) submitAction.action.Enable();
    }

    private void Update()
    {
        HandleSaving();
        HandleLoading();
        HandleSubmit();
        
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
            if (currentHoveredShelfPot)
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
            if (targetManager)
            {
                targetManager.EvaluateAndShowResult();
            }
        }
        wasSubmitPressed = isPressed;
    }

    private void LoadPotFromShelf(ShelfPot shelfPot)
    {
        Debug.Log("Loading pot from shelf");

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

    private void OnTriggerExit(Collider other)
    {
        ShelfPot hitPot = other.GetComponent<ShelfPot>();
        if (hitPot != null && hitPot == currentHoveredShelfPot)
        {
            currentHoveredShelfPot.SetHighlight(false);
            currentHoveredShelfPot = null;
        }
    }
    
    private void PrintRadiiToConsole()
    {
        if (!pottery) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("new float[] { ");

        for (int i = 0; i < pottery.ringsRadius.Length; i++)
        {
            float val = pottery.ringsRadius[i];
            string valString = val.ToString("F2", CultureInfo.InvariantCulture);

            sb.Append(valString + "f");

            if (i < pottery.ringsRadius.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(" };");

        Debug.Log("Copy");
        Debug.Log(sb.ToString());
    }
}
