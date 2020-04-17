using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Camera Settings")]
    [Space]

    [SerializeField]
    [Tooltip("The character followed by the camera")]
    private GameObject player;

    [Space]

    [SerializeField]
    [Tooltip("Camera's translation offset from the player's position")]
    private Vector3 cameraOffset = new Vector3(0, 1, -10);
    private Vector3 storedCameraOffset;

    [SerializeField]
    [Tooltip("Camera's angular offset from the player's orientation")]
    private Vector3 cameraAngularOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("Speed at which the camera align itself to the character")]
    [Range(10, 500)]
    private float alignSpeed = 100;

    [Space]
    [Header("Look around")]
    [Space]

    [SerializeField]
    [Tooltip("The held camera (must be a child of the camera holder)")]
    private GameObject heldCamera;

    [SerializeField]
    [Tooltip("Duration to reach the maxLookAroundAngle when the input is pushed at max")]
    [Range(0, 5)]
    private float lookAroundTime = 0.5f; // seconds

    [SerializeField]
    [Tooltip("Max angle of a look around")]
    [Range(10, 90)]
    private float maxLookAroundAngle = 45; // degrees

    private float originalYRotation;
    private Vector3 lastPlayerPosition;
    private Vector2 accumulator = Vector2.zero;

    
    enum STATE { NORMAL, FOCUS, TO_FOCUS, TO_NORMAL };
    STATE recoil;

    [Space]
    [Header("Hurry Mode")]

    [SerializeField]
    [Tooltip("Z-Offset when player is in hurry mode")]
    [Range(-5, 5)]
    private float Z_OffsetHurry = 2.5f;

    [SerializeField]
    [Tooltip("Y-Offset when player is in hurry mode")]
    [Range(-5, 5)]
    private float Y_OffsetHurry = 0;

    [SerializeField]
    [Tooltip("The delay of the focus view on the character")]
    [Range(0, 5)]
    private float timeToFocus;

    [SerializeField]
    [Tooltip("The delay to go back to the normal view")]
    [Range(0, 5)]
    private float timeToNormal;

    private float recoilTimer;

    private float angleToThePlayer;

    private void Awake()
    {
       inputs = new Inputs();
    }

    // Start is called before the first frame update
    void Start()
    {
        storedCameraOffset = cameraOffset;
        originalYRotation  = transform.rotation.eulerAngles.y;
        transform.position = player.transform.position + player.transform.rotation * cameraOffset;
        UpdateRotation(); //transform.LookAt(player.transform);
        lastPlayerPosition = player.transform.position;
        recoil = STATE.NORMAL;
        recoilTimer = 0;
        // compute the angle between the camera in normal view and hurry view
        Vector3 hurryPosition = transform.position + Z_OffsetHurry * transform.forward - Y_OffsetHurry * transform.up;
        angleToThePlayer = Vector3.SignedAngle(Vector3.ProjectOnPlane((player.transform.position - transform.position), Vector3.up), (player.transform.position - hurryPosition).normalized, Vector3.up);
        
        StartCoroutine("AlignWithCharacter");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //UpdatePosition();

        UpdateRotation();

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

    private void UpdatePosition ()
    {
        // if values have changed in the inspector
        UpdateFromInspector();

        // adapt recoil to normal or hurry mode of the player
        Recoil();

        transform.position += (player.transform.position - lastPlayerPosition);
        lastPlayerPosition = player.transform.position;
    }

    private void UpdateRotation ()
    {
        if (recoil == STATE.NORMAL)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((player.transform.position - transform.position), Vector3.up)); // kind of a lookAt but without the rotation around the x-axis
        }
        else if (recoil == STATE.TO_FOCUS) // focus on the character
        {
            Vector3 startPosition  = transform.position - (player.transform.forward * Z_OffsetHurry - player.transform.up * Y_OffsetHurry) * (timeToFocus - recoilTimer) / timeToFocus; // recreate original position
            Vector3 endPosition    = transform.position - (player.transform.forward * Z_OffsetHurry - player.transform.up * Y_OffsetHurry) * recoilTimer / timeToFocus; // recreate original position
            Vector3 start = Vector3.ProjectOnPlane((player.transform.position - startPosition), Vector3.up);
            Vector3 end = (player.transform.position - endPosition).normalized;
            Vector3 current = Vector3.RotateTowards(start, end, angleToThePlayer * Mathf.Deg2Rad * (timeToFocus - recoilTimer) / timeToFocus, 0.0f);
            transform.rotation = Quaternion.LookRotation(current);
        }
        else if (recoil == STATE.FOCUS)
        {
            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
        }
        else if (recoil == STATE.TO_NORMAL)
        {
            Vector3 startPosition = transform.position + (player.transform.forward * Z_OffsetHurry - player.transform.up * Y_OffsetHurry) * (timeToNormal - recoilTimer) / timeToNormal; // recreate original position
            Vector3 endPosition   = transform.position + (player.transform.forward * Z_OffsetHurry - player.transform.up * Y_OffsetHurry) * recoilTimer / timeToNormal; // recreate original position
            Vector3 start = (player.transform.position - startPosition).normalized;
            Vector3 end = Vector3.ProjectOnPlane((player.transform.position - endPosition), Vector3.up);
            Vector3 current = Vector3.RotateTowards(start, end, angleToThePlayer * Mathf.Deg2Rad * (timeToNormal - recoilTimer) / timeToNormal, 0.0f);
            Debug.Log("Angle : " + current + ", timeToNormal : " + timeToNormal + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);
        }

        transform.rotation *= Quaternion.Euler(cameraAngularOffset.x, cameraAngularOffset.y, cameraAngularOffset.z);
    }

    private IEnumerator AlignWithCharacter()
    {
        for(; ;)
        {
            UpdatePosition(); // called here to avoir desynchronization

            float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, player.transform.forward, Vector3.up) % 360;

            float smooth = 0.95f * -Mathf.Pow((Mathf.Abs(angle) / 180f - 1), 2) + 1;  // [0.05-1]

            if (Mathf.Abs(angle) >= alignSpeed * smooth * Time.deltaTime)
            {
                transform.RotateAround(player.transform.position, Vector3.up, Mathf.Sign(angle) * alignSpeed * smooth * Time.deltaTime);
                transform.rotation *= Quaternion.Euler(0, originalYRotation, 0);
            }
            else if (!Mathf.Approximately(angle, 0))
            {
                transform.RotateAround(player.transform.position, Vector3.up, angle);
                transform.rotation *= Quaternion.Euler(0, originalYRotation, 0);
            }
            yield return null;
        }
    }

    void AbsoluteLookAround(Vector2 v)
    {
        heldCamera.transform.rotation *= Quaternion.Euler(90 * -v.y, 60 * v.x, 0);
    }

    void LookAround(Vector2 v)
    {
        float smoothx = 0;
        float smoothy = 0;

        if (!Mathf.Approximately(v.x, 0))
        {
            accumulator.x = Mathf.Clamp(accumulator.x + v.x * Time.deltaTime / lookAroundTime, -1, 1);
            smoothx = Mathf.Sign(accumulator.x) * ( 1 - Mathf.Pow(accumulator.x - Mathf.Sign(accumulator.x), 2)); // f(x) = 1 - (x-1)^2 for x between 0 and 1, f(x) = 1 - (x+1)^2 for x between -1 and 0
        }
        else
        {
            accumulator.x = (1 - Mathf.Sign(accumulator.x)) / 2f * Mathf.Min(accumulator.x - Mathf.Sign(accumulator.x) * Time.deltaTime / lookAroundTime, 0) + (1 + Mathf.Sign(accumulator.x)) / 2f * Mathf.Max(accumulator.x - Mathf.Sign(accumulator.x) * Time.deltaTime / lookAroundTime, 0);
            smoothx = Mathf.Sign(accumulator.x) * Mathf.Pow(accumulator.x, 2); // f(x) = -x^2 for x between 0 and 1, f(x) = x^2 for x between -1 and 0
        }

        if (!Mathf.Approximately(v.y, 0))
        {
            accumulator.y = Mathf.Clamp(accumulator.y + v.y * Time.deltaTime / lookAroundTime, -1, 1);
            smoothy = Mathf.Sign(accumulator.y) * (1 - Mathf.Pow(accumulator.y - Mathf.Sign(accumulator.y), 2)); // f(x) = 1 - (x-1)^2 for x between 0 and 1, f(x) = 1 - (x+1)^2 for x between -1 and 0
        }
        else
        {
            accumulator.y = (1 - Mathf.Sign(accumulator.y)) / 2f * Mathf.Min(accumulator.y - Mathf.Sign(accumulator.y) * Time.deltaTime / lookAroundTime, 0) + (1 + Mathf.Sign(accumulator.y)) / 2f * Mathf.Max(accumulator.y - Mathf.Sign(accumulator.y) * Time.deltaTime / lookAroundTime, 0);
            smoothy = Mathf.Sign(accumulator.y) * Mathf.Pow(accumulator.y, 2);
        }

        heldCamera.transform.localRotation = Quaternion.Euler(-smoothy * maxLookAroundAngle, smoothx * maxLookAroundAngle, 0);
    }

    void UpdateFromInspector()
    {
        if (cameraOffset != storedCameraOffset)
        {
            transform.position += (cameraOffset - storedCameraOffset);
            storedCameraOffset = cameraOffset;
        }
    }

    void Recoil()
    {
        if (recoil == STATE.NORMAL && player.GetComponent<PlayerFirst>().IsHurry)
        {
            recoilTimer = timeToFocus;
            recoil = STATE.TO_FOCUS;
        }

        else if (recoil == STATE.TO_FOCUS)
        {
            recoilTimer -= Time.deltaTime;
            transform.position += (player.transform.forward * Z_OffsetHurry + -player.transform.up * Y_OffsetHurry) * Time.deltaTime / timeToFocus;

            if (recoilTimer <= 0)
            {
                recoil = STATE.FOCUS;
            }
        }

        else if (recoil == STATE.FOCUS && !player.GetComponent<PlayerFirst>().IsHurry)
        {
            recoilTimer = timeToNormal;
            recoil = STATE.TO_NORMAL;
        }

        else if (recoil == STATE.TO_NORMAL)
        {
            recoilTimer -= Time.deltaTime;
            transform.position += (-player.transform.forward * Z_OffsetHurry + player.transform.up * Y_OffsetHurry) * Time.deltaTime / timeToNormal;
            Debug.Log("position : " + transform.position + ", timeToNormal : " + timeToNormal + ", recoilTimer : " + recoilTimer);
            if (recoilTimer <= 0)
            {
                recoil = STATE.NORMAL;
            }
        }
    }

} //FINISH
