using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class TrafficManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The street splines to be watched")]
    private Spline[] streetSplines;

    [SerializeField]
    [Range(10,100)]
    [Tooltip("The distance below which the object must tailor its speed to the predecessor")]
    private float minWatchDistance = 20f;

    [SerializeField]
    [Range(0, 5f)]
    [Tooltip("How much the speed of the object impact its safety distance")]
    private float minSafetySpeedFactor = 2f/3f;

    [SerializeField]
    [Range(1, 100)]
    [Tooltip("The distance below which the object must stop")]
    private float minSafetyDistance = 2f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckForward();
        CheckCrossroad();
    }

    void CheckForward()
    {
        foreach (Spline spline in streetSplines)
        {
            foreach (GameObject user in spline.GetComponent<SplineStreetMap>().Users)
            {
                // Set default state
                user.GetComponent<SplineStreetUser>().MovingState = SplineStreetUser.STATE.NORMAL;

                foreach (GameObject otherUser in spline.GetComponent<SplineStreetMap>().Users)
                {
                    if ( ! GameObject.ReferenceEquals(user, otherUser))
                    {
                        Debug.Log("Comparing " + user.name + " at " + user.GetComponent<SplineStreetUser>().Percentage + " and " + otherUser.name + " at " + otherUser.GetComponent<SplineStreetUser>().Percentage + " !!!!!!!!!!!!!");
                        float distance = otherUser.GetComponent<SplineStreetUser>().Percentage - user.GetComponent<SplineStreetUser>().Percentage;

                        // TO DO : ADD Forward/Backward Distinction
                        // minSafetyDistance proportional to speed of the object
                        float minWatchPercentage = Mathf.Max(user.GetComponent<SplineStreetUser>().Speed * minSafetySpeedFactor, minWatchDistance) / spline.Length;

                        // Join the end and the start of the spline
                        if ((0 <= distance && distance <= minWatchPercentage) || -1 <= distance &&  distance <= -1 + minWatchPercentage)
                        {
                            Debug.Log(otherUser.name + " IS IN FRONT OF " + user.name + " !!!!");
                            user.GetComponent<SplineStreetUser>().FrontSpeed = otherUser.GetComponent<SplineStreetUser>().Speed;
                            user.GetComponent<SplineStreetUser>().MovingState = SplineStreetUser.STATE.STAYBEHIND;
                            float minSafetyPercentage = minSafetyDistance / spline.Length;
                            user.GetComponent<SplineStreetUser>().ObstaclePercentage = Mathf.Repeat(otherUser.GetComponent<SplineStreetUser>().Percentage - minSafetyPercentage + 1, 1);
                            // TO DO : When there are two vehicules in front ! Choose the closer one !!!
                        }
                    }
                }
                foreach (Colinearity colinearity in spline.GetComponent<SplineStreetMap>().Colinearities)
                {
                    if(user.GetComponent<SplineStreetUser>().Percentage >= colinearity.percentageStart && user.GetComponent<SplineStreetUser>().Percentage <= colinearity.percentageEnd)
                    {
                        foreach (GameObject otherUser in colinearity.otherSpline.GetComponent<SplineStreetMap>().Users)
                        {
                            // Règle de trois
                            float otherUserMappedPercentage = Remap(otherUser.GetComponent<SplineStreetUser>().Percentage, colinearity.otherPercentageStart, colinearity.otherPercentageEnd, colinearity.percentageStart, colinearity.percentageEnd);

                            float distance = otherUserMappedPercentage - user.GetComponent<SplineStreetUser>().Percentage;

                            // TO DO : ADD Forward/Backward Distinction
                            float minSafetyPercentage = minWatchDistance / spline.Length;
                            if (distance > 0 && distance < minSafetyPercentage)
                            {
                                user.GetComponent<SplineStreetUser>().MovingState = SplineStreetUser.STATE.STAYBEHIND;
                                user.GetComponent<SplineStreetUser>().FrontSpeed = otherUser.GetComponent<SplineStreetUser>().Speed;

                                // TO DO : When there are two vehicules in front ! Choose the closer !!!
                            }
                        }
                    }
                }
            }
        }
    }

    public static float Remap(float value, float start1, float end1, float start2, float end2)
    {
        return start2 + ((value - start1) / (end1 - start1)) + (end2 - start2);
    }

    void CheckCrossroad()
    {
        foreach(Spline spline in streetSplines)
        {
            foreach (GameObject user in spline.GetComponent<SplineStreetMap>().Users)
            {
                foreach (Intersection intersection in spline.GetComponent<SplineStreetMap>().Intersections)
                {
                    float distanceToIntersection = intersection.percentage - user.GetComponent<SplineStreetUser>().Percentage;
                    float minWatchPercentage = Mathf.Max(user.GetComponent<SplineStreetUser>().Speed * minSafetySpeedFactor, minWatchDistance) / spline.Length;

                    if ((0 <= distanceToIntersection && distanceToIntersection <= minWatchPercentage) || (-1 <= distanceToIntersection && distanceToIntersection <= -1 + minWatchPercentage))
                    {
                        foreach (GameObject otherUser in intersection.otherSpline.GetComponent<SplineStreetMap>().Users)
                        {
                            Debug.Log("Comparing " + user.name + " on " + spline.name + " at " + user.GetComponent<SplineStreetUser>().Percentage + " with " + otherUser.name + " on " + intersection.otherSpline.name + " at " + otherUser.GetComponent<SplineStreetUser>().Percentage + " !!!!!!!!!!!!!");

                            float otherDistanceToIntersection = intersection.otherPercentage - otherUser.GetComponent<SplineStreetUser>().Percentage;
                            float otherMinWatchPercentage = Mathf.Max(otherUser.GetComponent<SplineStreetUser>().Speed * minSafetySpeedFactor, minWatchDistance) / spline.Length;

                            // Join the end and the start of the spline
                            if ((0 <= otherDistanceToIntersection && otherDistanceToIntersection <= otherMinWatchPercentage) || (-1 <= otherDistanceToIntersection && otherDistanceToIntersection <= -1 + otherMinWatchPercentage))
                            {
                                user.GetComponent<SplineStreetUser>().MovingState = SplineStreetUser.STATE.STOP;
                                float minSafetyPercentage = minSafetyDistance / spline.Length;
                                user.GetComponent<SplineStreetUser>().IntersectionPercentage = Mathf.Repeat(intersection.percentage - minSafetyPercentage + 1, 1);
                            }
                        }
                    }
                }
            }
        }
    }
}
