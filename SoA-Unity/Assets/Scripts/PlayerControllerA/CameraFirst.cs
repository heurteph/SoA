using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFirst : MonoBehaviour
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Camera Settings")]
    [Space]

    [SerializeField]
    private GameObject player;

    [Space]

    [SerializeField]
    [Tooltip("Distance to the player")]
    [Range(1, 20)]
    private float radius = 1f;

    [SerializeField]
    [Tooltip("Latitude in degrees")]
    [Range(-90, 90)]
    private float latitude = 0f;

    [SerializeField]
    [Tooltip("Longitude in degrees")]
    [Range(-180, 180)]
    private float longitude = 0f;

    [Space]

    [SerializeField]
    [Tooltip("Camera's angular offset from the player's orientation")]
    private Vector3 cameraAngularOffset;

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
        transform.position = player.transform.position + Quaternion.Euler(90, 0, 0) * new Vector3 (radius * Mathf.Cos(Mathf.Deg2Rad * latitude) * Mathf.Cos(Mathf.Deg2Rad * longitude), 
                                                                      radius * Mathf.Cos(Mathf.Deg2Rad * latitude) * Mathf.Sin(Mathf.Deg2Rad * longitude),
                                                                      radius * Mathf.Sin(Mathf.Deg2Rad * latitude));

        transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);



        transform.rotation *= Quaternion.Euler(cameraAngularOffset.x, cameraAngularOffset.y, cameraAngularOffset.z);

    }


} //FINISH
