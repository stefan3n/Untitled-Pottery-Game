using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] shelfSlots;
    public GameObject potPrefab;

    [Header("Visuals")]
    public Material globalHighlightMaterial;

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
        if (potPrefab == null || activePot == null)
        {
            Debug.LogError("ShelfManager: Pot Prefab or Active Pot missing!");
            return;
        }

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

        float[] radiiData = activePot.GetRadiiData();
        float[] heightsData = activePot.GetHeightsData();
<<<<<<< Updated upstream
=======
        bool isPainted = activePot.CheckIfPainted();

        Texture2D originalTexOut = activePot.GetPaintTexture(0);
        Texture2D originalTexIn = activePot.GetPaintTexture(1);
        Texture2D texOutCopy = originalTexOut ? Instantiate(originalTexOut) : null;
        Texture2D texInCopy = originalTexIn ? Instantiate(originalTexIn) : null;
>>>>>>> Stashed changes

        GameObject newPotObj = Instantiate(potPrefab, shelfSlots[freeIndex].position, shelfSlots[freeIndex].rotation);
        newPotObj.transform.SetParent(shelfSlots[freeIndex]);
        newPotObj.transform.localScale = Vector3.one * savedPotScale;
        newPotObj.name = $"SavedPot_Slot_{freeIndex}";

<<<<<<< Updated upstream
        ShelfPot shelfPotScript = newPotObj.AddComponent<ShelfPot>();
        shelfPotScript.Initialize(freeIndex, radiiData, heightsData);
=======
        newPotObj.layer = LayerMask.NameToLayer("PotLayer");
        foreach (Transform child in newPotObj.transform) child.gameObject.layer = LayerMask.NameToLayer("PotLayer");

        ShelfPot shelfPotScript = newPotObj.GetComponent<ShelfPot>();

        if (shelfPotScript == null) shelfPotScript = newPotObj.AddComponent<ShelfPot>();

        shelfPotScript.highlightMaterial = globalHighlightMaterial;

        shelfPotScript.Initialize(freeIndex, radiiData, heightsData, texOutCopy, texInCopy, isPainted);
>>>>>>> Stashed changes

        Potter newPotterScript = newPotObj.GetComponent<Potter>();
        if (newPotterScript != null)
        {
<<<<<<< Updated upstream
            newPotterScript.LoadPotData(radiiData, heightsData);

            MeshRenderer sourceRenderer = activePot.GetComponent<MeshRenderer>();
            MeshRenderer targetRenderer = newPotObj.GetComponent<MeshRenderer>();

            if (sourceRenderer != null && targetRenderer != null)
            {
                targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

                Texture2D srcTex = activePot.GetPaintTexture();
                if (srcTex != null)
                {
                    Texture2D newTex = new Texture2D(srcTex.width, srcTex.height, srcTex.format, false);
                    Graphics.CopyTexture(srcTex, newTex);

                    var mats = targetRenderer.materials;
                    foreach (var m in mats)
                    {
                        if (m.HasProperty("_PaintTex")) m.SetTexture("_PaintTex", newTex);
                    }
                    targetRenderer.materials = mats;
                }
            }

=======
>>>>>>> Stashed changes
            newPotterScript.isStatic = true;
            newPotterScript.LoadPotData(radiiData, heightsData, texOutCopy, texInCopy, false);
        }

        isSlotOccupied[freeIndex] = true;

        activePot.ResetPot();

        Debug.Log($"SUCCESS: Pot saved to slot {freeIndex}. Is Painted: {isPainted}");
    }
}