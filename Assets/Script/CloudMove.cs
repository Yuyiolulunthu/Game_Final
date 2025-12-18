using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public float range = 10f;  
    public float speed = 5f;   

    private float startX;
    private bool ended = false;

    void Start()
    {
        startX = transform.position.x;
    }
    void OnEnable()
    {
        PlayerMove.OnGameEnd += HandleEnd;
    }
    void OnDisable()
    {
        PlayerMove.OnGameEnd -= HandleEnd;
    }
    void HandleEnd()
    {
        ended = true;
    }

    void Update()
    {
        if (ended) return;
        float x = startX + Mathf.PingPong(Time.time * speed, range * 2);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}