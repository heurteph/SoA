﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

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

    private float forwardAccumulator = 0;
    private float forwardDuration = 2; //s
    private float forwardDistance = 3;

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
    private float timeHurryToNormal = 1.5f;

    [SerializeField]
    [Tooltip("The delay to switch from protected to normal view, in seconds")]
    [Range(0.1f, 5)]
    private float timeProtectedToNormal = 0.7f;

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
    private float timeNormalToHurry = 0.7f;

    [SerializeField]
    [Tooltip("The delay to switch from protected to hurry view, in seconds")]
    [Range(0.1f, 5)]
    private float timeProtectedToHurry = 0.4f;

    [Space]
    [Header("Protected Mode")]

    [SerializeField]
    [Tooltip("Z-Offset when player is in protected mode (-closer, +farther)")]
    [Range(-10, 10)]
    private float Z_OffsetProtected = -10f;

    [SerializeField]
    [Tooltip("Y-Offset when player is in hurry mode (-closer, +farther)")]
    [Range(-10, 10)]
    private float Y_OffsetProtected = 4;

    [SerializeField]
    [Tooltip("The delay to switch from normal to protected view, in seconds")]
    [Range(0.1f, 5)]
    private float timeNormalToProtected = 0.7f;

    [SerializeField]
    [Tooltip("The delay to switch from hurry to protected view, in seconds")]
    [Range(0.1f, 5)]
    private float timeHurryToProtected = 0.4f;

    private float zoomTimer;

    private float angleFromHurryToHorizon = 0;
    private float angleFromProtectedToHorizon = 0;

    [Space]
    [Header("Sway")]

    [SerializeField]
    [Tooltip("The sway pivot")]
    private GameObject cameraSway;

    [SerializeField]
    [Tooltip("The minimal sway radius")]
    [Range(0.001f, 1f)]
    private float swayRadiusMin = 0.01f;

    [SerializeField]
    [Tooltip("The maximal sway radius")]
    [Range(0.001f, 1f)]
    private float swayRadiusMax = 0.05f;

    private float latitude = 0, longitude = 0, swayRadius = 0;

    [SerializeField]
    [Tooltip("The sway duration")]
    [Range(2f, 10f)]
    private float swayDurationMin = 4f;

    [SerializeField]
    [Tooltip("The sway duration")]
    [Range(2f, 10f)]
    private float swayDurationMax = 6f;
    private float swayDuration = 0;
    private float swayTimer = 0;

    // Collisions

    bool isColliding = false; // TO DO : Set private
    float lastDistanceToCollider = Mathf.Infinity; // keep consistent with MaxDegDelta when it changes
    private float collidingAlignSpeed = 20;

    // Targeting

    private bool isTargeting = false;
    private Vector3 startForward = Vector3.zero;
    private Vector3 endForward = Vector3.zero;
    private Vector3 storedForward = Vector3.zero;
    private Quaternion storedRotation = Quaternion.identity;
    private float targetingTimer = 0;
    private Quaternion initialParentRotation = Quaternion.identity;
    private Quaternion currentParentRotation = Quaternion.identity;

    [Space]
    [Header("Targeting")]


    [SerializeField]
    private GameObject defaultForward;

    [SerializeField]
    [Range(0.5f,5f)]
    [Tooltip("The total duration of the targeting (pause included) in seconds")]
    private float targetingDuration = 5;

    [SerializeField]
    [Tooltip("The pause on the target in seconds")]
    [Range(0.5f, 2f)]
    private float targetingPause = 1;

    private Vector3 targetPosition = Vector3.zero;

    private bool isAvailable;
    public bool IsAvailable { get { return isAvailable; } set { isAvailable = value; } }

    public bool isPausingAlign;
    public bool IsPausingAlign { get { return isPausingAlign; } set { isPausingAlign = value; } }

    private void Awake()
    {
        inputs = InputsManager.Instance.Inputs;
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
        isAvailable = true;
        isPausingAlign = false;

        if (cameraSway)
        {
            InitializeSway();
            StartCoroutine("Sway");
        }

        //StartCoroutine("AlignWithCharacter");
        StartCoroutine("AlignWithCharacter2");
    }
    void UpdateFromInspector()
    {
        if (cameraOffset != storedCameraOffset)
        {
            transform.position += (cameraOffset - storedCameraOffset);
            storedCameraOffset = cameraOffset;
        }
    }

    public void ResetCameraToFrontView()
    {
        lastPlayerPosition = player.transform.position; // disable following during the warp
        transform.position = player.transform.position + Quaternion.LookRotation(-player.transform.forward, Vector3.up) * cameraOffset;

        zoomTimer = 0; // check if correct
        cameraState = STATE.NORMAL;

        // do not align until first player move
        isPausingAlign = true;
    }

    private void OnEnable()
    {
        // might trigger inputs back during warp to shelter ???
        // inputs.Player.Enable();
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

        if (!isTargeting)
        {
            LookAround(inputs.Player.LookAround.ReadValue<Vector2>());
            //ProjectiveLookAround(inputs.Player.ProjectiveLook.ReadValue<float>());
            //ExtendedLookAround(inputs.Player.LookAround.ReadValue<Vector2>());
        }

        if (cameraSway)
        {
            Sway();
        }
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

    private void UpdatePosition()
    {
        // if values have changed in the inspector
        UpdateFromInspector();

        // adapt recoil to normal, hurry or protected mode from the player
        UpdateRecoilPosition();

        if (player.transform.position != lastPlayerPosition)
        {
            //Debug.Log("Camera moving in " + transform.position);
            transform.position += (player.transform.position - lastPlayerPosition);
            lastPlayerPosition = player.transform.position;
        }
    }

    private bool CheckPlayerMovement()
    {
        return (player.transform.position != lastPlayerPosition);
    }

    private IEnumerator AlignWithCharacter()
    {
        for(; ;)
        {
            UpdatePosition(); // called here to avoid desynchronization

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

    private IEnumerator AlignWithCharacter2()
    {
        if (!isTargeting) // CHECK IF CORRECT ?? SHOULDN'T IT BE INSIDE THE FOR LOOP ?
        {

            float angle, newAngle;
            float thisPercent;
            float thisSinerp, previousSinerp;

            alignSpeed = 50; // TO DO : change in inspector

            for (; ; )
            {
                while(isTargeting) // Let's try it here, CHECK IF WORKING
                {
                    yield return null;
                }

                while (isPausingAlign)
                {
                    Debug.Log("Align en pause");
                    yield return null;
                    if(CheckPlayerMovement())
                    {
                        Debug.Log("Sorti de pause");
                        isPausingAlign = false;
                    }
                }

                angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, player.transform.forward, Vector3.up) % 360;
                angle = GetAngleToFirstObstacle(angle);

                // if camera not aligned with the closest position to be aligned with the character

                if (!Mathf.Approximately(angle, 0.0f))
                {
                    Debug.Log("Start of an interpolation, angle : " + angle);

                    Vector3 startForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                    thisPercent = 0;
                    thisSinerp = 0;

                    float debugSum = 0;

                    // Launch an interpolation

                    while (thisPercent != 1.0f)
                    {
                        UpdatePosition(); // called here to avoid desynchronization 

                        // Compute the angle from the current camera position to the align with the character

                        newAngle = Vector3.SignedAngle(startForward, player.transform.forward, Vector3.up);

                        // SignedAngle's return value is in domain [-180;180], so extend it

                        if (Mathf.Sign(newAngle) != Mathf.Sign(angle))
                        {
                            Debug.Log("Angle : " + angle + " and NewAngle " + newAngle);
                            
                            if((angle - newAngle) > 180)
                            {
                                newAngle += 360;
                                Debug.Log("Warp newAngle : " + newAngle);
                            }
                            else if ((newAngle - angle) > 180)
                            {
                                newAngle -= 360;
                                Debug.Log("Warp newAngle : " + newAngle);
                            }
                        }

                        // Next, check if there is a collision along the new rotation path

                        newAngle = GetAngleToFirstObstacle(newAngle);

                        if (!Mathf.Approximately(newAngle, angle))
                        {
                            // Get the original [0-1] from the smooth [0-1] updated with the angle modification, need clamping because Asin's domain is [-1,1]

                            if (!Mathf.Approximately(newAngle, 0)) // avoid dividing by zero
                            {
                                Debug.Log("Remapping the percentage");
                                thisPercent = InverseSinerp(thisSinerp * angle / newAngle);
                            }
                            else
                            {
                                thisPercent = 1;
                            }
                        }
                        
                        // Increment differently when it's colliding

                        if (!isColliding)
                        {
                            thisPercent = Mathf.Min(thisPercent + alignSpeed * Time.deltaTime / Mathf.Abs(newAngle), 1.0f); // TO CHECK : Removing division by angle gives quite another gamefeel, but use with alignSpeed = 1
                        }
                        else
                        {
                            Debug.Log("Is colliding");
                            thisPercent = Mathf.Min(thisPercent + collidingAlignSpeed * Time.deltaTime / Mathf.Abs(newAngle), 1.0f); // TO CHECK : Removing division by angle gives quite another gamefeel, but use with alignSpeed = 1
                        }

                        previousSinerp  = thisSinerp;
                        thisSinerp      = Sinerp(thisPercent);
                        //Debug.Log("Rotation delta : " + angle * (thisSinerp - previousSinerp) + " from angle=" + angle + ", thisSinerp=" + thisSinerp + ", previousSinerp=" + previousSinerp + ", thisFrame=" + thisFrame + ", previousFrame=" + previousFrame);
                        debugSum += newAngle * (thisSinerp - previousSinerp);

                        transform.RotateAround(player.transform.position, Vector3.up, newAngle * (thisSinerp - previousSinerp)); // <-- ERROR, THE SUM OF THE FRACTIONS DOESNT RESULT IN newAngle AT THE END
                        transform.rotation *= Quaternion.Euler(0, originalYRotation, 0);

                        Debug.Log("Angle interpolation : " + (newAngle * thisSinerp) + "/" + newAngle + ", t = " + thisSinerp);

                        angle = newAngle;
                        yield return null;
                    }
                    Debug.Log("End of the interpolation, angle : " + angle + "\n---------------------------");
                    Debug.Log("True angle done : " + debugSum);
                }
                else
                {
                    UpdatePosition(); // called here to avoir desynchronization

                    yield return null;
                }
            }
        }
    }

    float Sinerp(float x)
    {
        if(!(0 <= x && x <= 1))
        {
            throw new System.Exception("Sinerp argument must be in range [0-1] : " + x);
        }
        return Mathf.Sin(x * Mathf.PI / 2f);
    }

    float InverseSinerp(float x)
    {
        return Mathf.Asin(Mathf.Clamp(x, -1.0f, 1.0f)) * 2f / Mathf.PI; // get the original [0-1] from the smooth [0-1] updated with the angle modification, need clamping because Asin's domain is [-1,1]
    }

    private float GetAngleToFirstObstacle(float targetAngle)
    {
        float startAngle  = 0;  // in degrees
        float endAngle    = 0;  // in degrees
        float maxDegDelta = 10; // in degrees
        Vector3 startDirection = transform.position - player.transform.position,
                endDirection   = transform.position - player.transform.position,
                startPosition  = transform.position,
                endPosition    = transform.position;
        RaycastHit hit;
        
        do {
            startAngle     = endAngle;
            startDirection = endDirection;
            startPosition  = player.transform.position + startDirection; // height ???? it's a the ground level not at the camera's height !!!

            endAngle       = (1 - Mathf.Sign(targetAngle)) / 2f * Mathf.Max(startAngle - maxDegDelta, targetAngle) + (1 + Mathf.Sign(targetAngle)) / 2f * Mathf.Min(startAngle + maxDegDelta, targetAngle); // Sign ? DONE
            endDirection   = Quaternion.Euler(0, endAngle - startAngle, 0) * startDirection;
            endPosition    = player.transform.position + endDirection;

            if (Physics.Linecast(startPosition, endPosition, out hit))
            {
                /* Debug.Log("Obstacle in the first linecast, camera at : " + startPosition + ", obstacle at " + hit.point); */
                if (startAngle == 0) // if the first linecast hit an obstacle
                {
                    // TO DO : check if the angle to the obstacle is growing or not
                    if(!isColliding)
                    {
                        isColliding = true;
                        lastDistanceToCollider = (startPosition - hit.point).magnitude;
                        /* Debug.Log("Sticking to the obstacle"); */
                        return startAngle;
                    }
                    else if (isColliding && (startPosition - hit.point).magnitude <= lastDistanceToCollider)
                    {
                        /* Debug.Log("Still sicking to the obstacle, distance stays the same " + (startPosition - hit.point).magnitude); */
                        return startAngle;
                    }

                    /* Debug.Log("Moving away from the obstacle, the distance has grown due to player movement : " + (startPosition - hit.point).magnitude); */
                    // else, it means the distance between the obstacle and camera is growing, so do not return as do as usual [...]
                }
                else // reset colliding
                {
                    /* Debug.Log("Quitting the obstacle"); */
                    isColliding = false;
                }
                //Debug.Log("Obstacle " + hit.transform.name + " at : " + hit.point + " while I'm at " + transform.position);
                Vector3 newPosition = Vector3.Lerp(transform.position, hit.point, 0.9f); // keep your distance from the collider
                Vector3 newDirection = player.transform.position - newPosition;
                //Debug.DrawLine(startPosition + Vector3.up, newPosition + Vector3.up, Color.red, 1000);

                return Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, newDirection, Vector3.up);
            }

            //Debug.DrawLine(startPosition, endPosition, Color.blue, 2f, false);

        } while (endAngle < targetAngle);

        return targetAngle;
    }

    void LookAround(Vector2 v)
    {
        float smoothx = 0;
        float smoothy = 0;

        //Debug.Log("Accumulator : " + accumulator);

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
        //heldCamera.transform.localRotation = Quaternion.Euler(-smoothy * maxLookAroundAngle, smoothx * maxLookAroundAngle, 0);
        heldCamera.transform.localRotation  = Quaternion.Euler(y_stabilization, 0, 0);
        heldCamera.transform.localRotation *= Quaternion.Euler(0, smoothx * maxHorizontalAngle, 0);
        heldCamera.transform.localRotation *= Quaternion.Euler(-smoothy * maxVerticalAngle, 0, 0);
    }

    void ProjectiveLookAround(float v)
    {
        float sinerpForward = 0;
        Debug.Log("V = " + v);
        if(v != 0)
        {
            forwardAccumulator = Mathf.Clamp (forwardAccumulator + Time.deltaTime / forwardDuration, 0, 1);
            sinerpForward = Mathf.Sin(forwardAccumulator * Mathf.PI * 0.5f);
            Debug.Log("sinerp : " + sinerpForward);
        }
        else
        {
            forwardAccumulator = Mathf.Clamp(forwardAccumulator - Time.deltaTime / forwardDuration, 0, 1);
            sinerpForward = Mathf.Sin(forwardAccumulator * Mathf.PI * 0.5f);
            Debug.Log("sinerp : " + sinerpForward);
        }
        //heldCamera.transform.position = (1-v)*transform.position + v* (player.transform.position) + (player.transform.forward * forwardDistance) * sinerpForward;
        heldCamera.transform.position = transform.position + (player.transform.position + player.transform.forward * forwardDistance - transform.position) * sinerpForward;
    }

    void ExtendedLookAround(Vector2 v)
    {
        maxHorizontalAngle = 90; // needed
        float minHorizontalAngle = 45;

        float rotationSmooth = 0;
        float lateralSmooth  = 0;
        float forwardSmooth  = 0;

        float rightProjection = 5;
        float forwardProjection = 12;

        if (!Mathf.Approximately(v.x, 0))
        {
            accumulator.x = Mathf.Clamp(accumulator.x + v.x * Time.deltaTime / horizontalDuration, -1, 1);
            rotationSmooth = Mathf.Sign(accumulator.x) * Mathf.Sin(Mathf.Abs(accumulator.x) * Mathf.PI * 0.5f);
            //lateralSmooth = Mathf.Sign(accumulator.x) * (1 - Mathf.Sqrt(1 - Mathf.Pow(accumulator.x,2)));
            lateralSmooth = Mathf.Sign(accumulator.x) * Mathf.Sin(Mathf.Abs(accumulator.x) * Mathf.PI * 0.5f);
        }
        else
        {
            accumulator.x = (1 - Mathf.Sign(accumulator.x)) / 2f * Mathf.Min(accumulator.x - Mathf.Sign(accumulator.x) * Time.deltaTime / horizontalDuration, 0) + (1 + Mathf.Sign(accumulator.x)) / 2f * Mathf.Max(accumulator.x - Mathf.Sign(accumulator.x) * Time.deltaTime / horizontalDuration, 0);
            rotationSmooth = Mathf.Sign(accumulator.x) * Mathf.Sin(Mathf.Abs(accumulator.x) * Mathf.PI * 0.5f);
            //lateralSmooth = Mathf.Sign(accumulator.x) * (1 - Mathf.Sqrt(1 - Mathf.Pow(accumulator.x,2)));
            lateralSmooth = Mathf.Sign(accumulator.x) * Mathf.Sin(Mathf.Abs(accumulator.x) * Mathf.PI * 0.5f);
        }

        if (!Mathf.Approximately(v.y, 0))
        {
            //inputs.Player.Walk.Disable();
            accumulator.y = Mathf.Clamp(accumulator.y + v.y * Time.deltaTime / verticalDuration, -1, 1);
            forwardSmooth = Mathf.Sign(accumulator.y) * Mathf.Sin(Mathf.Abs(accumulator.y) * Mathf.PI * 0.5f);
        }
        else
        {
            //inputs.Player.Walk.Enable();
            accumulator.y = (1 - Mathf.Sign(accumulator.y)) / 2f * Mathf.Min(accumulator.y - Mathf.Sign(accumulator.y) * Time.deltaTime / verticalDuration, 0) + (1 + Mathf.Sign(accumulator.y)) / 2f * Mathf.Max(accumulator.y - Mathf.Sign(accumulator.y) * Time.deltaTime / verticalDuration, 0);
            forwardSmooth = Mathf.Sign(accumulator.y) * Mathf.Sin(Mathf.Abs(accumulator.y) * Mathf.PI * 0.5f);
        }

        // Stabilization of the look around
        float y_stabilization = 0;
        if (cameraState == STATE.HURRY) { y_stabilization = Mathf.Abs(rotationSmooth) * -angleFromHurryToHorizon; }
        else if (cameraState == STATE.NORMAL_TO_HURRY) { y_stabilization = Mathf.Abs(rotationSmooth) * -angleFromHurryToHorizon * (timeNormalToHurry - zoomTimer) / timeNormalToHurry; }
        else if (cameraState == STATE.HURRY_TO_NORMAL) { y_stabilization = Mathf.Abs(rotationSmooth) * -angleFromHurryToHorizon * zoomTimer / timeHurryToNormal; }

        else if (cameraState == STATE.PROTECTED) { y_stabilization = Mathf.Abs(rotationSmooth) * -angleFromProtectedToHorizon; }
        else if (cameraState == STATE.NORMAL_TO_PROTECTED) { y_stabilization = Mathf.Abs(rotationSmooth) * -angleFromProtectedToHorizon * (timeNormalToProtected - zoomTimer) / timeNormalToProtected; }
        else if (cameraState == STATE.PROTECTED_TO_NORMAL) { y_stabilization = Mathf.Abs(rotationSmooth) * -angleFromProtectedToHorizon * zoomTimer / timeProtectedToNormal; }

        else if (cameraState == STATE.HURRY_TO_PROTECTED) { y_stabilization = Mathf.Abs(rotationSmooth) * (-angleFromHurryToHorizon * zoomTimer / timeHurryToProtected - angleFromProtectedToHorizon * (timeHurryToProtected - zoomTimer) / timeHurryToProtected); }
        else if (cameraState == STATE.PROTECTED_TO_HURRY) { y_stabilization = Mathf.Abs(rotationSmooth) * (-angleFromProtectedToHorizon * zoomTimer / timeProtectedToHurry - angleFromHurryToHorizon * (timeProtectedToHurry - zoomTimer) / timeProtectedToHurry); }

        // Must be separated in two because unity's order for euler is ZYX and we want X-Y-X
        //heldCamera.transform.localRotation = Quaternion.Euler(-smoothy * maxLookAroundAngle, smoothx * maxLookAroundAngle, 0);
        heldCamera.transform.localRotation = Quaternion.Euler(y_stabilization, 0, 0);
        heldCamera.transform.localRotation *= Quaternion.Euler(0, rotationSmooth * (minHorizontalAngle + forwardSmooth * (maxHorizontalAngle - minHorizontalAngle)), 0);
        //heldCamera.transform.localRotation *= Quaternion.Euler(-smoothy * maxVerticalAngle, 0, 0);

        heldCamera.transform.position = transform.position + transform.right * rightProjection * lateralSmooth + (player.transform.position + player.transform.forward * forwardProjection - transform.position) * forwardSmooth;
    }

    void UpdateRecoilPosition()
    {
        if (cameraState == STATE.NORMAL)
        {
            if (!isTargeting) //isAvailable ?
            {
                if (player.GetComponent<PlayerFirst>().IsProtectingEyes || player.GetComponent<PlayerFirst>().IsProtectingEars)
                {
                    zoomTimer = timeNormalToProtected;
                    isAvailable = false;
                    cameraState = STATE.NORMAL_TO_PROTECTED;
                }

                else if (player.GetComponent<PlayerFirst>().IsHurry)
                {
                    zoomTimer = timeNormalToHurry;
                    isAvailable = false;
                    cameraState = STATE.NORMAL_TO_HURRY;
                }
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
                isAvailable = true;
                cameraState = STATE.HURRY;
            }
        }

        else if (cameraState == STATE.HURRY)
        {
            if (!isTargeting) // isAvailable ?
            {
                if (player.GetComponent<PlayerFirst>().IsProtectingEyes || player.GetComponent<PlayerFirst>().IsProtectingEars)
                {
                    zoomTimer = timeHurryToProtected;
                    isAvailable = false;
                    cameraState = STATE.HURRY_TO_PROTECTED;
                }
                else if (!player.GetComponent<PlayerFirst>().IsHurry)
                {
                    zoomTimer = timeHurryToNormal;
                    isAvailable = false;
                    cameraState = STATE.HURRY_TO_NORMAL;
                }
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
            if (zoomTimer <= 0)
            {
                transform.position = endPosition; // should wait one frame more
                isAvailable = true;
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
                isAvailable = true;
                cameraState = STATE.PROTECTED;
            }
        }

        else if (cameraState == STATE.PROTECTED)
        {
            if (!isTargeting) // isAvailable ?
            {
                if (!player.GetComponent<PlayerFirst>().IsProtectingEyes && !player.GetComponent<PlayerFirst>().IsProtectingEars)
                {
                    if (player.GetComponent<PlayerFirst>().IsHurry)
                    {
                        zoomTimer = timeProtectedToHurry;
                        isAvailable = false;
                        cameraState = STATE.PROTECTED_TO_HURRY;
                    }
                    else
                    {
                        zoomTimer = timeProtectedToNormal;
                        isAvailable = false;
                        cameraState = STATE.PROTECTED_TO_NORMAL;
                    }
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
                isAvailable = true;
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
                isAvailable = true;
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
                isAvailable = true;
                cameraState = STATE.HURRY;
            }
        }
    }

    void InitializeSway()
    {
        latitude      = Random.Range(0, 180);
        longitude     = Random.Range(0, 360);
        swayRadius    = Random.Range(swayRadiusMin, swayRadiusMax);
        swayDuration  = Random.Range(swayDurationMin, swayDurationMax);
        swayTimer     = swayDuration;
    }
    IEnumerator Sway()
    {
        for (; ;)
        {
            Vector3 target = transform.position + new Vector3(
                swayRadius * Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Cos(longitude * Mathf.Deg2Rad),
                swayRadius * Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Sin(longitude * Mathf.Deg2Rad),
                swayRadius * Mathf.Cos(latitude * Mathf.Deg2Rad)
            );
            
            swayTimer -= Time.deltaTime;
            float smoothstep = Mathf.SmoothStep(0.0f, 1.0f, (swayDuration - swayTimer) / swayDuration);
            cameraSway.transform.position = Vector3.Lerp(cameraSway.transform.position, target, smoothstep);

            if (Mathf.Approximately((cameraSway.transform.position - target).magnitude, 0))
            {
                latitude     = Random.Range(0, 90) + 90 * (1 - Mathf.Sign(latitude - 90) / 2f);
                longitude    = Random.Range(longitude - 90, longitude + 90) % 360;
                swayRadius   = Random.Range(swayRadiusMin, swayRadiusMax);
                swayDuration = Random.Range(swayDurationMin, swayDurationMax);
                swayTimer    = swayDuration;
            }
            yield return null;
        }
    }

    /* Targeting */

    public void TargetingObstacle(GameObject target)
    {
        if (!isTargeting && isAvailable && !player.GetComponent<PlayerFirst>().IsProtectingEyes && !player.GetComponent<PlayerFirst>().IsProtectingEars)
        {
            //isAvailable = false;
            inputs.Player.ProtectEars.Disable();
            inputs.Player.ProtectEyes.Disable();
            isTargeting = true;
            targetingTimer = targetingDuration;

            targetPosition = target.transform.position;

            storedForward = heldCamera.transform.forward;
            storedRotation = heldCamera.transform.localRotation;
            initialParentRotation = this.transform.rotation;

            StartCoroutine("SlerpTo");
        }
    }

    IEnumerator SlerpTo()
    {
        startForward = storedForward;
        endForward = Vector3.ProjectOnPlane((targetPosition - heldCamera.transform.position).normalized, Vector3.up);

        while (targetingTimer > (targetingDuration - targetingPause) * 0.5f + targetingPause)
        {
            currentParentRotation = this.transform.rotation;
            //startForward = Quaternion.Inverse(currentParentRotation) * storedForward;
            startForward = Vector3.ProjectOnPlane(defaultForward.transform.forward, Vector3.up);
            endForward = Vector3.ProjectOnPlane((targetPosition - heldCamera.transform.position).normalized, Vector3.up); // Check if ok
            targetingTimer = Mathf.Max(targetingTimer - Time.deltaTime, (targetingDuration - targetingPause) * 0.5f + targetingPause);
            Vector3 current = Vector3.Slerp(startForward, endForward, ((targetingDuration - targetingPause) * 0.5f - (targetingTimer - ((targetingDuration - targetingPause) * 0.5f + targetingPause))) / ((targetingDuration - targetingPause) * 0.5f));
            //current = new Vector3(current.x, current.y, 0);
            /* Quaternion rotation = Quaternion.LookRotation(transform.up, -current)
                                * Quaternion.AngleAxis(90f, Vector3.right);
            heldCamera.transform.rotation = rotation; */
            //Quaternion look = Quaternion.LookRotation((new Vector3(current.x,current.y,0)).normalized, Vector3.up);
            heldCamera.transform.localRotation = Quaternion.Inverse(heldCamera.transform.parent.rotation) * Quaternion.LookRotation(current, Vector3.up);
            //heldCamera.transform.rotation = Quaternion.LookRotation(current, Vector3.up);
            //float angle = Vector3.Angle(Vector3.ProjectOnPlane(startForward, Vector3.up), Vector3.ProjectOnPlane(endForward, Vector3.up));
            //heldCamera.transform.rotation = Quaternion.Inverse(heldCamera.transform.parent.rotation) * Quaternion.Euler(0, -angle, 0);

            //heldCamera.transform.rotation = Quaternion.LookRotation(current);

            Debug.Log("startForward: " + startForward + ", endForward: " + endForward + ", currentForward: " + current);
            yield return null;
        }
        while (targetingTimer > (targetingDuration - targetingPause) * 0.5f)
        {
            targetingTimer = Mathf.Max(targetingTimer - Time.deltaTime, (targetingDuration - targetingPause) * 0.5f);
            heldCamera.transform.LookAt(targetPosition);
        }
        StartCoroutine("SlerpFrom");
    }

    IEnumerator SlerpFrom()
    {
        startForward = heldCamera.transform.forward;
        endForward = storedForward;

        while (targetingTimer > 0)
        {
            targetingTimer = Mathf.Max(targetingTimer - Time.deltaTime, 0);
            Vector3 current = Vector3.Slerp(startForward, endForward, ((targetingDuration - targetingPause) * 0.5f - targetingTimer) / ((targetingDuration - targetingPause) * 0.5f));
            heldCamera.transform.localRotation = Quaternion.Inverse(heldCamera.transform.parent.rotation) * Quaternion.LookRotation(current);
            yield return null;
        }
        isTargeting = false;
        inputs.Player.ProtectEars.Enable();
        inputs.Player.ProtectEyes.Enable();

        /* Let's try this here */
        /*
        if (!player.GetComponent<PlayerFirst>().IsDamaged)
        {
            StartCoroutine("Timer");
        }
        player.GetComponent<PlayerFirst>().IsDamaged = true;*/
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(3);
        player.GetComponent<PlayerFirst>().IsDamaged = false;
    }

} //FINISH
