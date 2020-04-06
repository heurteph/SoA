using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Player settings")]
    [Space]

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private GameObject cameraMain;

    [Space]
    [SerializeField]
    [Range(1.0f, 10.0f)]
    private float speed = 1;


    void Awake()
    {
     //    angle = player.transform.rotation.eulerAngles.y;
        inputs = new Inputs();
    }

    // Start is called before the first frame update
    void Start()
    {

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
        
         /* Vector3 translationZ = cameraMain.transform.forward*v.y;
         Vector3 translationX = cameraMain.transform.right * v.x;
         characterController.Move((translationX + translationZ)*speed*Time.deltaTime); */

        if (v.magnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.Euler(0, cameraMain.transform.rotation.eulerAngles.y + Mathf.Rad2Deg * Mathf.Atan2(v.x, v.y), 0);   // cartesian to polar, starting from the Y+ axis as it's the one mapped to the camera's forward, thus using tan-1(x,y) and not tan-1(y,x) / No rotationSpeed * Time.deltaTime as it takes absolute orientation
            characterController.Move(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * v.magnitude * speed * Time.deltaTime);  // projection normalized to have the speed independant from the camera angle
        }  
    }




} // FINISH
