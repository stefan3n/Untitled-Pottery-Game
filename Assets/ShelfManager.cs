using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] shelfSlots;
    public GameObject potPrefab;

    [Header("Visuals")]
    public Material globalHighlightMaterial;

    [Header("UI Feedback")]
    public NotificationManager notificationManager;

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

    private Texture2D DuplicateTexture(Texture2D source)
    {
        if (source == null) return null;

        Texture2D newTex = new Texture2D(source.width, source.height, source.format, false);
        
        if (source.isReadable)
        {
            newTex.SetPixels(source.GetPixels());
            newTex.Apply(); 
        }
        else
        {
            try { Graphics.CopyTexture(source, newTex); } catch {}
        }
        
        return newTex;
    }
    
    public void SavePotToShelf(Potter activePot)
    {
        if (!potPrefab || !activePot)
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
            if (notificationManager != null) notificationManager.ShowNotification("Shelf Full!");
            return;
        }

        float[] radiiData = activePot.GetRadiiData();
        float[] heightsData = activePot.GetHeightsData();
        bool isPainted = activePot.CheckIfPainted();

        Material sourceMaterial = null;
        Renderer activeRenderer = activePot.GetComponent<Renderer>();
        if (activeRenderer)
        {
            sourceMaterial = activeRenderer.sharedMaterial;
        }
        
        Texture2D originalTexOut = activePot.GetPaintTexture(0);
        Texture2D originalTexIn = activePot.GetPaintTexture(1);
        Texture2D texOutCopy = DuplicateTexture(originalTexOut);
        Texture2D texInCopy = DuplicateTexture(originalTexIn);
        
        GameObject newPotObj = Instantiate(potPrefab, shelfSlots[freeIndex].position, shelfSlots[freeIndex].rotation);
        newPotObj.transform.SetParent(shelfSlots[freeIndex]);
        newPotObj.transform.localScale = Vector3.one * savedPotScale;
        newPotObj.name = $"SavedPot_Slot_{freeIndex}";
        
        string paintPropName = activePot.PaintTextureProperty; 
        if (string.IsNullOrEmpty(paintPropName)) paintPropName = "_PaintTex";

        newPotObj.layer = LayerMask.NameToLayer("PotLayer");
        foreach (Transform child in newPotObj.transform) child.gameObject.layer = LayerMask.NameToLayer("PotLayer");
        
        Renderer[] rends = newPotObj.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            if (sourceMaterial)
            {
                Material newMat = new Material(sourceMaterial);

                if (sourceMaterial.HasProperty("_Color")) newMat.SetColor("_Color", sourceMaterial.GetColor("_Color"));
                if (sourceMaterial.HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", sourceMaterial.GetColor("_BaseColor"));

                if (texOutCopy && newMat.HasProperty(paintPropName))
                {
                    newMat.SetTexture(paintPropName, texOutCopy);
                }

                if (r.sharedMaterials.Length > 1 && texInCopy)
                {
                    Material matIn = new Material(newMat);
                    if (matIn.HasProperty(paintPropName)) matIn.SetTexture(paintPropName, texInCopy);
                    r.materials = new Material[] { newMat, matIn };
                }
                else
                {
                    r.material = newMat;
                }
            }
        }
        
        ShelfPot shelfPotScript = newPotObj.GetComponent<ShelfPot>();
        if (!shelfPotScript) shelfPotScript = newPotObj.AddComponent<ShelfPot>();

        shelfPotScript.highlightMaterial = globalHighlightMaterial;

        shelfPotScript.Initialize(freeIndex, radiiData, heightsData, texOutCopy, texInCopy, isPainted);

        Potter newPotterScript = newPotObj.GetComponent<Potter>();
        if (newPotterScript)
        {
            newPotterScript.isStatic = true;
            
            newPotterScript.LoadPotData(radiiData, heightsData, texOutCopy, texInCopy, false);
        }

        isSlotOccupied[freeIndex] = true;

        activePot.FullReset();

        if (notificationManager != null)
            notificationManager.ShowNotification("Pot Saved!");

        Debug.Log($"SUCCESS: Pot saved to slot {freeIndex}. Is Painted: {isPainted}");
    }
}