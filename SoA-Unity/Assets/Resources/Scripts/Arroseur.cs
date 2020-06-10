using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arroseur : MonoBehaviour
{
    [SerializeField]
    float speed = 10.0f;
    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;

        Vector3 pivot = new Vector3(gameObject.transform.parent.transform.position.x, gameObject.transform.parent.transform.position.y, gameObject.transform.parent.transform.position.z);
        pivot = transform.position;
        Debug.Log("Mat point " + transform.position);
        mat.SetVector("_Pivot",new Vector4(pivot.x, pivot.y, pivot.z, 1.0f));
        mat.SetFloat("_DistanceMax",transform.localScale.x);
        mat.SetFloat("_height", transform.localScale.y);
    }

    // Update is called once per frame
    void Update()
    {

        transform.RotateAround(transform.parent.position,new Vector3(0.0f, 1.0f, 0.0f), Time.deltaTime * speed);
        /*MeshFilter mf = GetComponent<MeshFilter>();
        for (int i = 0; i < mf.mesh.vertexCount; i++)
        {
            Vector3 vertex = mf.mesh.vertices[i];
            float x = vertex.x;
            float y = vertex.y;
            vertex.x = x * Mathf.Cos(Time.realtimeSinceStartup) + y * Mathf.Sin(Time.realtimeSinceStartup);
            vertex.y = x * -Mathf.Sin(Time.realtimeSinceStartup) + y * Mathf.Cos(Time.realtimeSinceStartup);
            mf.mesh.vertices[i] = vertex;
        }*/
        
        //au cas ou changement mais normalement non
        mat.SetVector("_Pivot", new Vector4(transform.position.x, transform.position.y, transform.position.z, 1.0f));
        mat.SetFloat("_DistanceMax", transform.localScale.x);
    }
}
