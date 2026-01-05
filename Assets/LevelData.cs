using UnityEngine;

[CreateAssetMenu(fileName = "NewPotLevel", menuName = "Pottery/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Vaza Simpla";
    [Range(0.1f, 1.0f)]
    public float[] targetRadii; // Aici stocam forma tintei

    public float accuracyThreshold = 0.90f; // Precizie necesara
}