using UnityEngine;

public class HandSensor : MonoBehaviour
{
    public PotWorkflowController mainController;

    private void OnTriggerEnter(Collider other)
    {
        if (mainController != null)
        {
            mainController.ReportHandEnter(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (mainController != null)
        {
            mainController.ReportHandExit(other);
        }
    }
}