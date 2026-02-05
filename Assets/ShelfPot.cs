using UnityEngine;

public class ShelfPot : MonoBehaviour
{
    public int ShelfSlotIndex { get; private set; }

    private float[] savedRadii;
    private float[] savedHeights;
    private Texture2D savedTextureOutside;
    private Texture2D savedTextureInside;

    public bool WasPainted { get; private set; }

    private Renderer[] renderers;
    private Material[] originalMaterials;

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

        if (highlightMaterial == null) return;

        foreach (var rend in renderers)
        {
            if (active)
            {
                if (rend.sharedMaterial == highlightMaterial) continue;

                if (originalMaterials == null || originalMaterials.Length == 0)
                    originalMaterials = rend.sharedMaterials;

                Material[] glowMats = new Material[rend.sharedMaterials.Length];
                for (int i = 0; i < glowMats.Length; i++) glowMats[i] = highlightMaterial;

                rend.materials = glowMats;
            }
            else
            {
                if (originalMaterials != null && originalMaterials.Length > 0)
                {
                    rend.sharedMaterials = originalMaterials;
                    originalMaterials = null;
                }
            }
        }
    }
}