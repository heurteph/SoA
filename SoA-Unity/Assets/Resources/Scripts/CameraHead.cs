using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHead : MonoBehaviour
{
    Camera cam;
    //pour mouvement camera avec axe x 
    private Vector2 offSet;
    [SerializeField]
    private float factor = 1.0f;

    //private float _camDistance;
    // Start is called before the first frame update
    void Start()
    {
        offSet = new Vector2(0.0f,0.0f);
        cam = GetComponent<Camera>();
        //_camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (OpenCVFaceDetection.NormalizedFacePositions.Count == 0)
            return;

        Vector2 old_offset = new Vector2(offSet.x, offSet.y);

        Debug.Log(OpenCVFaceDetection.positions);
        //transform.position = Camera.main.ViewportToWorldPoint(new Vector3(OpenCVFaceDetection.positions.x, 1 - OpenCVFaceDetection.positions.y, _camDistance));
        
        offSet.x = (OpenCVFaceDetection.positions.x*2.0f)-1.0f;
        offSet.y = (OpenCVFaceDetection.positions.y * 2.0f) - 1.0f;


        Debug.Log("x est de "+offSet);
        cam.transform.position = new Vector3(cam.transform.position.x + ((offSet.x - old_offset.x)*factor), cam.transform.position.y + ((offSet.y - old_offset.y) * factor), cam.transform.position.z);
        /*transform.position = Camera.main.ViewportToWorldPoint(new Vector3(-OpenCVFaceDetection.NormalizedFacePositions[0].x, 
            -OpenCVFaceDetection.NormalizedFacePositions[0].y, _camDistance));*/
        //Debug.Log()
    }
}
