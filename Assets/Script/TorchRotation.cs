using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchRotation : MonoBehaviour
{
    [Header("旋轉設定")]
    [Tooltip("Player 物件的 Transform")]
    public Transform player;
    
    [Tooltip("旋轉速度（度/秒）")]
    public float rotationSpeed = 90f;
    
    [Tooltip("與 Player 的距離")]
    public float distanceFromPlayer = 2f;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("找不到 Player ");
            }
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float rotationAmount = 0f;
        
        // 按 J 鍵 - 逆時針旋轉
        if (Input.GetKey(KeyCode.J))
        {
            rotationAmount = rotationSpeed * Time.deltaTime;
        }
        // 按 L 鍵 - 順時針旋轉
        else if (Input.GetKey(KeyCode.L))
        {
            rotationAmount = -rotationSpeed * Time.deltaTime;
        }
        
        if (rotationAmount != 0f)
        {
            transform.RotateAround(player.position, Vector3.up, rotationAmount);
            
            // 讓手電筒一直朝向 Player
            // transform.LookAt(player);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, distanceFromPlayer);
            Gizmos.DrawLine(player.position, transform.position);
        }
    }
}