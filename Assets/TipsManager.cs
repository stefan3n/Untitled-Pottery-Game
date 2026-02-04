using UnityEngine;
using TMPro;
using System.Collections;

public class TipsManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI tipText;

    [Header("Settings")]
    public float changeInterval = 20f; 

    [TextArea(3, 10)] 
    public string[] tipsList; 

    private int lastIndex = -1;

    void Start()
    {
        if (tipText != null && tipsList.Length > 0)
        {
            StartCoroutine(CycleTipsRoutine());
        }
        else
        {
            Debug.LogWarning("TipsManager: Missing text or empty list!");
        }
    }

    IEnumerator CycleTipsRoutine()
    {
        ShowRandomTip();

        while (true) 
        {
            yield return new WaitForSeconds(changeInterval);

            ShowRandomTip();
        }
    }

    void ShowRandomTip()
    {
        if (tipsList.Length == 0) return;

        int newIndex;

        do
        {
            newIndex = Random.Range(0, tipsList.Length);
        }
        while (tipsList.Length > 1 && newIndex == lastIndex);

        lastIndex = newIndex;
        tipText.text = tipsList[newIndex];
    }
}