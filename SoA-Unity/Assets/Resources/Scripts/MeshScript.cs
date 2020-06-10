using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshScript : MonoBehaviour
{
    [SerializeField]
    Color color;
    void Awake()
    {
        Material mat = gameObject.GetComponent<MeshRenderer>().material;
        mat = new Material(mat);
        //mat.SetVector("_Color",new Vector4(color.r, color.g, color.b, color.a));
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }
}
