using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfficheScript : MonoBehaviour
{
    [SerializeField]
    Light l;
    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Light Position "+l.transform.position);
        mat.SetVector("_LightPosition",gameObject.transform.parent.position);
        mat.SetFloat("_LightIntensity",l.intensity);
        mat.SetColor("_ContourColor",l.color);
    }
}
