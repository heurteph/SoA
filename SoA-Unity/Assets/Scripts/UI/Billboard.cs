using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Main Camera")]
    private GameObject mainCamera;

    //private float zAngle;

    // Start is called before the first frame update
    void Start()
    {
        //zAngle = transform.rotation.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.LookAt(Vector3.zero);
        //transform.LookAt(mainCamera.transform.position);
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.position - transform.position);
        //transform.LookAt(Camera.main.transform.position);
    }
}
