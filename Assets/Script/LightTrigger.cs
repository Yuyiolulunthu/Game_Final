using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightTrigger : MonoBehaviour
{
    public Light directionalLight; 
    public float targetIntensity = 1.5f;
    public float speed = 2f;

    private bool isInside = false;
    private bool sceneLoaded = false;
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
        if (isInside && !sceneLoaded && Mathf.Abs(directionalLight.intensity - targetIntensity) < 0.05f)
        {
            sceneLoaded = true;
            SceneManager.LoadScene("Scene_Menu");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isInside = true;
    }
}
