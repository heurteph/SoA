﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuggerBehaviour : MonoBehaviour
{

    [SerializeField]
    private RawImage grayRawImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void DisplayTexture2D (Texture2D t2D) 
    {
        grayRawImage.texture = t2D;

    }
} // FINISH
