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

    private float originalYRotation;
    private Vector3 lastPlayerPosition;

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
    [Tooltip("Max horizontal angle of a look around")]
    [Range(5, 90)]
    private float maxHorizontalAngle = 45; // degrees

    [SerializeField]
    [Tooltip("Max vertical angle of a look around")]
    [Range(5, 90)]
    private float maxVerticalAngle = 45; // degrees

    [SerializeField]
    [Tooltip("Duration to reach the maxHorizontalLookAroundAngle when the input is pushed at max")]
    [Range(0.1f, 5)]
    private float horizontalDuration = 0.5f; // seconds

    [SerializeField]
    [Tooltip("Duration to reach the maxVerticalLookAroundAngle when the input is pushed at max")]
    [Range(0.1f, 5)]
    private float verticalDuration = 0.5f; // seconds

    private Vector2 accumulator = Vector2.zero;

    enum STATE { 
        NORMAL,
        HURRY,
        PROTECTED,
        NORMAL_TO_HURRY,
        HURRY_TO_NORMAL,
        HURRY_TO_PROTECTED,
        NORMAL_TO_PROTECTED,
        PROTECTED_TO_NORMAL,
        PROTECTED_TO_HURRY
    };
    STATE cameraState;

    [Space]
    [Header("Normal Mode")]

    [SerializeField]
    [Tooltip("The delay to switch from hurry to normal view, in seconds")]
    [Range(0.1f, 5)]
    private float timeHurryToNormal = 1.2f;

    [SerializeField]
    [Tooltip("The delay to switch from protected to normal view, in seconds")]
    [Range(0.1f, 5)]
    private float timeProtectedToNormal = 1.2f;

    [Space]
    [Header("Hurry Mode")]

    [SerializeField]
    [Tooltip("Z-Offset when player is in hurry mode (-closer, +farther)")]
    [Range(-10, 10)]
    private float Z_OffsetHurry = 2.5f;

    [SerializeField]
    [Tooltip("Y-Offset when player is in hurry mode (-closer, +farther)")]
    [Range(-10, 10)]
    private float Y_OffsetHurry = 0;

    [SerializeField]
    [Tooltip("The delay to switch from normal to hurry view, in seconds")]
    [Range(0.1f, 5)]
    private float timeNormalToHurry = 0.4f;

    [SerializeField]
    [Tooltip("The delay to switch from protected to hurry view, in seconds")]
    [Range(0.1f, 5)]
    private float timeProtectedToHurry = 0.2f;

    [Space]
    [Header("Protected Mode")]

    [SerializeField]
    [Tooltip("Z-Offset when player is in protected mode (-closer, +farther)")]
    [Range(-10, 10)]
    private float Z_OffsetProtected = 2.5f;

    [SerializeField]
    [Tooltip("Y-Offset when player is in hurry mode (-closer, +farther)")]
    [Range(-10, 10)]
    private float Y_OffsetProtected = 0;

    [SerializeField]
    [Tooltip("The delay to switch from normal to protected view, in seconds")]
    [Range(0.1f, 5)]
    private float timeNormalToProtected = 0.4f;

    [SerializeField]
    [Tooltip("The delay to switch from hurry to protected view, in seconds")]
    [Range(0.1f, 5)]
    private float timeHurryToProtected = 0.2f;

    private float zoomTimer;

    private float angleFromHurryToHorizon = 0;
    private float angleFromProtectedToHorizon = 0;

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

        // compute the angle between the camera in normal view and hurry view for the look-around stabilization
        Vector3 hurryPosition = transform.position - Z_OffsetHurry * Vector3.ProjectOnPlane((player.transform.position - transform.position).normalized, Vector3.up) + Y_OffsetHurry * Vector3.up;
        angleFromHurryToHorizon = Vector3.Angle(Vector3.ProjectOnPlane((player.transform.position - transform.position), Vector3.up).normalized, (player.transform.position - hurryPosition).normalized);
        Debug.Log("angleFromHurryToHorizon : " + angleFromHurryToHorizon);

        // compute the angle between the camera in normal view and protected view for the look-around stabilization
        Vector3 protectedPosition = transform.position - Z_OffsetProtected * Vector3.ProjectOnPlane((player.transform.position - transform.position).normalized, Vector3.up) + Y_OffsetProtected * Vector3.up;
        angleFromProtectedToHorizon = Vector3.Angle(Vector3.ProjectOnPlane((player.transform.position - transform.position), Vector3.up).normalized, (player.transform.position - protectedPosition).normalized);
        Debug.Log("angleFromProtectedToHorizon : " + angleFromProtectedToHorizon);

        cameraState = STATE.NORMAL;
        zoomTimer = 0;

        StartCoroutine("AlignWithCharacter");
    }
    void UpdateFromInspector()
    {
        if (cameraOffset != storedCameraOffset)
        {
            transform.position += (cameraOffset - storedCameraOffset);
            storedCameraOffset = cameraOffset;
        }
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
    }

    private void OnDisable()
    {
        inputs.Player.Disable();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //UpdatePosition();

        UpdateRotation();

        LookAround(inputs.Player.LookAround.ReadValue<Vector2>());
    }

    private void UpdatePosition ()
    {
        // if values have changed in the inspector
        UpdateFromInspector();

        // adapt recoil to normal, hurry or protected mode from the player
        UpdateRecoilPosition();

        transform.position += (player.transform.position - lastPlayerPosition);
        lastPlayerPosition = player.transform.position;
    }

    private void UpdateRotation ()
    {
        if (cameraState == STATE.NORMAL)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((player.transform.position - transform.position), Vector3.up)); // kind of a lookAt but without the rotation around the x-axis
        }
        else if (cameraState == STATE.NORMAL_TO_HURRY) // focus on the character
        {
            Vector3 startPosition  = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * (timeNormalToHurry - zoomTimer) / timeNormalToHurry; // recreate original position
            Vector3 endPosition    = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * zoomTimer / timeNormalToHurry; // recreate original position
            Vector3 start   = Vector3.ProjectOnPlane((player.transform.position - startPosition), Vector3.up).normalized;
            Vector3 end     = (player.transform.position - endPosition).normalized;
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToFocus - recoilTimer) / timeToFocus);
            Vector3 current = Vector3.Slerp(start, end, (timeNormalToHurry - zoomTimer) / timeNormalToHurry);
            //Debug.Log("Start : " + start + ", End : " + end + ", Angle : " + current + ", timeToFocus : " + timeToFocus + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);

            //heldCamera.GetComponent<Camera>().fieldOfView = 60 - (60 - 50) * (timeToFocus - recoilTimer) / timeToFocus;
        }
        else if (cameraState == STATE.HURRY)
        {
            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
        }

        else if (cameraState == STATE.PROTECTED)
        {
            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
        }
        else if (cameraState == STATE.HURRY_TO_NORMAL)
        {
            Vector3 startPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * (timeHurryToNormal - zoomTimer) / timeHurryToNormal; // recreate original position
            Vector3 endPosition   = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * zoomTimer / timeHurryToNormal; // recreate original position
            Vector3 start   = (player.transform.position - startPosition).normalized;
            Vector3 end     = Vector3.ProjectOnPlane((player.transform.position - endPosition), Vector3.up).normalized;
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToNormal - recoilTimer) / timeToNormal);
            Vector3 current = Vector3.Slerp(start, end, (timeHurryToNormal - zoomTimer) / timeHurryToNormal);
            //Debug.Log("Start : " + start + ", End : " + end + ", Angle : " + current + ", timeToNormal : " + timeToNormal + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);

            //heldCamera.GetComponent<Camera>().fieldOfView = 60 - (60 - 50) * recoilTimer / timeToNormal;
        }
        else if (cameraState == STATE.NORMAL_TO_PROTECTED) // focus on the character
        {
            Vector3 startPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * (timeNormalToProtected - zoomTimer) / timeNormalToProtected; // recreate original position
            Vector3 endPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * zoomTimer / timeNormalToProtected; // recreate original position
            Vector3 start = Vector3.ProjectOnPlane((player.transform.position - startPosition), Vector3.up).normalized;
            Vector3 end = (player.transform.position - endPosition).normalized;
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToFocus - recoilTimer) / timeToFocus);
            Vector3 current = Vector3.Slerp(start, end, (timeNormalToProtected - zoomTimer) / timeNormalToProtected);
            //Debug.Log("Start : " + start + ", End : " + end + ", Angle : " + current + ", timeToFocus : " + timeToFocus + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);

            //heldCamera.GetComponent<Camera>().fieldOfView = 60 - (60 - 50) * (timeToFocus - recoilTimer) / timeToFocus;
        }
        else if (cameraState == STATE.PROTECTED_TO_NORMAL)
        {
            Vector3 startPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * (timeProtectedToNormal - zoomTimer) / timeProtectedToNormal; // recreate original position
            Vector3 endPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * zoomTimer / timeProtectedToNormal; // recreate original position
            Vector3 start = (player.transform.position - startPosition).normalized;
            Vector3 end = Vector3.ProjectOnPlane((player.transform.position - endPosition), Vector3.up).normalized;
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToNormal - recoilTimer) / timeToNormal);
            Vector3 current = Vector3.Slerp(start, end, (timeProtectedToNormal - zoomTimer) / timeProtectedToNormal);
            //Debug.Log("Start : " + start + ", End : " + end + ", Angle : " + current + ", timeToNormal : " + timeToNormal + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);

            //heldCamera.GetComponent<Camera>().fieldOfView = 60 - (60 - 50) * recoilTimer / timeToNormal;
        }
        else if (cameraState == STATE.PROTECTED_TO_HURRY)
        {
            // TO DEBUG
            /*
            Vector3 startPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * -(Z_OffsetProtected - Z_OffsetHurry) + Vector3.up * -(Y_OffsetProtected - Y_OffsetHurry)) * (timeProtectedToHurry - zoomTimer) / timeProtectedToHurry; // recreate original position
            Vector3 endPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * -(Z_OffsetProtected - Z_OffsetHurry) + Vector3.up * -(Y_OffsetProtected - Y_OffsetHurry)) * zoomTimer / timeProtectedToHurry; // recreate original position
            Vector3 start = (player.transform.position - startPosition).normalized; // no projection on both start and end
            Vector3 end = (player.transform.position - endPosition).normalized;
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToFocus - recoilTimer) / timeToFocus);
            Vector3 current = Vector3.Slerp(start, end, (timeProtectedToHurry - zoomTimer) / timeProtectedToHurry);
            //Debug.Log("Start : " + start + ", End : " + end + ", Angle : " + current + ", timeToFocus : " + timeToFocus + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);
        */


            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
            //heldCamera.GetComponent<Camera>().fieldOfView = 60 - (60 - 50) * (timeToFocus - recoilTimer) / timeToFocus;
        }
        else if (cameraState == STATE.HURRY_TO_PROTECTED)
        {
            // TO DEBUG

            /*
            Vector3 startPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * -(Z_OffsetHurry - Z_OffsetProtected) + Vector3.up * -(Y_OffsetHurry - Y_OffsetProtected)) * (timeHurryToProtected - zoomTimer) / timeHurryToProtected; // recreate original position
            Vector3 endPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * -(Z_OffsetHurry - Z_OffsetProtected) + Vector3.up * -(Y_OffsetHurry - Y_OffsetProtected)) * zoomTimer / timeHurryToProtected; // recreate original position
            Vector3 start = (player.transform.position - startPosition).normalized; // no projection on both start and end
            Vector3 end = (player.transform.position - endPosition).normalized;
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToFocus - recoilTimer) / timeToFocus);
            Vector3 current = Vector3.Slerp(start, end, (timeHurryToProtected - zoomTimer) / timeHurryToProtected);
            //Debug.Log("Start : " + start + ", End : " + end + ", Angle : " + current + ", timeToFocus : " + timeToFocus + ", recoilTimer : " + recoilTimer);
            transform.rotation = Quaternion.LookRotation(current);*/

            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);

            //heldCamera.GetComponent<Camera>().fieldOfView = 60 - (60 - 50) * (timeToFocus - recoilTimer) / timeToFocus;
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

    void LookAround(Vector2 v)
    {
        float smoothx = 0;
        float smoothy = 0;

        if (!Mathf.Approximately(v.x, 0))
        {
            accumulator.x = Mathf.Clamp(accumulator.x + v.x * Time.deltaTime / horizontalDuration, -1, 1);
            //smoothx = Mathf.Sign(accumulator.x) * ( 1 - Mathf.Pow(accumulator.x - Mathf.Sign(accumulator.x), 2)); // f(x) = 1 - (x-1)^2 for x between 0 and 1, f(x) = 1 - (x+1)^2 for x between -1 and 0
            //smoothx = Mathf.Sign(accumulator.x) * Mathf.SmoothStep(0.0f, 1.0f, Mathf.Abs(accumulator.x));
            smoothx = Mathf.Sign(accumulator.x) * Mathf.Sin(Mathf.Abs(accumulator.x) * Mathf.PI * 0.5f);
        }
        else
        {
            accumulator.x = (1 - Mathf.Sign(accumulator.x)) / 2f * Mathf.Min(accumulator.x - Mathf.Sign(accumulator.x) * Time.deltaTime / horizontalDuration, 0) + (1 + Mathf.Sign(accumulator.x)) / 2f * Mathf.Max(accumulator.x - Mathf.Sign(accumulator.x) * Time.deltaTime / horizontalDuration, 0);
            //smoothx = Mathf.Sign(accumulator.x) * Mathf.Pow(accumulator.x, 2); // f(x) = -x^2 for x between 0 and 1, f(x) = x^2 for x between -1 and 0
            //smoothx = Mathf.Sign(accumulator.x) * Mathf.SmoothStep(0.0f, 1.0f, Mathf.Abs(accumulator.x));
            smoothx = Mathf.Sign(accumulator.x) * Mathf.Sin(Mathf.Abs(accumulator.x) * Mathf.PI * 0.5f);
        }

        if (!Mathf.Approximately(v.y, 0))
        {
            accumulator.y = Mathf.Clamp(accumulator.y + v.y * Time.deltaTime / verticalDuration, -1, 1);
            //smoothy = Mathf.Sign(accumulator.y) * (1 - Mathf.Pow(accumulator.y - Mathf.Sign(accumulator.y), 2)); // f(x) = 1 - (x-1)^2 for x between 0 and 1, f(x) = 1 - (x+1)^2 for x between -1 and 0
            //smoothy = Mathf.Sign(accumulator.y) * Mathf.SmoothStep(0.0f, 1.0f, Mathf.Abs(accumulator.y));
            smoothy = Mathf.Sign(accumulator.y) * Mathf.Sin(Mathf.Abs(accumulator.y) * Mathf.PI * 0.5f);
        }
        else
        {
            accumulator.y = (1 - Mathf.Sign(accumulator.y)) / 2f * Mathf.Min(accumulator.y - Mathf.Sign(accumulator.y) * Time.deltaTime / verticalDuration, 0) + (1 + Mathf.Sign(accumulator.y)) / 2f * Mathf.Max(accumulator.y - Mathf.Sign(accumulator.y) * Time.deltaTime / verticalDuration, 0);
            //smoothy = Mathf.Sign(accumulator.y) * Mathf.Pow(accumulator.y, 2);
            //smoothy = Mathf.Sign(accumulator.y) * Mathf.SmoothStep(0.0f, 1.0f, Mathf.Abs(accumulator.y));
            smoothy = Mathf.Sign(accumulator.y) * Mathf.Sin(Mathf.Abs(accumulator.y) * Mathf.PI * 0.5f);
        }

        // Stabilization of the look around
        float y_stabilization = 0;
        if      (cameraState == STATE.HURRY)           { y_stabilization = Mathf.Abs(smoothx) * -angleFromHurryToHorizon; }
        else if (cameraState == STATE.NORMAL_TO_HURRY) { y_stabilization = Mathf.Abs(smoothx) * -angleFromHurryToHorizon * (timeNormalToHurry - zoomTimer) / timeNormalToHurry; }
        else if (cameraState == STATE.HURRY_TO_NORMAL) { y_stabilization = Mathf.Abs(smoothx) * -angleFromHurryToHorizon * zoomTimer / timeHurryToNormal; }

        else if (cameraState == STATE.PROTECTED)           { y_stabilization = Mathf.Abs(smoothx) * -angleFromProtectedToHorizon; }
        else if (cameraState == STATE.NORMAL_TO_PROTECTED) { y_stabilization = Mathf.Abs(smoothx) * -angleFromProtectedToHorizon * (timeNormalToProtected - zoomTimer) / timeNormalToProtected; }
        else if (cameraState == STATE.PROTECTED_TO_NORMAL) { y_stabilization = Mathf.Abs(smoothx) * -angleFromProtectedToHorizon * zoomTimer / timeProtectedToNormal; }

        else if (cameraState == STATE.HURRY_TO_PROTECTED) { y_stabilization = Mathf.Abs(smoothx) * (-angleFromHurryToHorizon * zoomTimer / timeHurryToProtected - angleFromProtectedToHorizon * (timeHurryToProtected - zoomTimer) / timeHurryToProtected); }
        else if (cameraState == STATE.PROTECTED_TO_HURRY) { y_stabilization = Mathf.Abs(smoothx) * (-angleFromProtectedToHorizon * zoomTimer / timeProtectedToHurry - angleFromHurryToHorizon * (timeProtectedToHurry - zoomTimer) / timeProtectedToHurry); }

        // Must be separated in two because unity's order for euler is ZYX and we want X-Y-X
        heldCamera.transform.localRotation  = Quaternion.Euler(y_stabilization, 0, 0);
        heldCamera.transform.localRotation *= Quaternion.Euler(0, smoothx * maxHorizontalAngle, 0);
        heldCamera.transform.localRotation *= Quaternion.Euler(-smoothy * maxVerticalAngle, 0, 0);
        
        //heldCamera.transform.localRotation = Quaternion.Euler(-smoothy * maxLookAroundAngle, smoothx * maxLookAroundAngle, 0);
    }

    void UpdateRecoilPosition()
    {
        if (cameraState == STATE.NORMAL)
        {
            if (player.GetComponent<PlayerFirst>().IsProtected)
            {
                zoomTimer = timeNormalToProtected;
                cameraState = STATE.NORMAL_TO_PROTECTED;
            }

            else if (player.GetComponent<PlayerFirst>().IsHurry)
            {
                zoomTimer = timeNormalToHurry;
                cameraState = STATE.NORMAL_TO_HURRY;
            }
        }

        else if (cameraState == STATE.NORMAL_TO_HURRY)
        {
            Vector3 startPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position,Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * (timeNormalToHurry - zoomTimer) / timeNormalToHurry; // recreate original position
            Vector3 endPosition   = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * zoomTimer / timeNormalToHurry; // recreate original position
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToFocus - recoilTimer) / timeToFocus);
            zoomTimer = Mathf.Max(zoomTimer - Time.deltaTime, 0);
            transform.position = Vector3.Lerp(startPosition, endPosition, (timeNormalToHurry - zoomTimer) / timeNormalToHurry);

            // Transition
            if (zoomTimer <= 0)
            {
                transform.position = endPosition; // should wait one frame more
                cameraState = STATE.HURRY;
            }
        }

        else if (cameraState == STATE.HURRY)
        {
            if(player.GetComponent<PlayerFirst>().IsProtected)
            {
                zoomTimer = timeHurryToProtected;
                cameraState = STATE.HURRY_TO_PROTECTED;
            }
            else if (!player.GetComponent<PlayerFirst>().IsHurry)
            {
                zoomTimer = timeHurryToNormal;
                cameraState = STATE.HURRY_TO_NORMAL;
            }
        }

        else if (cameraState == STATE.HURRY_TO_NORMAL)
        {
            Vector3 startPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * (timeHurryToNormal - zoomTimer) / timeHurryToNormal; // recreate original position
            Vector3 endPosition   = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetHurry + Vector3.up * Y_OffsetHurry) * zoomTimer / timeHurryToNormal; // recreate original position
            zoomTimer = Mathf.Max(zoomTimer - Time.deltaTime, 0);
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToNormal - recoilTimer) / timeToNormal);
            transform.position = Vector3.Lerp(startPosition, endPosition, (timeHurryToNormal - zoomTimer) / timeHurryToNormal);
            
            // Transition
            if (zoomTimer <= 0) {
                transform.position = endPosition; // should wait one frame more
                cameraState = STATE.NORMAL;
            }
        }

        else if (cameraState == STATE.NORMAL_TO_PROTECTED)
        {
            Vector3 startPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * (timeNormalToProtected - zoomTimer) / timeNormalToProtected; // recreate original position
            Vector3 endPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * zoomTimer / timeNormalToProtected; // recreate original position
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToFocus - recoilTimer) / timeToFocus);
            zoomTimer = Mathf.Max(zoomTimer - Time.deltaTime, 0);
            transform.position = Vector3.Lerp(startPosition, endPosition, (timeNormalToProtected - zoomTimer) / timeNormalToProtected);
            
            // Transition
            if (zoomTimer <= 0)
            {
                transform.position = endPosition;  // should wait one frame more
                cameraState = STATE.PROTECTED;
            }
        }

        else if (cameraState == STATE.PROTECTED)
        {
            if (!player.GetComponent<PlayerFirst>().IsProtected)
            {
                if (player.GetComponent<PlayerFirst>().IsHurry)
                {
                    zoomTimer = timeProtectedToHurry;
                    cameraState = STATE.PROTECTED_TO_HURRY;
                }
                else
                {
                    zoomTimer = timeProtectedToNormal;
                    cameraState = STATE.PROTECTED_TO_NORMAL;
                }
            }
        }

        else if (cameraState == STATE.PROTECTED_TO_NORMAL)
        {
            Vector3 startPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * (timeProtectedToNormal - zoomTimer) / timeProtectedToNormal; // recreate original position
            Vector3 endPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * Z_OffsetProtected + Vector3.up * Y_OffsetProtected) * zoomTimer / timeProtectedToNormal; // recreate original position
            zoomTimer = Mathf.Max(zoomTimer - Time.deltaTime, 0);
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToNormal - recoilTimer) / timeToNormal);
            transform.position = Vector3.Lerp(startPosition, endPosition, (timeProtectedToNormal - zoomTimer) / timeProtectedToNormal);

            // Transition
            if (zoomTimer <= 0)
            {
                transform.position = endPosition; // should wait one frame more
                cameraState = STATE.NORMAL;
            }
        }

        else if (cameraState == STATE.HURRY_TO_PROTECTED)
        {
            Vector3 startPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * (Z_OffsetHurry - Z_OffsetProtected) + Vector3.up * (Y_OffsetHurry - Y_OffsetProtected)) * (timeHurryToProtected - zoomTimer) / timeHurryToProtected; // recreate original position
            Vector3 endPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * (Z_OffsetHurry - Z_OffsetProtected) + Vector3.up * (Y_OffsetHurry - Y_OffsetProtected)) * zoomTimer / timeHurryToProtected; // recreate original position
            zoomTimer = Mathf.Max(zoomTimer - Time.deltaTime, 0);
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToNormal - recoilTimer) / timeToNormal);
            transform.position = Vector3.Lerp(startPosition, endPosition, (timeHurryToProtected - zoomTimer) / timeHurryToProtected);

            // Transition
            if (zoomTimer <= 0)
            {
                transform.position = endPosition; // should wait one frame more
                cameraState = STATE.PROTECTED;
            }
        }

        else if (cameraState == STATE.PROTECTED_TO_HURRY)
        {
            Vector3 startPosition = transform.position + (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * (Z_OffsetProtected - Z_OffsetHurry) + Vector3.up * (Y_OffsetProtected - Y_OffsetHurry)) * (timeProtectedToHurry - zoomTimer) / timeProtectedToHurry; // recreate original position
            Vector3 endPosition = transform.position - (-Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized * (Z_OffsetProtected - Z_OffsetHurry) + Vector3.up * (Y_OffsetProtected - Y_OffsetHurry)) * zoomTimer / timeProtectedToHurry; // recreate original position
            zoomTimer = Mathf.Max(zoomTimer - Time.deltaTime, 0);
            //float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (timeToNormal - recoilTimer) / timeToNormal);
            transform.position = Vector3.Lerp(startPosition, endPosition, (timeProtectedToHurry - zoomTimer) / timeProtectedToHurry);

            // Transition
            if (zoomTimer <= 0)
            {
                transform.position = endPosition; // should wait one frame more
                cameraState = STATE.HURRY;
            }
        }
    }

} //FINISH
