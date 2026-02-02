using UnityEngine;

[CreateAssetMenu(fileName = "NewPotLevel", menuName = "Pottery/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Vaza Noua";

    public float[] targetRadii;
    public float[] targetHeights;

    [Range(0.0f, 1.0f)]
    public float accuracyThreshold = 0.90f;
}