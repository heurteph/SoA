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

        // compare which one is closer !

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
                        //Debug.Log("Comparing " + user.name + " at " + user.GetComponent<SplineStreetUser>().Percentage + " and " + otherUser.name + " at " + otherUser.GetComponent<SplineStreetUser>().Percentage + " !!!!!!!!!!!!!");
                        float distance = otherUser.GetComponent<SplineStreetUser>().Percentage - user.GetComponent<SplineStreetUser>().Percentage;

                        // TO DO : ADD Forward/Backward Distinction
                        // minSafetyDistance proportional to speed of the object
                        float minWatchPercentage = Mathf.Max(user.GetComponent<SplineStreetUser>().Speed * minSafetySpeedFactor, minWatchDistance) / spline.Length;

                        // Join the end and the start of the spline
                        if ((0 <= distance && distance <= minWatchPercentage) || (-1 <= distance && distance <= -1 + minWatchPercentage))
                        {
                            //Debug.Log(otherUser.name + " IS IN FRONT OF " + user.name + " !!!!");
                            user.GetComponent<SplineStreetUser>().FrontSpeed = otherUser.GetComponent<SplineStreetUser>().Speed;
                            user.GetComponent<SplineStreetUser>().MovingState = SplineStreetUser.STATE.STAYBEHIND;
                            float minSafetyPercentage = minSafetyDistance / spline.Length;
                            user.GetComponent<SplineStreetUser>().ObstaclePercentage = Mathf.Repeat(otherUser.GetComponent<SplineStreetUser>().Percentage - minSafetyPercentage + 1, 1);  // +1 because Repeat does not take negative numbers into argument
                            // TO DO : When there are two vehicules in front ! Choose the closer one !!!
                        }
                    }
                }

                foreach (Colinearity colinearity in spline.GetComponent<SplineStreetMap>().Colinearities)
                {
                    if(IsInRange(user.GetComponent<SplineStreetUser>().Percentage, colinearity.percentageStart, colinearity.percentageEnd))
                    {
                        foreach (GameObject otherUser in colinearity.otherSpline.GetComponent<SplineStreetMap>().Users)
                        {
                            if (IsInRange(otherUser.GetComponent<SplineStreetUser>().Percentage, colinearity.otherPercentageStart, colinearity.otherPercentageEnd))
                            {
                                // Règle de trois
                                float otherUserMappedPercentage = Remap(otherUser.GetComponent<SplineStreetUser>().Percentage, colinearity.otherPercentageStart, colinearity.otherPercentageEnd, colinearity.percentageStart, colinearity.percentageEnd);

                                Debug.Log("COLINEARITY OF TWO VEHICLES : " + user.name + " at " + user.GetComponent<SplineStreetUser>().Percentage + " and the other " + otherUser.name + " at " + otherUserMappedPercentage);

                                float distance = otherUserMappedPercentage - user.GetComponent<SplineStreetUser>().Percentage;

                                // TO DO : ADD Forward/Backward Distinction
                                float minWatchPercentage = Mathf.Max(user.GetComponent<SplineStreetUser>().Speed * minSafetySpeedFactor, minWatchDistance) / spline.Length;
                                if ((0 <= distance && distance < minWatchPercentage) || (-1 <= distance && distance <= -1 + minWatchPercentage))
                                {
                                    Debug.Log(otherUser.name + " IS IN FRONT OF " + user.name + " BY COLINEARITY !!!!");
                                    user.GetComponent<SplineStreetUser>().MovingState = SplineStreetUser.STATE.STAYBEHIND;
                                    user.GetComponent<SplineStreetUser>().FrontSpeed = otherUser.GetComponent<SplineStreetUser>().Speed; // TO DO : Remap too ?
                                    float minSafetyPercentage = minSafetyDistance / spline.Length;
                                    user.GetComponent<SplineStreetUser>().ObstaclePercentage = Mathf.Repeat(otherUserMappedPercentage - minSafetyPercentage + 1, 1); // +1 because Repeat does not take negative numbers into argument
                                    Debug.Log("COLINEARITY TRIGGER WITH OBSTACLE PERCENTAGE AT " + user.GetComponent<SplineStreetUser>().ObstaclePercentage);
                                    // TO DO : When there are two vehicules in front ! Choose the closer !!!
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static bool IsInRange(float value, float start, float end)
    {
        if (!(0 <= value && value <= 1 && 0 <= start && start <= 1 && 0 <= end && end <= 1))
        {
            throw new System.ArgumentOutOfRangeException("Values are not in a valid range [0,1]");
        }
        if(start < end)
        {
            return start <= value && value <= end;
        }
        if(end < start)
        {
            return start <= value || value <= end;
        }
        else // end == start
        {
            return value == end && start == value;
        }
    }

    public static float Remap(float value, float start1, float end1, float start2, float end2)
    {
        if (start1 < end1 && start2 < end2)
        {
            return start2 + ((value - start1) / (end1 - start1)) * (end2 - start2);
        }

        else if (end1 < start1 && start2 < end2)
        {
            float x = 0;
            if(start1 <= value)
            {
                x = (value - start1) / (1 - start1 + end1);
            }
            else if(value <= end1)
            {
                x = (1 - start1 + value) / (1 - start1 + end1);
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Wrong value for remapping");
            }
            return start2 + x * (end2 - start2);
        }

        else if (start1 < end1 && end2 < start2)
        {
            return Mathf.Repeat(start2 + ((value - start1) / (end1 - start1)) * (1 - start2 + end2), 1);
        }

        else if (end1 < start1 && end2 < start2)
        {
            float x = 0;
            if (value >= start1)
            {
                x = (value - start1) / (1 - start1 + end1);
            }
            else if (value <= end1)
            {
                x = (1 - start1 + value) / (1 - start1 + end1);
            }
            return Mathf.Repeat(start2 + x * (1 - start2 + end2), 1);
        }
        return value;
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
                            //Debug.Log("Comparing " + user.name + " on " + spline.name + " at " + user.GetComponent<SplineStreetUser>().Percentage + " with " + otherUser.name + " on " + intersection.otherSpline.name + " at " + otherUser.GetComponent<SplineStreetUser>().Percentage + " !!!!!!!!!!!!!");

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
