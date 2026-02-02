using UnityEngine;

public class ShelfPot : MonoBehaviour
{
    public int ShelfSlotIndex { get; private set; }

    private float[] savedRadii;
    private float[] savedHeights;
    private Renderer[] renderers;
    private Material[] originalMaterials;

    public void Initialize(int slotIndex, float[] radii, float[] heights)
    {
        ShelfSlotIndex = slotIndex;
        savedRadii = radii;
        savedHeights = heights;
        renderers = GetComponentsInChildren<Renderer>();
    }

    public float[] GetRadii() => savedRadii;
    public float[] GetHeights() => savedHeights;

    public void SetHighlight(bool active)
    {
        if (renderers == null) return;

        foreach (var rend in renderers)
        {
            if (active)
            {
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
                {
                    rend.sharedMaterials = originalMaterials;
                }
            }
        }
    }
}