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

    bool colliding = false;
    float lastDistanceToCollider = Mathf.Infinity; // keep consistent with MaxDegDelta when it changes
    private float collidingAlignSpeed = 20;

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
        ProjectiveLookAround(inputs.Player.ProjectiveLook.ReadValue<float>());

        if (cameraSway) { Sway(); }
    }

    private void UpdatePosition ()
    {
        // if values have changed in the inspector
        UpdateFromInspector();

        // adapt recoil to normal, hurry or protected mode from the player
        UpdateRecoilPosition();

        transform.position += (player.transform.position - lastPlayerPosition);
        if (player.transform.position != lastPlayerPosition)
            Debug.Log("Camera moving in " + transform.position);
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
        float angle, angleCorrected, newAngle, newAngleCorrected, lastAngleCorrected;
        float thisFrame, previousFrame, thisSinerp, previousSinerp;
        Vector3 lastDirection;

        alignSpeed = 50; // TO DO : change in inspector
        float obstacleTolerance = 0.1f * Mathf.Deg2Rad; // TO DO : Add in the inspector

        for (; ; )
        {
            angle          = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, player.transform.forward, Vector3.up) % 360;
            angleCorrected = GetAngleToFirstObstacle(angle); // take obstacles into account

            Quaternion lastPlayerRotation = player.transform.rotation;
            Vector3 lastPlayerPos = player.transform.position;

            if (!Mathf.Approximately(angleCorrected, 0.0f)) // if not aligned with the character
            {

                Vector3 startDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                lastDirection = startDirection;
                thisFrame = 0;
                thisSinerp = 0;
                lastAngleCorrected = 0;

                while (thisFrame != 1.0f)
                {
                    UpdatePosition(); // called here to avoid desynchronization 

                    // Compute updated player angle
                    // SignedAngle's return value is in domain [-180;180], so extend it
                    // Save lastDirection to keep track of the last direction of the rotation

                    newAngle = Vector3.SignedAngle(startDirection, player.transform.forward, Vector3.up);
                    
                    if (Mathf.Sign(newAngle) != Mathf.Sign(angle))
                    {
                        if(Mathf.Sign(angle) > 0 && Vector3.SignedAngle(lastDirection, player.transform.forward, Vector3.up) > 0)
                        {
                            newAngle += 360;
                        }
                        else if (Mathf.Sign(angle) < 0 && Vector3.SignedAngle(lastDirection, player.transform.forward, Vector3.up) < 0)
                        {
                            newAngle -= 360;
                        }
                    }

                    lastDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

                    // Check if there is a collision along the rotation path

                    Debug.Log("t = " + thisFrame);
                    newAngleCorrected = GetAngleToFirstObstacle(newAngle);
                    //if (player.transform.position != lastPlayerPos || player.transform.rotation != lastPlayerRotation)  // avoid infinite oscillation
                    //{
                        //Debug.Log("My corrected angle is " + newAngleCorrected + ", forget about " + newAngle);
                        newAngle = newAngleCorrected;
                    //}
                    //lastPlayerRotation = player.transform.rotation;
                    //lastPlayerPos      = player.transform.position;
                    
                    //-----

                    if ( ! Mathf.Approximately(newAngle, 0)) // avoid dividing by zero
                        thisFrame = Mathf.Asin(Mathf.Clamp(thisSinerp * angle / newAngle, -1.0f, 1.0f)) * 2f / Mathf.PI; // get the original [0-1] from the smooth [0-1] updated with the angle modification, need clamping because Asin's domain is [-1,1]
                    else
                        thisFrame = 1;

                    previousFrame  = thisFrame;
                    angle          = newAngle;
                    thisFrame = Mathf.Min(thisFrame + collidingAlignSpeed * Time.deltaTime / Mathf.Abs(angle), 1.0f); // TO CHECK : Removing division by angle gives quite another gamefeel, but use with alignSpeed = 1
                    thisFrame = Mathf.Min(thisFrame + alignSpeed * Time.deltaTime / Mathf.Abs(angle), 1.0f); // TO CHECK : Removing division by angle gives quite another gamefeel, but use with alignSpeed = 1
                    previousSinerp = Mathf.Sin(previousFrame * Mathf.PI / 2f);
                    thisSinerp     = Mathf.Sin(thisFrame * Mathf.PI / 2f);
                    //Debug.Log("Rotation delta : " + angle * (thisSinerp - previousSinerp) + " from angle=" + angle + ", thisSinerp=" + thisSinerp + ", previousSinerp=" + previousSinerp + ", thisFrame=" + thisFrame + ", previousFrame=" + previousFrame);
                    transform.RotateAround(player.transform.position, Vector3.up, angle * (thisSinerp - previousSinerp));
                    
                    transform.rotation *= Quaternion.Euler(0, originalYRotation, 0);

                    yield return null;
                }
                //Debug.Log("Finish transition");
            }
            else
            {
                UpdatePosition(); // called here to avoir desynchronization

                yield return null;
            }
        }
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
                Debug.Log("Obstacle in the first linecast, camera at : " + startPosition + ", obstacle at " + hit.point);
                if (startAngle == 0) // if the first linecast hit an obstacle
                {
                    // TO DO : check if the angle to the obstacle is growing or not
                    if(!colliding)
                    {
                        colliding = true;
                        lastDistanceToCollider = (startPosition - hit.point).magnitude;
                        Debug.Log("Sticking to the obstacle");
                        return startAngle;
                    }
                    else if (colliding && (startPosition - hit.point).magnitude <= lastDistanceToCollider)
                    {
                        Debug.Log("Still sicking to the obstacle, distance stays the same " + (startPosition - hit.point).magnitude);
                        return startAngle;
                    }

                    Debug.Log("Moving away from the obstacle, the distance has grown due to player movement : " + (startPosition - hit.point).magnitude);
                    // else, it means the distance between the obstacle and camera is growing, so do not return as do as usual [...]
                }
                else // reset colliding
                {
                    Debug.Log("Quitting the obstacle");
                    colliding = false;
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

} //FINISH
