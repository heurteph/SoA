﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pixelplacement;

public class SplineUser : MonoBehaviour
{
    [SerializeField] [Tooltip("The user of the spline")]
    private Transform target;

    [SerializeField]
    [Tooltip("The raycaster")]
    private Transform raycaster;

    [SerializeField]
    [Tooltip("The ground level")]
    private Transform groundLevel;
    private float groundOffset;

    [SerializeField] [Tooltip("The spline used by the object")]
    private Spline spline;

    [SerializeField] [Range(0, 1)] [Tooltip("Initial position on the spline (in percent)")]
    private float startPercentage = 0;
    private float percentage;
    private float smoothstep;

    private float speed = 0.1f;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    [Tooltip("The speed of the object on a LONG")]
    private float longSpeed = 0.1f;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    [Tooltip("The speed of the object on a STRAIGHT")]
    private float straightSpeed = 0.1f;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    [Tooltip("The speed of the object on a TURN")]
    private float turnSpeed = 0.1f;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    [Tooltip("The speed of the object on a SHARP")]
    private float sharpSpeed = 0.1f;

    private Dictionary<ROADSECTION, float> speedZones;

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
        speedZones = new Dictionary<ROADSECTION, float>();
        speedZones.Add(ROADSECTION.LONG, longSpeed);
        speedZones.Add(ROADSECTION.STRAIGHT, straightSpeed);
        speedZones.Add(ROADSECTION.TURN, turnSpeed);
        speedZones.Add(ROADSECTION.SHARP, sharpSpeed);

        groundOffset = target.transform.position.y - groundLevel.transform.position.y;
        movingState = STATE.NORMAL;
        if      (directionState == DIRECTION.FORWARD)  { directionalRotation = Quaternion.identity; }
        else if (directionState == DIRECTION.BACKWARD) { directionalRotation = Quaternion.Euler(0, 180, 0); }
        Vector3 position = spline.GetPosition(percentage);
        target.position = StickToTheGround(position);
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
                // Approach the current section speed
                CurveDetail curve = spline.GetCurve(smoothstep);
                //Debug.Log(transform.name + "on curve " + curve.currentCurve); // TO DO : Stop before a turn
                ROADSECTION roadSection = spline.GetComponent<SplineRoadmap>().RoadSections[curve.currentCurve];
                if(speed < speedZones[roadSection])
                {
                    speed = Mathf.Min(speed + 0.01f * Time.deltaTime, speedZones[roadSection]);
                }
                else if (speed > speedZones[roadSection])
                {
                    speed = Mathf.Max(speed - 0.1f * Time.deltaTime, speedZones[roadSection]);
                }

                if (startPercentage != 1) // avoid dividing by zero
                {
                    percentage = Mathf.Min(percentage + speed * Time.deltaTime / (1f - startPercentage), 1f);
                }
                if (directionState == DIRECTION.FORWARD)       { smoothstep = Mathf.SmoothStep(startPercentage, 1, percentage); }
                else if (directionState == DIRECTION.BACKWARD) { smoothstep = Mathf.SmoothStep(startPercentage, 1, 1 - percentage); }

                Vector3 position = spline.GetPosition(smoothstep);
                target.rotation = Quaternion.LookRotation(directionalRotation * spline.GetDirection(smoothstep));
                target.position = StickToTheGround(position);

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

    private Vector3 StickToTheGround(Vector3 position)
    {
        LayerMask mask = LayerMask.GetMask("Ground");
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
                startPercentage = smoothstep; // smooth restart
                percentage = 0;
                StartCoroutine("ResumeMove");
            }
        }
    }
}
