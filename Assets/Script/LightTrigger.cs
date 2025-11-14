using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    public Light directionalLight; 
    public float targetIntensity = 1.5f;
    public float speed = 2f;

    private bool isInside = false;
    private float originalIntensity;

    void Start()
    {
        if (directionalLight != null) originalIntensity = directionalLight.intensity;
    }

    void Update()
    {
        if (directionalLight == null) return;
        float target = isInside ? targetIntensity : originalIntensity;
        directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, target, Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isInside = true;
    }
}
