using UnityEngine;
using System.Collections;
using Pixelplacement;

public class SplineUser : MonoBehaviour
{
    [SerializeField] [Tooltip("The user of the spline")]
    private Transform target;

    [SerializeField] [Tooltip("The spline used by the object")]
    private Spline spline;

    [SerializeField] [Range(0, 1)] [Tooltip("Initial position on the spline (in percent)")]
    private float startPercentage = 0;
    private float percentage;
    private float smoothstep;

    [SerializeField] [Range(0.05f, 0.5f)] [Tooltip("The speed of the object")]
    private float speed = 0.1f;

    [SerializeField][Range(0f, 10f)][Tooltip("The duration of the pause")]
    private float stopDuration = 0;

    [SerializeField][Range(1f, 5f)][Tooltip("The duration of the pause")]
    private float freezeDuration = 3f;

    private enum DIRECTION { FORWARD, BACKWARD }
    [SerializeField]
    private DIRECTION directionState;
    private int directionalPercentage;
    private Quaternion directionalRotation;

    private enum STATE { NORMAL, FREEZE }
    private STATE movingState;

    private void Start()
    {
        movingState = STATE.NORMAL;
        if      (directionState == DIRECTION.FORWARD)  { directionalRotation = Quaternion.identity; }
        else if (directionState == DIRECTION.BACKWARD) { directionalRotation = Quaternion.Euler(0, 180, 0); }
        target.position = spline.GetPosition(percentage);
        StartCoroutine("Move");
    }

    void Update()
    {

    }

    private IEnumerator Move()
    {
        for(; ;)
        {
            if (movingState == STATE.NORMAL)
            {
                if (startPercentage != 1) // avoid dividing by zero
                {
                    percentage = Mathf.Min(percentage + speed * Time.deltaTime / (1f - startPercentage), 1f);
                }
                if (directionState == DIRECTION.FORWARD)       { smoothstep = Mathf.SmoothStep(startPercentage, 1, percentage); }
                else if (directionState == DIRECTION.BACKWARD) { smoothstep = Mathf.SmoothStep(startPercentage, 1, 1 - percentage); }

                CurveDetail cd = spline.GetCurve(smoothstep);
                Debug.Log(transform.name + "on curve " + cd.currentCurve);

                target.position = spline.GetPosition(smoothstep);
                target.rotation = Quaternion.LookRotation(directionalRotation * spline.GetDirection(smoothstep));
                if (percentage == 1)
                {
                    percentage = 0;
                    startPercentage = 0;
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

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            if (movingState != STATE.FREEZE)
            {
                movingState = STATE.FREEZE;
                startPercentage = smoothstep; // smooth restart
                percentage = 0;
                StartCoroutine("ResumeMove");
            }
        }
    }
}
