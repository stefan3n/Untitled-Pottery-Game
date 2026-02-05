using UnityEngine;

[CreateAssetMenu(fileName = "NewPotLevel", menuName = "Pottery/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Vaza Simpla";

    [Header("Data")]
    public float[] targetRadii;   
    public float[] targetHeights; 

    public float accuracyThreshold = 0.90f;
}