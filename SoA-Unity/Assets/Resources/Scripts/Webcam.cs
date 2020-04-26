using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WebCamTexture web = new WebCamTexture();
        Renderer rend = GetComponent<Renderer>();
        rend.material.mainTexture = web;
        web.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
