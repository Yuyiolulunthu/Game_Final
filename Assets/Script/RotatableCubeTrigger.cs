using UnityEngine;

public class RotatableCubeTrigger : MonoBehaviour
{
    public DirectionalLightRotator lightRotator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lightRotator.StartRotation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lightRotator.StopRotation();
        }
    }
}
