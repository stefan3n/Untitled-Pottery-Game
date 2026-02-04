using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] shelfSlots;
    public GameObject potPrefab;

    // --- MODIFICARE 1: Aici tinem referinta la material ---
    [Header("Visuals")]
    public Material globalHighlightMaterial;
    // -----------------------------------------------------

    [Header("Appearance")]
    [Range(0.1f, 2.0f)]
    public float savedPotScale = 0.2f;

    private bool[] isSlotOccupied;

    void Awake()
    {
        if (shelfSlots != null)
            isSlotOccupied = new bool[shelfSlots.Length];
    }

    public void FreeSlot(int index)
    {
        if (index >= 0 && index < isSlotOccupied.Length)
        {
            isSlotOccupied[index] = false;
        }
    }

    public void SavePotToShelf(Potter activePot)
    {
        // 1. Validari
        if (potPrefab == null || activePot == null)
        {
            Debug.LogError("ShelfManager: Pot Prefab or Active Pot missing!");
            return;
        }

        // 2. Gasim slot liber
        int freeIndex = -1;
        for (int i = 0; i < isSlotOccupied.Length; i++)
        {
            if (!isSlotOccupied[i])
            {
                freeIndex = i;
                break;
            }
        }

        if (freeIndex == -1)
        {
            Debug.Log("No free shelf slots available!");
            return;
        }

        // 3. Extragem datele
        float[] radiiData = activePot.GetRadiiData();
        float[] heightsData = activePot.GetHeightsData();
        bool isPainted = activePot.CheckIfPainted();

        Texture2D originalTexOut = activePot.GetPaintTexture(0);
        Texture2D originalTexIn = activePot.GetPaintTexture(1);
        Texture2D texOutCopy = originalTexOut ? Instantiate(originalTexOut) : null;
        Texture2D texInCopy = originalTexIn ? Instantiate(originalTexIn) : null;

        // 4. Cream obiectul vizual pe raft
        GameObject newPotObj = Instantiate(potPrefab, shelfSlots[freeIndex].position, shelfSlots[freeIndex].rotation);
        newPotObj.transform.SetParent(shelfSlots[freeIndex]);
        newPotObj.transform.localScale = Vector3.one * savedPotScale;
        newPotObj.name = $"SavedPot_Slot_{freeIndex}";

        // Setam Layer-ul corect
        newPotObj.layer = LayerMask.NameToLayer("PotLayer");
        foreach (Transform child in newPotObj.transform) child.gameObject.layer = LayerMask.NameToLayer("PotLayer");

        // 5. Initializam scriptul raftului
        ShelfPot shelfPotScript = newPotObj.GetComponent<ShelfPot>();

        // AICI ESTE CHEIA: Daca nu exista pe prefab, il adaugam noi
        if (shelfPotScript == null) shelfPotScript = newPotObj.AddComponent<ShelfPot>();

        // --- MODIFICARE 2: Ii pasam materialul din Manager ---
        shelfPotScript.highlightMaterial = globalHighlightMaterial;
        // ----------------------------------------------------

        // Pasam restul datelor
        shelfPotScript.Initialize(freeIndex, radiiData, heightsData, texOutCopy, texInCopy, isPainted);

        // 6. Configuram vizualul
        Potter newPotterScript = newPotObj.GetComponent<Potter>();
        if (newPotterScript != null)
        {
            newPotterScript.isStatic = true;
            newPotterScript.LoadPotData(radiiData, heightsData, texOutCopy, texInCopy, false);
        }

        // 7. Finalizare
        isSlotOccupied[freeIndex] = true;
        activePot.ResetPot();

        Debug.Log($"SUCCESS: Pot saved to slot {freeIndex}. Is Painted: {isPainted}");
    }
}