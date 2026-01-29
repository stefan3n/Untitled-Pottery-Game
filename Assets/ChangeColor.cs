using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ChangeTargetObjectColorButton : MonoBehaviour
{
    [Header("The object whose color will be changed")]
    public GameObject targetObject;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ChangeTargetColor);
    }

    public void ChangeTargetColor()
    {
        Color newColor = Color.white;
        Graphic buttonGraphic = button.targetGraphic;

        if (buttonGraphic != null)
        {
            newColor = buttonGraphic.color; 
        }

        if (targetObject == null)
        {
            return;
        }

        PaintBrush brush = targetObject.GetComponent<PaintBrush>();
        if (brush != null)
        {
            brush.SetBrushColor(newColor);
        }
    }
}