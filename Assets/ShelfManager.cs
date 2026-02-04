using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] shelfSlots;
    public GameObject potPrefab;

    [Header("Visuals")]
    public Material globalHighlightMaterial; // Materialul albastru transparent pentru highlight

    [Header("UI Feedback")]
    public NotificationManager notificationManager; // Referinta pentru notificari text

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

    // Functie helper pentru a copia textura curat (fara bug-uri pe Quest)
    private Texture2D DuplicateTexture(Texture2D source)
    {
        if (source == null) return null;

        // Cream o textura noua goala cu aceleasi setari
        Texture2D newTex = new Texture2D(source.width, source.height, source.format, false);

        // Copiem pixelii direct pe GPU
        Graphics.CopyTexture(source, newTex);

        return newTex;
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
            if (notificationManager != null) notificationManager.ShowNotification("Shelf Full!");
            return;
        }

        // 3. Extragem datele geometrice
        float[] radiiData = activePot.GetRadiiData();
        float[] heightsData = activePot.GetHeightsData();
        bool isPainted = activePot.CheckIfPainted();

        // --- PRELUAM MATERIALUL SURSA (Fix pentru Culoarea Clay) ---
        // Luam materialul exact de pe vasul de pe masa ca sa pastram culoarea si proprietatile
        Material sourceMaterial = null;
        Renderer activeRenderer = activePot.GetComponent<Renderer>();
        if (activeRenderer != null)
        {
            sourceMaterial = activeRenderer.sharedMaterial;
        }
        // ----------------------------------------------------------

        // --- COPIEM TEXTURILE (Fix pentru Textura Neagra pe Quest) ---
        Texture2D originalTexOut = activePot.GetPaintTexture(0);
        Texture2D originalTexIn = activePot.GetPaintTexture(1);
        Texture2D texOutCopy = DuplicateTexture(originalTexOut);
        Texture2D texInCopy = DuplicateTexture(originalTexIn);
        // -------------------------------------------------------------

        // 4. Cream obiectul vizual pe raft
        GameObject newPotObj = Instantiate(potPrefab, shelfSlots[freeIndex].position, shelfSlots[freeIndex].rotation);
        newPotObj.transform.SetParent(shelfSlots[freeIndex]);
        newPotObj.transform.localScale = Vector3.one * savedPotScale;
        newPotObj.name = $"SavedPot_Slot_{freeIndex}";

        // Setam Layer-ul corect pentru Raycast
        newPotObj.layer = LayerMask.NameToLayer("PotLayer");
        foreach (Transform child in newPotObj.transform) child.gameObject.layer = LayerMask.NameToLayer("PotLayer");

        // --- APLICARE MATERIAL CLONAT PE RAFT ---
        // Aici ne asiguram ca vasul de pe raft arata IDENTIC cu cel de pe masa (Clay + Pictura)
        Renderer[] rends = newPotObj.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            if (sourceMaterial != null)
            {
                // 1. Cream o COPIE a materialului de pe masa (deci va fi Clay)
                Material newMat = new Material(sourceMaterial);

                // 2. Ii aplicam textura noua pictata
                if (texOutCopy != null)
                {
                    newMat.mainTexture = texOutCopy;
                    // Suport pentru URP/HDRP daca e cazul
                    if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", texOutCopy);
                }

                // 3. Atribuim acest material nou vasului de pe raft
                r.material = newMat;
            }
        }
        // ----------------------------------------

        // 5. Initializam scriptul raftului (si adaugam componenta daca lipseste)
        ShelfPot shelfPotScript = newPotObj.GetComponent<ShelfPot>();
        if (shelfPotScript == null) shelfPotScript = newPotObj.AddComponent<ShelfPot>();

        // Ii dam materialul de highlight pentru interactiune
        shelfPotScript.highlightMaterial = globalHighlightMaterial;

        // Initializam datele
        shelfPotScript.Initialize(freeIndex, radiiData, heightsData, texOutCopy, texInCopy, isPainted);

        // 6. Configuram vizualul geometric (Potter script pe raft e doar vizual, static)
        Potter newPotterScript = newPotObj.GetComponent<Potter>();
        if (newPotterScript != null)
        {
            newPotterScript.isStatic = true;
            // Incarcam datele geometrice. Textura e deja pusa pe material mai sus, dar o pasam si aici pt consistenta
            newPotterScript.LoadPotData(radiiData, heightsData, texOutCopy, texInCopy, false);
        }

        // 7. Finalizare
        isSlotOccupied[freeIndex] = true;

        // Resetam vasul de pe masa
        activePot.ResetPot();

        // --- NOTIFICARE UI ---
        if (notificationManager != null)
            notificationManager.ShowNotification("Pot Saved!");

        Debug.Log($"SUCCESS: Pot saved to slot {freeIndex}. Is Painted: {isPainted}");
    }
}