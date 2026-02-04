using UnityEngine;

public class ShelfPot : MonoBehaviour
{
    public int ShelfSlotIndex { get; private set; }

    // Datele salvate
    private float[] savedRadii;
    private float[] savedHeights;
    private Texture2D savedTextureOutside;
    private Texture2D savedTextureInside;

    public bool WasPainted { get; private set; }

    private Renderer[] renderers;
    private Material[] originalMaterials;

    // --- SCHIMBARE: Variabila Publica pentru Material ---
    // Vom atribui materialul in Inspector pe Prefab, nu il mai cream in cod
    [Header("Visuals")]
    public Material highlightMaterial;

    public void Initialize(int slotIndex, float[] radii, float[] heights, Texture2D texOut, Texture2D texIn, bool wasPainted)
    {
        ShelfSlotIndex = slotIndex;
        savedRadii = radii;
        savedHeights = heights;
        savedTextureOutside = texOut;
        savedTextureInside = texIn;
        WasPainted = wasPainted;

        renderers = GetComponentsInChildren<Renderer>();
    }

    public float[] GetRadii() => savedRadii;
    public float[] GetHeights() => savedHeights;
    public Texture2D GetTextureOutside() => savedTextureOutside;
    public Texture2D GetTextureInside() => savedTextureInside;

    public void SetHighlight(bool active)
    {
        if (renderers == null) return;

        // Daca ai uitat sa pui materialul in inspector, iesim ca sa nu dea eroare
        if (highlightMaterial == null) return;

        foreach (var rend in renderers)
        {
            if (active)
            {
                // Daca deja e highlight, nu facem nimic
                if (rend.sharedMaterial == highlightMaterial) continue;

                // Salvam materialele originale
                if (originalMaterials == null || originalMaterials.Length == 0)
                    originalMaterials = rend.sharedMaterials;

                // Aplicam highlight
                Material[] glowMats = new Material[rend.sharedMaterials.Length];
                for (int i = 0; i < glowMats.Length; i++) glowMats[i] = highlightMaterial;

                rend.materials = glowMats;
            }
            else
            {
                // Restauram materialele originale
                if (originalMaterials != null && originalMaterials.Length > 0)
                {
                    rend.sharedMaterials = originalMaterials;
                    originalMaterials = null;
                }
            }
        }
    }
}