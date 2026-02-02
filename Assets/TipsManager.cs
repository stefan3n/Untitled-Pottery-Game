using System.Collections;
using TMPro; 
using UnityEngine;

public class TipsManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI tipTextComponent; 

    [Header("Settings")]
    public float changeInterval = 30f; 

    [Header("Messages")]
    [TextArea(2, 5)] 
    public string[] tipsList; 

    private void Start()
    {
        if (tipsList.Length > 0)
        {
            ShowRandomTip();

            StartCoroutine(TipsCycle());
        }
        else
        {
            Debug.LogWarning("Nu ai adaugat niciun mesaj in lista 'Tips List'!");
        }
    }

    IEnumerator TipsCycle()
    {
        while (true) 
        {
            yield return new WaitForSeconds(changeInterval);
            ShowRandomTip();
        }
    }

    void ShowRandomTip()
    {
        int randomIndex = Random.Range(0, tipsList.Length);
        tipTextComponent.text = tipsList[randomIndex];
    }
}