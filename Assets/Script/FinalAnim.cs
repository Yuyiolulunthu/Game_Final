using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalAnim : MonoBehaviour
{
    [Header("Animation (Animator)")]
    public Animator animator;        
    public string endParam = "IsEnd";  
    // Start is called before the first frame update
    void Start()
    {
        if (animator) animator.SetBool(endParam, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
