using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float speed = 10.0f;
    [SerializeField]
    float rot_speed = 100.0f;
    [SerializeField]
    Camera cam;
    [SerializeField]
    float a = 1.0f, b = 1.0f;
    float pitch = 0.0f;
    float yaw = 0.0f;
    Matrix4x4 shear_mat;

    Vector3 currentEuler;

    // Start is called before the first frame update
    void Start()
    {
        shear_mat = new Matrix4x4(new Vector4(1,0,a,0), new Vector4(0, 1, b, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
        cam = gameObject.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        //float rot = Input.GetAxis("Horizontal") * rot_speed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        
        yaw += Input.GetAxis("Mouse X") * rot_speed * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * rot_speed * Time.deltaTime;

        //x => pitch y => yaw z => roll

        /*Vector3 pos = transform.position;

        transform.position = new Vector3(0, 0, 0);
        Quaternion tmp = transform.rotation;
        transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), Input.GetAxis("Mouse Y"));

        transform.rotation = new Quaternion(tmp.x + transform.rotation.x, tmp.y + transform.rotation.y, tmp.z + transform.rotation.z, 1.0f);
        transform.position = pos;
        
        transform.Translate(new Vector3(x, 0, z));
         */
        Vector3 pos = cam.transform.position;

        //yaw autour de y axe et pitch autour de x axe
        cam.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        //pour shearing apply
        //=> modify matrix of camera

        Matrix4x4 mat = new Matrix4x4(cam.projectionMatrix.GetColumn(0), cam.projectionMatrix.GetColumn(1), 
            cam.projectionMatrix.GetColumn(2), cam.projectionMatrix.GetColumn(3));
        //mat.SetColumn()


        //quand on passe pas de changement 
        //uniquement si on reprend derriere
        Matrix4x4 matproj = cam.projectionMatrix;
        //matproj[0, 1] = 1.1f * matproj[0, 1];
        //matproj[0, 2] = 1.001f * matproj[0, 2];


        Debug.Log("Camera look at " + cam.transform.forward);


        //cam.projectionMatrix = cam.projectionMatrix * shear_mat;
        
        
        //cam.projectionMatrix[0, 1] = 2 * cam.projectionMatrix[0, 1];// = new Matrix4x4();
        //cam.projectionMatrix[0, 2] = 2 * cam.projectionMatrix[0, 2];

        Debug.Log("Angles " + cam.transform.eulerAngles);
        Debug.Log("Local Angles " + cam.transform.eulerAngles);

        /*Vector3 mvt = new Vector3(dir.x * x, 0.0f, dir.z * z);

        Debug.Log("Dir " + dir);

        transform.position += mvt;*/

        //transform.Translate(new Vector3(x*dir.x, 0, z*dir.z));
        transform.Translate(new Vector3(x, 0, z));

        transform.forward = cam.transform.forward;

        //ramene la cam en 0 0 0 pour transforme
        /*cam.transform.position = new Vector3(0, 0, 0);
        cam.transform.Rotate(0, Hrot, 0);
        cam.transform.Rotate(Vrot, 0, 0);
        cam.transform.position = pos;*/

    }
}
