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
    float a00, a01, a02;
    float a10, a11, a12;
    float a20, a21, a22;

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

         offSet.x = (OpenCVFaceDetection.positions.x * 2.0f)-1.0f;
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


             //fov doit etre régler sur vertical pour FOV_AXIS
             float near, far, fov_vertical, fov_horizontal, right, left, top, bottom;



             float coef_deplacement = 1.0f;

             offSet.x = -offSet.x;

             /*Touches pour manip matrice*/
        /*if (Input.GetKey(KeyCode.R))
        {
            a00 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a00,-1.0f,1.0f);
        }
        if (Input.GetKey(KeyCode.T))
        {
            a01 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a01, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.Y))
        {
            a02 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a02, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.F))
        {
            a10 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a10, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.G))
        {
            a11 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a11, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.H))
        {
            a12 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a12, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.V))
        {
            a20 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a20, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.B))
        {
            a21 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a21, -1.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.N))
        {
            a22 += 0.01f * (Input.GetKey(KeyCode.Space) ? -1.0f : 1.0f);
            Mathf.Clamp(a22, -1.0f, 1.0f);
        }*/




        /*shear.SetRow(0, new Vector4(1.0f, a01, a02, 0.0f));
        shear.SetRow(1, new Vector4(a10, 1.0f, a12, 0.0f));
        //jouer sur la distance entre plan far et near
        //shear.SetRow(2, new Vector4(0.0f, 0.0f, offSet.y+1.0f, 0.0f));
        shear.SetRow(2, new Vector4(a20 , a21 , 1.0f, 0.0f));
        shear.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));*/


        //x
        //shear.SetRow(0, new Vector4(1.0f, offSet.y, 0.0f, 0.0f));

        /*shear.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, -offSet.x*4.0f));
        shear.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, -offSet.y*4.0f));
        shear.SetRow(2, new Vector4(offSet.x*2.0f, offSet.y*2.0f, 1.0f, 0.0f));*/


        //cam.transform.Translate(new Vector3(0.0f, -old_offset.y / coef_deplacement, 0.0f));
        //cam.transform.Translate(new Vector3(0.0f, offSet.y / coef_deplacement, 0.0f));

        /*cam.transform.Translate(new Vector3(-old_offset.x/ coef_deplacement, -old_offset.y / coef_deplacement, 0.0f));
        cam.transform.Translate(new Vector3(offSet.x/ coef_deplacement, offSet.y/ coef_deplacement, 0.0f));*/

        near = cam.nearClipPlane;
            far = cam.farClipPlane;
            fov_vertical = cam.fieldOfView;

            //ici base

            /*top = Mathf.Tan((fov_vertical / 2.0f) * Mathf.Deg2Rad);
            bottom = -top;
            right = top * cam.aspect;
            left = -right;*/


            //ici calcul en prenant en compte head tracking position
            //toa => pour chercher hauteur du coté opposé à l'angle du fov/2.0f
            top = near * Mathf.Tan((fov_vertical / 2.0f) * Mathf.Deg2Rad);
            bottom = -top;
            right = top * cam.aspect;
            left = -right;

            //right += offSet.x;
            //left -= offSet.x;
            
            //offSet.x += 1.0f;
            //offSet.x /= 2.0f;
            /*if(offSet.x >= 0.0f)
            {
                offSet.x += 1.0f;
                offSet.x /= 2.0f;
                right = 1.0f;
                left = -1.0f + offSet.x;
            }
            else
            {
                offSet.x += 1.0f;
                offSet.x /= 2.0f;
                left = -1.0f;
                right = 1-(offSet.x/2.0f);
            }*/



            Debug.Log("Left " + left + " Right " + right + " Top " + top + " Bottom " + bottom);

            float r2, l2, t2, b2;
            /*if(offSet.x >= 0.0f)
            {
                r2 = right + ((offSet.x/2.0f) * right);
                //l2 = left;
            }
            else
            {
                l2 = left + ((offSet.x / 2.0f) * right);//left + (offSet.x < 0.0f ? offSet.x : 0.0f);
                //r2 = right;//right + (offSet.x >= 0.0f ? offSet.x : 0.0f);
            }*/
            r2 = right * (1.0f - offSet.x);
            l2 = left * (1.0f + offSet.x);

            t2 = top * (1.0f - offSet.y);// + (offSet.y >= 0.0f ? offSet.y : 0.0f);
            b2 = bottom * (1.0f + offSet.y);// + (offSet.y < 0.0f ? offSet.y : 0.0f);


            /*Matrix4x4 camera = new Matrix4x4();
            camera.SetRow(0, new Vector4((2 * near) / (right - left), 0.0f, (right + left) / (right - left), 0.0f));
            camera.SetRow(1, new Vector4(0.0f, (2 * near) / (top - bottom), (top + bottom) / (top - bottom), 0.0f));
            camera.SetRow(2, new Vector4(0.0f, 0.0f, -(far + near) / (far - near), -(2 * near * far) / (far - near)));
            camera.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));*/

            //Transformation sur projection pour effet Mouvement HeadTracking (3D)//
            //--------------------------------------//

            //Transvection
            //shear.SetRow(0,new Vector4(1.0f,offSet.y,0.0f,0.0f));
            //shear.SetRow(1, new Vector4(offSet.x, 1.0f, 0.0f, 0.0f));
            //shear.SetRow(2, new Vector4(offSet.x, offSet.y, 1.0f, 0.0f));
            //shear.SetRow(0, new Vector4(1.0f, offSet.x, 0.0f, 0.0f));
            //shear.SetRow(1, new Vector4(offSet.y, 1.0f, 0.0f, 0.0f));
            //shear.SetRow(2, new Vector4(offSet.x*(5.0f/(2.0f*coef_deplacement)), offSet.y * (5.0f / (2.0f * coef_deplacement)), 1.0f, 0.0f));
            //shear.SetRow(2, new Vector4(0.0f, offSet.y * (5.0f / (2.0f * coef_deplacement)), 1.0f, 0.0f));
            //shear.SetRow(0, new Vector4(1.0f, offSet.x, 0.0f, 0.0f));
            //shear.SetRow(1, new Vector4(0.0f, 1.0f, offSet.y, 0.0f));
            //shear.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));

            /*methode 1 shear transformation v1*/
            /*shear.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
            shear.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
            shear.SetRow(2, new Vector4(offSet.x, offSet.y, 1.0f, 0.0f));
            cam.projectionMatrix = cam_base * shear;*/

            /*methode 2 shear transformation v2*/
            /*cam.projectionMatrix = cam_base * shear;*/
            
            /*methode 1 shear transformation v1 + translation dans matrice shear*/
            /*shear.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, -offSet.x * 4.0f));
            shear.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, -offSet.y * 4.0f));
            shear.SetRow(2, new Vector4(offSet.x, offSet.y, 1.0f, 0.0f));

            cam.projectionMatrix = cam_base * shear;*/


            Matrix4x4 camera = new Matrix4x4();
            /*methode 2 sur la matrice de projection*/

            //verif effet quand premier (top - bottom) ou (right - left) => reste de base, et juste changement sur le deuxieme calcul
            /*camera.SetRow(0, new Vector4((2 * near) / (r2 - l2), 0.0f, (r2 + l2) / (r2 - l2), 0.0f));
            camera.SetRow(1, new Vector4(0.0f, (2 * near) / (t2 - b2), (t2 + b2) / (t2 - b2), 0.0f));
            camera.SetRow(2, new Vector4(0.0f, 0.0f, -(far + near) / (far - near), -(2 * near * far) / (far - near)));
            camera.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));

            cam.projectionMatrix = camera;*/

            /*methode 2 sur projection + shear Translation*/
            shear.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, -offSet.x * 4.0f));
            shear.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, -offSet.y * 4.0f));
            shear.SetRow(2, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));

            camera.SetRow(0, new Vector4((2 * near) / (r2 - l2), 0.0f, (r2 + l2) / (r2 - l2), 0.0f));
            camera.SetRow(1, new Vector4(0.0f, (2 * near) / (top - bottom), (t2 + b2) / (t2 - b2), 0.0f));
            camera.SetRow(2, new Vector4(0.0f, 0.0f, -(far + near) / (far - near), -(2 * near * far) / (far - near)));
            camera.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));

            cam.projectionMatrix = camera * shear;

            //--------------------------------------//

            /*Version Kooima 3D stereoscopique*/
            //modif avec offSet.x et OffSet.y
            /*eye_pos.x = offSet.x;
            eye_pos.y = offSet.y;
            //eye_pos = new Vector3(offSet.x + transform.position.x, offSet.y + transform.position.y, transform.position.z);


            va = pa - eye_pos;
            vb = pb - eye_pos;
            vc = pc - eye_pos;

            //calcul de la distance entre le point et la position eye, comme differente direction opposé
            d = -Vector3.Dot(vn, va);

            //calcul du left right top bottom
            l = Vector3.Dot(vr, va) * cam.nearClipPlane / d;
            r = Vector3.Dot(vr, vb) * cam.nearClipPlane / d;
            b = Vector3.Dot(vu, va) * cam.nearClipPlane / d;
            t = Vector3.Dot(vu, vc) * cam.nearClipPlane / d;

            P.SetRow(0, new Vector4((2 * cam.nearClipPlane) / (r - l), 0.0f, (r + l) / (r - l), 0.0f));
            P.SetRow(1, new Vector4(0.0f, (2 * cam.nearClipPlane) / (t - b), (t + b) / (t - b), 0.0f));
            P.SetRow(2, new Vector4(0.0f, 0.0f, -(cam.farClipPlane + cam.nearClipPlane) / (cam.farClipPlane - cam.nearClipPlane), -(2 * cam.farClipPlane * cam.nearClipPlane) / (cam.farClipPlane - cam.nearClipPlane)));
            P.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));

            //vecteur 
            M.SetColumn(0,vr);
            M.SetColumn(1, vu);
            M.SetColumn(2, vn);
            M.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            M.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            //matrice orthogonal donc Transpose == inverse
            //inverse
            M = M.transpose;

            //Translate avec la position du eye (head tracking)
            T.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, -eye_pos.x));
            T.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, -eye_pos.y));
            T.SetRow(2, new Vector4(0.0f, 0.0f, 1.0f, -eye_pos.z));
            T.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            //cam.worldToCameraMatrix = P * M * T;
            //attention au sens T * M * proj ?
            //cam.projectionMatrix = P * M * T;*/


            //Debug.Log("point est de est de " + offSet);
            //cam.transform.position = new Vector3(cam.transform.position.x + ((offSet.x - old_offset.x)*factor), cam.transform.position.y + ((offSet.y - old_offset.y) * factor), cam.transform.position.z);            
            /*transform.position = Camera.main.ViewportToWorldPoint(new Vector3(-OpenCVFaceDetection.NormalizedFacePositions[0].x, 
            -OpenCVFaceDetection.NormalizedFacePositions[0].y, _camDistance));*/
            //Debug.Log()
        }
        else
        {
            offSet = old_offset;
            offsetZ = old_offset_z;
        }
        //cam.worldToCameraMatrix = P * M * T;
        
        //version 3D Kooima
        //cam.projectionMatrix = P * M * T;
    }
}
