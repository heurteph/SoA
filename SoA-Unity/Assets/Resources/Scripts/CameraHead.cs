using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHead : MonoBehaviour
{
    Camera cam;
    Matrix4x4 cam_base;
    //pour mouvement camera avec axe x 
    private Vector2 offSet;
    float offsetZ;
    [SerializeField]
    private float factor = 1.0f;

    //private float _camDistance;
    // Start is called before the first frame update
    void Start()
    {
        offSet = new Vector2(0.0f,0.0f);
        offsetZ = 1.0f;
        cam = GetComponent<Camera>();
        cam_base = cam.projectionMatrix;
        //_camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal") * 10.0f * Time.deltaTime;
        //float rot = Input.GetAxis("Horizontal") * rot_speed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * 10.0f * Time.deltaTime;
        transform.Translate(new Vector3(x, 0, z));

        if (OpenCVFaceDetection.NormalizedFacePositions.Count == 0)
            return;

        Vector2 old_offset = new Vector2(offSet.x, offSet.y);
        float old_offset_z = offsetZ;

        Debug.Log(OpenCVFaceDetection.positions);
        //transform.position = Camera.main.ViewportToWorldPoint(new Vector3(OpenCVFaceDetection.positions.x, 1 - OpenCVFaceDetection.positions.y, _camDistance));


        float radius_median = 85.0f;

        offSet.x = (OpenCVFaceDetection.positions.x*2.0f)-1.0f;
        offSet.y = (OpenCVFaceDetection.positions.y * 2.0f) - 1.0f;
        //offsetZ = OpenCVFaceDetection.positions.z/radius_median;

        Debug.Log("Radius est de " + OpenCVFaceDetection.positions.z);

        Debug.Log("Offsets dot "+ Mathf.Acos(offSet.x * old_offset.x + offSet.y * old_offset.y));

        float marge_erreur = 0.025f;
        float marge_erreur_radius = 5.0f;
        Debug.Log("Marge erreur "+ Mathf.Abs(offSet.x - old_offset.x)+" et "+ Mathf.Abs(offSet.y - old_offset.y));
        
        //un second filtre pour calibrage du cadrage

        //ici détermine si marge d'erreur pour éviter tremblements
        //en angle ? => pour dot product evaluation
        //if(Mathf.Abs(Mathf.Acos(offSet.x * old_offset.x + offSet.y * old_offset.y)) >= (Mathf.Deg2Rad * 90.0f))
        if(Mathf.Abs(offSet.x - old_offset.x) > marge_erreur || Mathf.Abs(offSet.y - old_offset.y) > marge_erreur )//|| Mathf.Abs(offsetZ - old_offset_z) > marge_erreur_radius )
        {
            Matrix4x4 shear = Matrix4x4.identity;
            //shear.SetRow(1,new Vector4(0,1,2,0));
            //shear.SetRow(0, new Vector4(2, 0, 0, 0));
            //shear.SetRow(0, new Vector4(0, 2, 0, 0));
            Mathf.Clamp(offsetZ,0.995f,1.005f);
            //shear.SetRow(2, new Vector4(offsetZ*offSet.x, offsetZ * offSet.y, 1, 0));
            //shear.SetRow(2, new Vector4(offSet.x, offSet.y, 2 - offsetZ, 0));
            //shear.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, -1.0f));
            //ici multiplication de la troisieme colonne sur Mat de projection

            //shear.SetRow(0,new Vector4(1.0f,offSet.y,0.0f,0.0f));
            //shear.SetRow(1, new Vector4(offSet.x, 1.0f, 0.0f, 0.0f));
            shear.SetRow(2, new Vector4(offSet.x, offSet.y, 1.0f, 0.0f));
            


            Debug.Log("point est de est de " + offSet);
            //cam.transform.position = new Vector3(cam.transform.position.x + ((offSet.x - old_offset.x)*factor), cam.transform.position.y + ((offSet.y - old_offset.y) * factor), cam.transform.position.z);
            cam.projectionMatrix = cam_base * shear;
            /*transform.position = Camera.main.ViewportToWorldPoint(new Vector3(-OpenCVFaceDetection.NormalizedFacePositions[0].x, 
            -OpenCVFaceDetection.NormalizedFacePositions[0].y, _camDistance));*/
            //Debug.Log()
        }
        else
        {
            offSet = old_offset;
            offsetZ = old_offset_z;
        }
    }
}
