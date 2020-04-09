using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]
    private GameObject[] points;

    [SerializeField]
    private int startIndex = 0;

    [SerializeField]
    private float speed = 5;

    private GameObject nextPoint;
    private int index;

    // Start is called before the first frame update
    void Start()
    {
        index = startIndex;
        nextPoint = points[++index % points.Length];
        transform.LookAt(nextPoint.transform);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity))
        {
            transform.position = new Vector3(transform.position.x, hit.transform.position.y + 10, transform.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextPoint.transform.position, speed * Time.deltaTime);
        if(transform.position == nextPoint.transform.position)
        {
            nextPoint = points[++index % points.Length];
            transform.LookAt(nextPoint.transform);
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity))
        {
            transform.position = new Vector3(transform.position.x, hit.transform.position.y + 10, transform.position.z);
        }
    }
}
