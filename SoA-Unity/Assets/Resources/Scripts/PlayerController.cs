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

    float limit = 1.0f;

    Vector3 currentEuler;

    // Start is called before the first frame update
    void Start()
    {
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

        transform.Translate(new Vector3(x, 0, z));

        transform.forward = cam.transform.forward;
    }
}
