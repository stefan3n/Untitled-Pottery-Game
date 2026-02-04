using UnityEngine;

[CreateAssetMenu(fileName = "NewPotLevel", menuName = "Pottery/Level Data")]
public class LevelData : ScriptableObject
{
<<<<<<< Updated upstream
    public string levelName = "Vaza Noua";

    public float[] targetRadii;
    public float[] targetHeights;

    [Range(0.0f, 1.0f)]
    public float accuracyThreshold = 0.90f;
=======
    public string levelName = "Vaza Simpla";
    [Range(0.1f, 1.0f)]
    public float[] targetRadii; 

    public float accuracyThreshold = 0.90f; 
>>>>>>> Stashed changes
}