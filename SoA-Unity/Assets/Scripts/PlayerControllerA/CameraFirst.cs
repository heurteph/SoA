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

    [SerializeField]
    [Tooltip("Camera's angular offset from the player's orientation")]
    private Vector3 cameraAngularOffset;

    [Space]
    [Header("Position (Spherical Coordinates)")]
    [Space]

    [SerializeField]
    [Tooltip("Distance to the player")]
    private float radius = 1f;
    private float storedRadius;

    [SerializeField]
    [Tooltip("Longitude in degrees")]
    [Range(-180, 180)]
    private float longitude = 0f;
    private float storedLongitude;

    [SerializeField]
    [Tooltip("Latitude in degrees")]
    [Range(-90, 90)]
    private float latitude = 0f;
    private float storedLatitude;

    [Space]
    [Header("Position (Cartesian Coordinates)")]
    [Space]

    [SerializeField]
    [Tooltip("Camera's position in cartesians coordinates relative to the player's position")]
    private Vector3 cameraOffset;
    private Vector3 storedCameraOffset;

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

    /* Spherical-Cartesian conversion functions */

    private void SphericalToCartesian()
    {
        cameraOffset.x = radius * Mathf.Cos(longitude * Mathf.Deg2Rad) * Mathf.Cos(latitude * Mathf.Deg2Rad);
        cameraOffset.y = radius * Mathf.Sin(longitude * Mathf.Deg2Rad) * Mathf.Cos(latitude * Mathf.Deg2Rad);
        cameraOffset.z = radius * Mathf.Sin(latitude * Mathf.Deg2Rad);

        storedCameraOffset = cameraOffset;
    }

    public void CartesianToSpherical()
    {
        radius = cameraOffset.magnitude;
        longitude = Mathf.Atan(Mathf.Sqrt(Mathf.Pow(cameraOffset.x,2) + Mathf.Pow(cameraOffset.y,2)) / cameraOffset.z) * Mathf.Rad2Deg;
        latitude = Mathf.Atan(cameraOffset.x / cameraOffset.y) * Mathf.Rad2Deg;

        storedLatitude = latitude;
        storedLongitude = longitude;
        storedRadius = radius;
    }

    private void OnValidate()
    {
        if (storedCameraOffset != cameraOffset)
        {
            storedCameraOffset = cameraOffset;
            CartesianToSpherical();
        }

        else if (storedRadius != radius)
        {
            storedRadius = radius;
            SphericalToCartesian();
        }
        else if (storedLongitude != longitude)
        {
            storedLongitude = longitude;
            SphericalToCartesian();
        }
        else if (storedLatitude != latitude)
        {
            storedLatitude = latitude;
            SphericalToCartesian();
        }
    }


} //FINISH
