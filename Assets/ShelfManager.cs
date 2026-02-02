using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] shelfSlots;
    public GameObject potPrefab;

    [Header("Appearance")]
    [Range(0.1f, 2.0f)]
    public float savedPotScale = 0.2f;

    private bool[] isSlotOccupied;

    void Awake()
    {
        if (shelfSlots != null)
        {
            isSlotOccupied = new bool[shelfSlots.Length];
        }
    }

    public void FreeSlot(int index)
    {
        if (index >= 0 && index < isSlotOccupied.Length)
        {
            isSlotOccupied[index] = false;
            Debug.Log($"Slot {index} freed.");
        }
    }

    public void SavePotToShelf(Potter activePot)
    {
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

        GameObject newPotObj = Instantiate(potPrefab, shelfSlots[freeIndex].position, shelfSlots[freeIndex].rotation);
        newPotObj.transform.SetParent(shelfSlots[freeIndex]);
        newPotObj.transform.localScale = Vector3.one * savedPotScale;

        ShelfPot shelfPotScript = newPotObj.AddComponent<ShelfPot>();
        shelfPotScript.Initialize(freeIndex, radiiData, heightsData);

        Potter newPotterScript = newPotObj.GetComponent<Potter>();

        if (newPotterScript != null)
        {
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

            newPotterScript.isStatic = true;
            newPotterScript.GenerateMesh();
        }

        isSlotOccupied[freeIndex] = true;

        activePot.ResetPot();

        Debug.Log($"Pot saved to shelf slot {freeIndex}.");
    }
}