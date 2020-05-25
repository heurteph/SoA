using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pixelplacement;

public class SplineStreetUser : MonoBehaviour
{
    [SerializeField] [Tooltip("The spline used by the object")]
    private Spline spline;
    public Spline MySpline { get { return spline; } set { spline = value; } }

    [SerializeField]
    private bool onStart = true;

    [Header("Position")]
    [Space]

    [SerializeField] [Range(0, 1)] [Tooltip("Initial position on the spline (in percent)")]
    private float startPercentage = 0;
    private float percentage;
    public float Percentage { get { return percentage; } set { percentage = value; } }

    private float remainder = 0;

    [SerializeField]
    [Tooltip("The position of the stop")]
    [Range(0, 1)]
    private float stopPercentage;
    private bool hasStopped;
    public bool HasStopped { get { return hasStopped; } }

    private float speed = 50f;
    public float Speed { get { return speed; } set { speed = value; } }

    private float frontSpeed;

    public float FrontSpeed { get { return frontSpeed; } set { frontSpeed = value; } }

    private float obstaclePercentage;
    public float ObstaclePercentage { get { return obstaclePercentage; } set { obstaclePercentage = value; } }

    private float intersectionPercentage;
    public float IntersectionPercentage { get { return intersectionPercentage; } set { intersectionPercentage = value; } }


    [Header("Speed")]
    [Space]

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("The speed of the object on a FAST section")]
    private float fastSpeed = 70f;

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("The speed of the object on a NORMAL section")]
    private float normalSpeed = 60f;

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("The speed of the object on a SLOW section")]
    private float slowSpeed = 20f;

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("The speed of the object on a CAUTIOUS section")]
    private float cautiousSpeed = 10f;

    [Space]

    [SerializeField]
    [Range(1f, 1000f)]
    [Tooltip("The acceleration of the object between two speeds")]
    private float acceleration = 10f;

    [SerializeField]
    [Range(1f, 1000f)]
    [Tooltip("The deceleration of the object between two speeds")]
    private float deceleration = 10f;

    [SerializeField]
    [Range(1f, 1000f)]
    [Tooltip("The deceleration of the object when behind another object")]
    private float decelerationObstacle = 50f;

    [SerializeField]
    [Range(1f, 1000f)]
    [Tooltip("The deceleration of the object to full stop")]
    private float decelerationStop = 50f;

    private Dictionary<SPEEDLIMIT, float> speedValues;

    [Header("Durations")]
    [Space]

    [SerializeField][Range(0f, 10f)][Tooltip("The duration of the pause")]
    private float stopDuration = 0;

    [SerializeField][Range(1f, 5f)][Tooltip("The duration of the pause")]
    private float freezeDuration = 3f;

    [Header("Ground")]
    [Space]

    [SerializeField]
    [Tooltip("The raycaster")]
    private Transform raycaster;

    [SerializeField]
    [Tooltip("The ground level")]
    private Transform groundLevel;
    private float groundOffset;

    private enum DIRECTION { FORWARD, BACKWARD }
    [SerializeField]
    private DIRECTION directionState;

    public enum STATE { NORMAL, FREEZE, STAYBEHIND, STOPPING, STOP }
    private STATE movingState;
    public STATE MovingState { get { return movingState; } set { movingState = value; } }

    private void Start()
    {
        // Register to the spline
        spline.GetComponent<SplineStreetMap>().RegisterUser(gameObject);

        // Set the speed according to the zones
        speedValues = new Dictionary<SPEEDLIMIT, float>
        {
            { SPEEDLIMIT.FAST, fastSpeed },
            { SPEEDLIMIT.NORMAL, normalSpeed },
            { SPEEDLIMIT.SLOW, slowSpeed },
            { SPEEDLIMIT.CAUTIOUS, cautiousSpeed }
        };
        spline.CalculateLength();

        percentage = startPercentage;
        movingState = STATE.NORMAL;
        groundOffset = transform.position.y - groundLevel.transform.position.y;

        // Set the initial position
        Vector3 position = spline.GetPosition(percentage);
        transform.position = StickToTheGround(position);

        // Set the initial rotation
        if (directionState == DIRECTION.FORWARD)
        {
            transform.rotation = Quaternion.LookRotation(spline.GetDirection(Mathf.Clamp(startPercentage, 0.01f, 0.99f), true)); // initial rotation
        }
        else if (directionState == DIRECTION.BACKWARD)
        {
            transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0, 180f, 0) * spline.GetDirection(Mathf.Clamp(startPercentage, 0.01f, 0.99f), true)); // initial rotation
        }

        // Set the initial speed
        SPEEDLIMIT lastZoneSpeedLimit = SPEEDLIMIT.NORMAL;
        float lastZonePercentage = 0;
        
        // for exemple : [0, 0.5, 1]
        foreach (KeyValuePair<float, SPEEDLIMIT> nextSpeedZone in spline.GetComponent<SplineStreetMap>().SpeedZones)
        {
            if (percentage >= lastZonePercentage && percentage < nextSpeedZone.Key)
            {
                speed = speedValues[lastZoneSpeedLimit];
                break;
            }
            lastZonePercentage = nextSpeedZone.Key;
            lastZoneSpeedLimit = nextSpeedZone.Value;
        }

        hasStopped = false;
        if (onStart) { StartCoroutine("Move"); }
    }

    public void Trigger()
    {
        if (!onStart) { StartCoroutine("Move"); }
    }

    void Update()
    {

    }

    private IEnumerator Move()
    {
        for(; ;)
        {
            if (movingState != STATE.FREEZE)
            {
                if (movingState == STATE.NORMAL)
                {
                    /* FIND THE SPEED LIMIT OF THE ZONE */

                    SPEEDLIMIT speedLimit = SPEEDLIMIT.NORMAL;

                    SPEEDLIMIT lastZoneSpeedLimit = SPEEDLIMIT.NORMAL;
                    float lastZonePercentage = 0;

                    foreach (KeyValuePair<float, SPEEDLIMIT> nextSpeedZone in spline.GetComponent<SplineStreetMap>().SpeedZones)
                    {
                        if(percentage >= lastZonePercentage && percentage < nextSpeedZone.Key)
                        {
                            speedLimit = lastZoneSpeedLimit;//speedZone.Value;
                            //Debug.Log("Speed Zone : [" + lastZonePercentage + "," + nextSpeedZone.Key + "]");
                            break;
                        }
                        lastZonePercentage = nextSpeedZone.Key;
                        lastZoneSpeedLimit = nextSpeedZone.Value;
                    }

                    /* UPDATE SPEED */

                    if (speed < speedValues[speedLimit])
                    {
                        //speed = Mathf.Min(speed + acceleration * Time.deltaTime / (speedValues[speedLimit] - speed), speedValues[speedLimit]);
                        speed = Mathf.Min(speed + acceleration * Time.deltaTime, speedValues[speedLimit]);
                        //speed = speedValues[speedLimit];
                    }
                    else if (speed > speedValues[speedLimit])
                    {
                        //speed = Mathf.Max(speed - deceleration * Time.deltaTime / (speed - speedValues[speedLimit]), speedValues[speedLimit]);
                        speed = Mathf.Max(speed - deceleration * Time.deltaTime, speedValues[speedLimit]);
                        //speed = speedValues[speedLimit];
                    }
                }
                else if (movingState == STATE.STAYBEHIND)
                {
                    /* UPDATE SPEED ACCORDING TO THE FRONT VEHICLE'S SPEED AND POSITION */

                    // TO DO : Use obstaclePercentage to keep moving till it's not reached

                    Debug.Log(transform.name + "IN STAYBEHIND MODE");

                    if (speed >= frontSpeed)
                    {
                        //speed = frontSpeed;
                        speed = Mathf.Max(speed - (Mathf.Max(speed, decelerationObstacle) + 10 * (obstaclePercentage - percentage)) * Time.deltaTime, frontSpeed); // deceleration proportionnal to speed
                        
                        // move towards the target
                        /*
                        if(speed == frontSpeed && obstaclePercentage > percentage)
                        {
                            speed += (obstaclePercentage - percentage) * Time.deltaTime;
                        }*/
                    }
                }
                else if (movingState == STATE.STOP)
                {
                    if (speed >= 0)
                    {
                        //speed = 0;
                        speed = Mathf.Max(speed - (Mathf.Max(speed, decelerationStop) - 10 * (intersectionPercentage - percentage)) * Time.deltaTime, 0); // deceleration proportionnal to speed

                        // move towards the target
                        /*
                        if (speed == 0 && intersectionPercentage > percentage)
                        {
                            speed = (intersectionPercentage - percentage) * Time.deltaTime;
                        }*/
                    }
                }

                /* UPDATE PERCENTAGE */

                if (directionState == DIRECTION.FORWARD)
                {
                    percentage = percentage + speed * Time.deltaTime / spline.Length;
                    if(percentage >= 1)
                    {
                        remainder = percentage - 1; // keep the remainder for the next loop
                        percentage = 1;
                    }
                }
                else if (directionState == DIRECTION.BACKWARD)
                {
                    percentage = percentage - speed * Time.deltaTime / spline.Length;
                    if(percentage <= 0)
                    {
                        remainder = -percentage; // keep the remainder for the next loop
                        percentage = 0;
                    }
                }

                /* UPDATE TRANSFORM */
                
                Vector3 position = spline.GetPosition(percentage);
                transform.position = StickToTheGround(position);
                if (directionState == DIRECTION.FORWARD)
                {
                    transform.rotation = Quaternion.LookRotation(spline.GetDirection(percentage));
                }
                else if (directionState == DIRECTION.BACKWARD)
                {
                    transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0, 180f, 0) * spline.GetDirection(percentage));
                }

                Debug.Log("PERCENTAGE : " + percentage + "!!!!!!!!!!!!!!!!!!!!!!!!!!");

                /* STOP CONDITION */

                if (directionState == DIRECTION.FORWARD)
                {
                    if (percentage >= stopPercentage && !hasStopped)
                    {
                        hasStopped = true;
                        if (stopDuration > 0)
                        {
                            yield return new WaitForSeconds(stopDuration);
                        }
                    }
                }
                else if (directionState == DIRECTION.BACKWARD)
                {
                    if (percentage <= stopPercentage && !hasStopped)
                    {
                        hasStopped = true;
                        if (stopDuration > 0)
                        {
                            yield return new WaitForSeconds(stopDuration);
                        }
                    }
                }

                /* LOOP CONDITION */

                if (directionState == DIRECTION.FORWARD && percentage == 1)
                {
                    percentage = remainder;
                    hasStopped = false;
                    if (stopDuration > 0)
                    {
                        yield return new WaitForSeconds(stopDuration);
                    }
                }
                else if (directionState == DIRECTION.BACKWARD && percentage == 0)
                {
                    percentage = 1 - remainder;
                    hasStopped = false;
                    if (stopDuration > 0)
                    {
                        yield return new WaitForSeconds(stopDuration);
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator ResumeMove()
    {
        yield return new WaitForSeconds(freezeDuration);
        movingState = STATE.NORMAL;
    }

    private Vector3 StickToTheGround(Vector3 position)
    {
        LayerMask mask = LayerMask.GetMask("AsphaltGround") | LayerMask.GetMask("GrassGround") | LayerMask.GetMask("SoilGround");
        if(Physics.Raycast(raycaster.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, mask))
        {
            return new Vector3(position.x, hit.point.y + groundOffset, position.z);
        }
        return position;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.CompareTag("Player") || other.transform.CompareTag("Vehicle"))
        {
            if (movingState != STATE.FREEZE)
            {
                movingState = STATE.FREEZE;
                StartCoroutine("ResumeMove");
            }
        }
    }
}
