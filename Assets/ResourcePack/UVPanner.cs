using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Renderer))]
public class UVPanner : MonoBehaviour {
    public Vector2 speed = new Vector2(0.005f, -0.003f);
    Renderer r;
    void Awake(){ r = GetComponent<Renderer>(); }
    void Update(){ r.material.mainTextureOffset += speed * Time.deltaTime; }
}
