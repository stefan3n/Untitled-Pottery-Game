using UnityEngine;

public class ShelfPot : MonoBehaviour
{
    public int ShelfSlotIndex { get; private set; }
    private float[] potData;
    private Renderer[] renderers;
    private Material[] originalMaterials;

    public void Initialize(int slotIndex, float[] data)
    {
        ShelfSlotIndex = slotIndex;
        potData = data;
        renderers = GetComponentsInChildren<Renderer>();
    }

    public float[] GetData()
    {
        return potData;
    }

    // Glow highlight pentru vasul din raft
    public void SetHighlight(bool active)
    {
        if (renderers == null) return;

        foreach (var rend in renderers)
        {
            if (active)
            {
                // Salvez materialele
                if (originalMaterials == null || originalMaterials.Length == 0)
                    originalMaterials = rend.sharedMaterials;

                // Creez glow-ul
                Material glowMat = new Material(Shader.Find("Standard"));
                glowMat.color = new Color(0f, 1f, 1f, 0.5f); // Turcoaz transparent

                // Setez stralucirea
                glowMat.EnableKeyword("_EMISSION");
                glowMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.8f) * 2f);

                rend.material = glowMat;
            }
            else
            {
                // Revin la materialele originale
                if (originalMaterials != null)
                {
                    rend.sharedMaterials = originalMaterials;
                }
            }
        }
    }
}