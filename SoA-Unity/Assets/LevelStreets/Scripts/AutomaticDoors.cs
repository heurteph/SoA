using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDoors : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The left automatic door")]
    private GameObject leftDoor;

    [SerializeField]
    [Tooltip("The right automatic door")]
    private GameObject rightDoor;

    [SerializeField]
    [Tooltip("The speed of the doors")]
    [Range(1,10)]
    private float speed = 3;

    [SerializeField]
    [Tooltip("The openness of the doors")]
    [Range(1, 10)]
    private float openness = 4;

    [SerializeField]
    [Tooltip("Minimal time for the doors to stay open")]
    [Range(1, 5)]
    private float minimalDuration = 3.5f;

    [Header("Sounds")]
    [Space]

    [SerializeField]
    [Tooltip("Event to be played when doors are opened")]
    private AK.Wwise.Event doorsOpenPlay;

    [SerializeField]
    [Tooltip("Event to stop the sound")]
    private AK.Wwise.Event doorsOpenStop;

    [SerializeField]
    [Tooltip("Sound to be played when doors are closed")]
    private AK.Wwise.Event doorsClosedPlay;

    [SerializeField]
    [Tooltip("Event to stop the sound")]
    private AK.Wwise.Event doorsClosedStop;

    [SerializeField]
    [Tooltip("Event to play supermarket annoucement")]
    private AK.Wwise.Event jinglePlay;

    [SerializeField]
    [Tooltip("Event to stop supermarket annoucement")]
    private AK.Wwise.Event jingleStop;

    [SerializeField]
    [Tooltip("Interval between two announcements")]
    [Range(5,10)]
    private float jingleInterval = 5;

    private float timer;
    private Vector3 leftDoorClosedPos, rightDoorClosedPos;
    private Vector3 leftDoorOpenPos, rightDoorOpenPos;
    private enum STATE { OPEN, CLOSED, INBETWEEN};
    private STATE state;
    // Start is called before the first frame update
    void Start()
    {
        if(leftDoor == null || rightDoor == null)
        {
            throw new System.NullReferenceException("No automatic doors for the building " + transform.name);
        }
        if(doorsClosedPlay == null || doorsOpenPlay == null || doorsClosedStop == null || doorsOpenStop == null)
        {
            throw new System.NullReferenceException("No doors sounds for the building " + transform.name);
        }
        leftDoorClosedPos = leftDoor.transform.position;
        rightDoorClosedPos = rightDoor.transform.position;
        leftDoorOpenPos = leftDoor.transform.position + leftDoor.transform.right * openness;
        rightDoorOpenPos = rightDoor.transform.position - rightDoor.transform.right * openness;
        state = STATE.CLOSED;
        timer = 0;

        if (jinglePlay != null)
        {
            //StartCoroutine("PlayJingle");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != STATE.OPEN)
        {
            state = STATE.INBETWEEN;
            StartCoroutine("OpenDoors");
        }
    }
    private IEnumerator OpenDoors()
    {
        doorsClosedStop.Post(gameObject);
        doorsOpenPlay.Post(gameObject);
        //jinglePlay?.Post(gameObject);
        StopCoroutine("CloseDoors");

        // TO DO : Play ambiance sound

        while (leftDoor.transform.position != leftDoorOpenPos)
        {
            leftDoor.transform.position = Vector3.MoveTowards(leftDoor.transform.position, leftDoorOpenPos, Time.deltaTime * speed);
            rightDoor.transform.position = Vector3.MoveTowards(rightDoor.transform.position, rightDoorOpenPos, Time.deltaTime * speed);
            yield return null;
        }
        Debug.Log("Doors opened !");
        state = STATE.OPEN;
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine("CloseDoors");
    }

    private IEnumerator CloseDoors()
    {
        /* must wait till it's fully open before starting the timer */
        while (state != STATE.OPEN)
        {
            yield return null;
        }

        yield return new WaitForSeconds(minimalDuration);
        
        state = STATE.INBETWEEN;

        doorsClosedPlay.Post(gameObject);
        
        while (leftDoor.transform.position != leftDoorClosedPos)
        {
            leftDoor.transform.position = Vector3.MoveTowards(leftDoor.transform.position, leftDoorClosedPos, Time.deltaTime * speed);
            rightDoor.transform.position = Vector3.MoveTowards(rightDoor.transform.position, rightDoorClosedPos, Time.deltaTime * speed);
            yield return null;
        }

        //jingleStop?.Post(gameObject);
        state = STATE.CLOSED;
    }

    private IEnumerator PlayJingle()
    {
        for(; ;)
        {
            jinglePlay.Post(gameObject);
            yield return new WaitForSeconds(jingleInterval);
        }
    }
}
