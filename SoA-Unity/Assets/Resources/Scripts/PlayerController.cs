using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float speed = 10.0f;
    [SerializeField]
    float rot_speed = 100.0f;

    Vector3 currentEuler;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float rot = Input.GetAxis("Horizontal") * rot_speed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        //x => pitch y => yaw z => roll

        /*Vector3 pos = transform.position;

        transform.position = new Vector3(0, 0, 0);
        Quaternion tmp = transform.rotation;
        transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), Input.GetAxis("Mouse Y"));

        transform.rotation = new Quaternion(tmp.x + transform.rotation.x, tmp.y + transform.rotation.y, tmp.z + transform.rotation.z, 1.0f);
        transform.position = pos;*/

        transform.Translate(new Vector3(0,0,z));
        transform.Rotate(0, rot, 0);
    }
}
