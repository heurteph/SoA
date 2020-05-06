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
        index = startIndex % points.Length;
        nextPoint = points[index];
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(nextPoint.transform.position - transform.position, Vector3.up));

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 5, transform.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(nextPoint.transform.position.x, transform.position.y, nextPoint.transform.position.z), speed * Time.deltaTime);

        Debug.Log(transform.name + ", test " + transform.position + " against " + nextPoint.transform.position);

        if(Vector3.ProjectOnPlane(transform.position,Vector3.up) == Vector3.ProjectOnPlane(nextPoint.transform.position, Vector3.up))
        //if(transform.position.x == nextPoint.transform.position.x && transform.position.z == nextPoint.transform.position.z)
        {
            Debug.Log("REACHED POSITION : " + transform.position + " at " + index);
            Vector3 startForward = transform.forward;
            index = (index + 1) % points.Length;
            nextPoint = points[index];
            Vector3 endForward = nextPoint.transform.forward;
            StartCoroutine("Turn", new Vector3[2] { startForward, endForward });
            Debug.Log("MOVING TO MY NEW TARGET : " + nextPoint.transform.position + " at " + index);
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 5, transform.position.z);
        }
    }

    private IEnumerator Turn(Vector3[] vectors)
    {
        Debug.Log("Start Coroutine");
        float t = 0;
        float speed = 0.4f;
        Vector3 startForward = vectors[0];
        Vector3 endForward   = vectors[1];

        while (t < 1.0f)
        {
            t = Mathf.Min(t + Time.deltaTime * speed, 1.0f);
            float sinerp = Mathf.Sin(t * Mathf.PI * 0.5f);
            Vector3 currentForward = Vector3.Slerp(startForward, endForward, t);
            transform.rotation = Quaternion.LookRotation(currentForward);
            yield return null;
        }
        Debug.Log("End Coroutine");
    }
}
