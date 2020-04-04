﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIco : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Inputs inputs;

    [SerializeField]
    private float offsetZCamera = 0f;

    [SerializeField]
    private float offsetYCamera = 0f;

    [SerializeField]
    private float offsetXCamera = 0f;

    [SerializeField]
    private float cameraAngleX, cameraAngleY, cameraAngleZ;

    private void Awake()
    {
       inputs = new Inputs();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateCamera();

        LookAround(inputs.Player.LookAround.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
    }

    private void OnDisable()
    {
        inputs.Player.Disable();
    }

    void LookAround(Vector2 v)
    {
        transform.rotation *= Quaternion.Euler( 90*v.y, 60*v.x, 0);
    }


    private void UpdateCamera ()
    {
        transform.position = player.transform.position + player.transform.forward.normalized * (-offsetZCamera);

        transform.rotation = player.transform.rotation;

        transform.position += offsetYCamera*transform.up + offsetXCamera*transform.right;

        transform.rotation *= Quaternion.Euler(cameraAngleX, cameraAngleY, cameraAngleZ);

    }


} //FINISH
