using UnityEngine;
using UnityEngine.InputSystem;

public class TestInput : MonoBehaviour
{
    [Header("LEFT HAND ACTIONS")]
    public InputActionReference leftTrigger;          // float
    public InputActionReference leftGrip;             // float
    public InputActionReference leftPrimaryButton;    // float (button)
    public InputActionReference leftSecondaryButton;  // float (button)
    public InputActionReference leftThumbstick;       // Vector2
    public InputActionReference leftThumbstickClick;  // float (button)
    public InputActionReference leftMenuButton;       // float (button, if available)

    [Header("RIGHT HAND ACTIONS")]
    public InputActionReference rightTrigger;
    public InputActionReference rightGrip;
    public InputActionReference rightPrimaryButton;
    public InputActionReference rightSecondaryButton;
    public InputActionReference rightThumbstick;
    public InputActionReference rightThumbstickClick;
    public InputActionReference rightMenuButton;

    private void OnEnable()
    {
        ToggleAll(true);
    }

    private void OnDisable()
    {
        ToggleAll(false);
    }

    private void Update()
    {
        TestSide(
            "Left",
            leftTrigger,
            leftGrip,
            leftPrimaryButton,
            leftSecondaryButton,
            leftThumbstick,
            leftThumbstickClick,
            leftMenuButton
        );

        TestSide(
            "Right",
            rightTrigger,
            rightGrip,
            rightPrimaryButton,
            rightSecondaryButton,
            rightThumbstick,
            rightThumbstickClick,
            rightMenuButton
        );
    }

    private void TestSide(
        string prefix,
        InputActionReference trigger,
        InputActionReference grip,
        InputActionReference primary,
        InputActionReference secondary,
        InputActionReference thumbstick,
        InputActionReference thumbstickClick,
        InputActionReference menu)
    {
        // Trigger
        if (TryReadFloat(trigger, out float triggerValue) && triggerValue > 0.1f)
            UnityEngine.Debug.Log($"{prefix} Trigger: {triggerValue:F2}");

        // Grip
        if (TryReadFloat(grip, out float gripValue) && gripValue > 0.1f)
            UnityEngine.Debug.Log($"{prefix} Grip: {gripValue:F2}");

        // Primary Button
        if (TryReadFloat(primary, out float primaryValue) && primaryValue > 0.5f)
            UnityEngine.Debug.Log($"{prefix} Primary Button pressed");

        // Secondary Button
        if (TryReadFloat(secondary, out float secondaryValue) && secondaryValue > 0.5f)
            UnityEngine.Debug.Log($"{prefix} Secondary Button pressed");

        // Thumbstick 2D axis
        if (TryReadVector2(thumbstick, out Vector2 stick) && stick.sqrMagnitude > 0.01f)
            UnityEngine.Debug.Log($"{prefix} Thumbstick: {stick}");

        // Thumbstick Click
        if (TryReadFloat(thumbstickClick, out float stickClickValue) && stickClickValue > 0.5f)
            UnityEngine.Debug.Log($"{prefix} Thumbstick Click pressed");

        // Menu / System Button
        if (TryReadFloat(menu, out float menuValue) && menuValue > 0.5f)
            UnityEngine.Debug.Log($"{prefix} Menu Button pressed");
    }

    #region Helper methods

    private bool TryReadFloat(InputActionReference actionRef, out float value)
    {
        value = 0f;
        if (actionRef == null || actionRef.action == null)
            return false;

        value = actionRef.action.ReadValue<float>();
        return true;
    }

    private bool TryReadVector2(InputActionReference actionRef, out Vector2 value)
    {
        value = Vector2.zero;
        if (actionRef == null || actionRef.action == null)
            return false;

        value = actionRef.action.ReadValue<Vector2>();
        return true;
    }

    private void ToggleAll(bool enable)
    {
        Toggle(leftTrigger, enable);
        Toggle(leftGrip, enable);
        Toggle(leftPrimaryButton, enable);
        Toggle(leftSecondaryButton, enable);
        Toggle(leftThumbstick, enable);
        Toggle(leftThumbstickClick, enable);
        Toggle(leftMenuButton, enable);

        Toggle(rightTrigger, enable);
        Toggle(rightGrip, enable);
        Toggle(rightPrimaryButton, enable);
        Toggle(rightSecondaryButton, enable);
        Toggle(rightThumbstick, enable);
        Toggle(rightThumbstickClick, enable);
        Toggle(rightMenuButton, enable);
    }

    private void Toggle(InputActionReference actionRef, bool enable)
    {
        if (actionRef == null || actionRef.action == null)
            return;

        var a = actionRef.action;
        if (enable && !a.enabled) a.Enable();
        else if (!enable && a.enabled) a.Disable();
    }

    #endregion
}