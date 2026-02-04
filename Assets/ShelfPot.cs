using UnityEngine;

public class ShelfPot : MonoBehaviour
{
    public int ShelfSlotIndex { get; private set; }

    private float[] savedRadii;
    private float[] savedHeights;
<<<<<<< Updated upstream
    private Renderer[] renderers;
    private Material[] originalMaterials;

    public void Initialize(int slotIndex, float[] radii, float[] heights)
=======
    private Texture2D savedTextureOutside;
    private Texture2D savedTextureInside;

    public bool WasPainted { get; private set; }

    private Renderer[] renderers;
    private Material[] originalMaterials;

    [Header("Visuals")]
    public Material highlightMaterial;

    public void Initialize(int slotIndex, float[] radii, float[] heights, Texture2D texOut, Texture2D texIn, bool wasPainted)
>>>>>>> Stashed changes
    {
        ShelfSlotIndex = slotIndex;
        savedRadii = radii;
        savedHeights = heights;
<<<<<<< Updated upstream
=======
        savedTextureOutside = texOut;
        savedTextureInside = texIn;
        WasPainted = wasPainted;

>>>>>>> Stashed changes
        renderers = GetComponentsInChildren<Renderer>();
    }

    public float[] GetRadii() => savedRadii;
    public float[] GetHeights() => savedHeights;
<<<<<<< Updated upstream
=======
    public Texture2D GetTextureOutside() => savedTextureOutside;
    public Texture2D GetTextureInside() => savedTextureInside;
>>>>>>> Stashed changes

    public void SetHighlight(bool active)
    {
        if (renderers == null) return;

        if (highlightMaterial == null) return;

        foreach (var rend in renderers)
        {
            if (active)
            {
<<<<<<< Updated upstream
                if (originalMaterials == null || originalMaterials.Length == 0)
                    originalMaterials = rend.sharedMaterials;

                Material glowMat = new Material(Shader.Find("Standard"));
                glowMat.color = new Color(0f, 1f, 1f, 0.5f);
                glowMat.EnableKeyword("_EMISSION");
                glowMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.8f) * 2f);

                rend.material = glowMat;
            }
            else
            {
                if (originalMaterials != null)
=======
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
>>>>>>> Stashed changes
                {
                    rend.sharedMaterials = originalMaterials;
                    originalMaterials = null;
                }
            }
        }
    }
}