using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class SplineDebugger : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The first spline to debug")]
    private Spline spline1;

    [SerializeField]
    [Tooltip("Percentage on the first spline")]
    [Range(0,1)]
    private float percentage1 = 0f;

    [SerializeField]
    [Tooltip("The second spline to debug")]
    private Spline spline2;

    [SerializeField]
    [Tooltip("Percentage on the second spline")]
    [Range(0, 1)]
    private float percentage2 = 0f;

    private GameObject user1;
    private GameObject user2;

    // Start is called before the first frame update
    void Start()
    {
        user1 = transform.GetChild(0).gameObject;
        user2 = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnValidate()
    {
        user1.transform.position = spline1.GetPosition(percentage1, true);
        user1.transform.rotation = Quaternion.LookRotation(spline1.GetDirection(percentage1, true));

        user2.transform.position = spline2.GetPosition(percentage2, true);
        user2.transform.rotation = Quaternion.LookRotation(spline2.GetDirection(percentage2, true));
        
        Debug.Log("On spline " + spline1.name + " : " + percentage1);
        Debug.Log("On spline " + spline2.name + " : " + percentage2);
    }
}
