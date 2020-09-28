using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject compassMagnet;

    private GameObject mainCamera;

    private float degrees = 0;
    private Vector3 axis;
    private Quaternion orientation;

    // Start is called before the first frame update
    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        Debug.Assert(mainCamera != null, "Missing main camera");
        Debug.Assert(compassMagnet != null, "Missing compass magnet");

        axis = (Quaternion.Euler(45, 0, 0) * Vector3.up).normalized;
        orientation = Quaternion.FromToRotation(Vector3.forward, axis);
    }

    // Update is called once per frame
    void Update()
    {
        Redraw();
    }

    private void OnEnable()
    {
        Redraw();
    }

    public void Redraw()
    {
        degrees = Vector3.SignedAngle(Vector3.ProjectOnPlane(compassMagnet.transform.position - mainCamera.transform.position, Vector3.up), Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up), Vector3.up);
        transform.rotation = Quaternion.AngleAxis(degrees, axis) * orientation;
    }
}
