using UnityEngine;
using UnityEngine.UI;

public class AlwaysOnTopFeature : MonoBehaviour
{
    void Start()
    {
        Image img = GetComponent<Image>();

        if (img != null)
        {
            Material newMat = new Material(img.material);

            newMat.SetInt("_ZTest", 8);

            if (newMat.HasProperty("unity_GUIZTestMode"))
            {
                newMat.SetInt("unity_GUIZTestMode", 8);
            }

            img.material = newMat;
            Debug.Log("AlwaysOnTop activat pe: " + gameObject.name);
        }
        else
        {
            Debug.LogError("Nu am gasit componenta Image pe " + gameObject.name);
        }
    }
}