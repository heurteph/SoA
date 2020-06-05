using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHead : MonoBehaviour
{
    Camera cam;
    Matrix4x4 cam_base;

    Matrix4x4 T, M, P;
    //pour mouvement camera avec axe x 
    Vector3 va, vb, vc;
    Vector3 vr, vu, vn;
    Vector3 eye_pos;
    Vector3 pa, pb, pc;
    //right left bottom top distance
    float r, l, b, t, d;

    private Vector2 offSet;
    float offsetZ;
    [SerializeField]
    private float factor = 1.0f;
    [SerializeField]
    float marge_erreur = 0.025f;
    [SerializeField]
    private OpenCVFaceDetection opencv;
    float a00, a01, a02;
    float a10, a11, a12;
    float a20, a21, a22;

    float clamp(float min,float max, float value)
    {
        if(value < min)
        {
            value = min;
        }else if(value > max)
        {
            value = max;
        }
        return value;
    }

    float smoothstep(float min, float max, float value)
    {
        float v = (value - min) / (max - min);
        v = clamp(0.0f, 1.0f, v);
        v = v * v * (3 - 2 * v);
        return v;
    }

    float roundDecimal(float value,int pow)
    {
        if(pow <= 0)
        {
            return value;
        }
        else
        {
            //v1
            //int v = (int)(value * Mathf.Pow(10.0f, pow-1));
            //return (v/ Mathf.Pow(10.0f, pow-1));
            //v2 avec >= 5 1 sinon 0

            float v = (int)(value * Mathf.Pow(10.0f, pow));
            int unit = ((int)(v/10.0f))*10;
            float nb = v - unit;
            /*if (nb % 2 != 0)
            {
                v = (v + 1);
            }
            v /= Mathf.Pow(10.0f,pow);*/
            if(nb >= 5)
            {
                v = (unit + 1) / Mathf.Pow(10.0f, pow);
            }
            else
            {
                v = unit / Mathf.Pow(10.0f, pow);
            }
            return v;
        }
    }
    //private float _camDistance;
    // Start is called before the first frame update
    void Start()
    {
        a00 = a01 = a02 = a10 = a11 = a12 = a20 = a21 = a22 = 0.0f;
        offSet = new Vector2(0.0f,0.0f);
        offsetZ = 1.0f;
        cam = GetComponent<Camera>();
        cam_base = cam.projectionMatrix;


        //v3D
        P = new Matrix4x4();
        M = new Matrix4x4();
        T = new Matrix4x4();


        //gérer les coords avec aspect et fov

        //float y = cam.nearClipPlane * Mathf.Tan(cam.fieldOfView/2.0f);
        //float x = y * cam.aspect;
        float x = 1.0f;
        float y = x / cam.aspect;

        //point en bas a gauche
        pa = new Vector3(-x, -y, -cam.nearClipPlane);//new Vector3(-1.5f, -0.75f, -18);
        //point a droite
        pb = new Vector3(x, -y, -cam.nearClipPlane);
        //point en haut a gauche
        pc = new Vector3(-x, y, -cam.nearClipPlane);


        //vecteur droite
        vr = (pb - pa).normalized;
        //vecteur up
        vu = (pc - pa).normalized;
        //vecteur normal
        vn = Vector3.Cross(vr, vu).normalized;
        //eye_pos = new Vector3(offSet.x, offSet.y, 0.0f);
        eye_pos = transform.position;
        /*
         //vecteurs par rapport au head tracking
         va = pa - eye_pos;
         vb = pb - eye_pos;
         vc = pc - eye_pos;

         //calcul de la distance entre le point et la position eye, comme differente direction opposé
         d = -Vector3.Dot(vn,va);

         //calcul du left right top bottom
         l = Vector3.Dot(vr, va) * cam.nearClipPlane / d;
         r = Vector3.Dot(vr, vb) * cam.nearClipPlane / d;
         b = Vector3.Dot(vu, va) * cam.nearClipPlane / d;
         t = Vector3.Dot(vu, vc) * cam.nearClipPlane / d;

         P = new Matrix4x4();
         P.SetRow(0, new Vector4((2*cam.nearClipPlane)/(r-l),0.0f, (r + l) / (r - l), 0.0f));
         P.SetRow(1, new Vector4(0.0f, (2 * cam.nearClipPlane) / (t - b),(t+b)/(t-b),0.0f));
         P.SetRow(2, new Vector4(0.0f,0.0f,-(cam.farClipPlane+cam.nearClipPlane)/(cam.farClipPlane-cam.nearClipPlane),-(2*cam.farClipPlane*cam.nearClipPlane)/(cam.farClipPlane-cam.nearClipPlane)));
         P.SetRow(3, new Vector4(0.0f,0.0f,-1.0f,0.0f));*/

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
        float marge_erreur_radius = 5.0f;

        if (opencv.getActivate())
        {
            offSet.x = (OpenCVFaceDetection.positions.x * 2.0f) - 1.0f;
            offSet.y = (OpenCVFaceDetection.positions.y * 2.0f) - 1.0f;

            //Debug.Log("Radius est de " + OpenCVFaceDetection.positions.z);
            //Debug.Log("Les Offsets sont de " + offSet.x + " et " + offSet.y);
            //Debug.Log("Les Offsets Old sont de " + old_offset.x + " et " + old_offset.y);
            //Debug.Log("Offsets dot " + Mathf.Acos(offSet.x * old_offset.x + offSet.y * old_offset.y));
            //Debug.Log("Marge erreur " + Mathf.Abs(offSet.x - old_offset.x) + " et " + Mathf.Abs(offSet.y - old_offset.y)+" Radius "+OpenCVFaceDetection.positions.z);
            offSet.x = -offSet.x;
            if (Mathf.Abs(offSet.x - old_offset.x) > marge_erreur || Mathf.Abs(offSet.y - old_offset.y) > marge_erreur)
            {
                Matrix4x4 shear = Matrix4x4.identity;
                Mathf.Clamp(offsetZ, 0.995f, 1.005f);

                //fov doit etre régler sur vertical pour FOV_AXIS
                float near, far, fov_vertical, fov_horizontal, right, left, top, bottom;

                float coef_deplacement = 1.0f;


                near = cam.nearClipPlane;
                far = cam.farClipPlane;
                fov_vertical = cam.fieldOfView;

                top = near * Mathf.Tan((fov_vertical / 2.0f) * Mathf.Deg2Rad);
                bottom = -top;
                right = top * cam.aspect;
                left = -right;


                float r2, l2, t2, b2;
                r2 = right * (1.0f - offSet.x);
                l2 = left * (1.0f + offSet.x);

                t2 = top * (1.0f - offSet.y);
                b2 = bottom * (1.0f + offSet.y);


                Matrix4x4 camera = new Matrix4x4();

                /*methode 2 sur projection + shear Translation*/
                shear.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, -offSet.x * 4.0f));
                shear.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, -offSet.y * 4.0f));
                shear.SetRow(2, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));

                camera.SetRow(0, new Vector4((2 * near) / (r2 - l2), 0.0f, (r2 + l2) / (r2 - l2), 0.0f));
                camera.SetRow(1, new Vector4(0.0f, (2 * near) / (top - bottom), (t2 + b2) / (t2 - b2), 0.0f));
                camera.SetRow(2, new Vector4(0.0f, 0.0f, -(far + near) / (far - near), -(2 * near * far) / (far - near)));
                camera.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));

                cam.projectionMatrix = camera * shear;
            }
            else
            {
                offSet = old_offset;
                offsetZ = old_offset_z;
            }
        }
        else
        {
            cam.projectionMatrix = cam_base;
        }

        
        //cam.worldToCameraMatrix = P * M * T;
        
        //version 3D Kooima
        //cam.projectionMatrix = P * M * T;
    }
}
