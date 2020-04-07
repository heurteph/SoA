using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFirst : MonoBehaviour
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Player Settings")]
    [Space]

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private CharacterController characterController;

    private float angle;

    [Space]
    [SerializeField]
    [Range(1.0f, 10.0f)]
    private float speed = 1;
    [SerializeField]
    [Range(1.0f, 360.0f)]
    private float rotationSpeed = 25;


    void Awake()
    {
        angle = player.transform.rotation.eulerAngles.y;
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
        Vector3 translation = player.transform.forward*v.y;
        characterController.Move(translation*speed*Time.deltaTime);
        angle += v.x * rotationSpeed * Time.deltaTime;
        characterController.transform.rotation = Quaternion.Euler(0,angle,0);
    }

} // FINISH
