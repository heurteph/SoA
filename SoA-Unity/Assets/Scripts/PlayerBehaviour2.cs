using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour2 : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private GameObject cameraMain;

    [SerializeField]
    private Inputs inputs;

    private float angle;

    [SerializeField]
    [Range(1.0f, 10.0f)]
    private float speed = 1;
    [SerializeField]
    [Range(1.0f, 360.0f)]
    private float rotationSpeed = 25;

    private Quaternion originalRotation;


    void Awake()
    {
     //    angle = player.transform.rotation.eulerAngles.y;
        inputs = new Inputs();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputs.Player.Walk != null)
        Walk(inputs.Player.Walk.ReadValue<Vector2>());
    }


    void OnEnable()
    {
        inputs.Player.Enable();
    }

    void OnDisable()
    {
        inputs.Player.Disable();
    }


    void Walk(Vector2 v)
    {
        
     /*    Vector3 translationZ = cameraMain.transform.forward*v.y;
         Vector3 translationX = cameraMain.transform.right * v.x;
         characterController.Move((translationX + translationZ)*speed*Time.deltaTime); */


        if (v.magnitude > Mathf.Epsilon)
        {
            transform.rotation = originalRotation * Quaternion.Euler(0, cameraMain.transform.rotation.eulerAngles.y + Mathf.Atan2(v.y, -v.x) * Mathf.Rad2Deg - 90, 0);
            characterController.Move(Vector3.ProjectOnPlane(transform.forward, Vector3.up) * 2.0f * v.magnitude);
        }  

    }




} // FINISH
