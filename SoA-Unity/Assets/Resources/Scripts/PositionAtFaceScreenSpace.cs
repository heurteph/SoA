using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionAtFaceScreenSpace : MonoBehaviour
{

    private float _camDistance;
    // Start is called before the first frame update
    void Start()
    {
        _camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (OpenCVFaceDetection.NormalizedFacePositions.Count == 0)
            return;

        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(OpenCVFaceDetection.positions.x,1-OpenCVFaceDetection.positions.y,_camDistance));//Camera.main.ViewportToWorldPoint(new Vector3(OpenCVFaceDetection.NormalizedFacePositions[0].x, OpenCVFaceDetection.NormalizedFacePositions[0].y, _camDistance));
        Camera.main.fieldOfView = 140 - OpenCVFaceDetection.taille;
        Debug.Log("Cam distance " + OpenCVFaceDetection.taille);
    }
}
