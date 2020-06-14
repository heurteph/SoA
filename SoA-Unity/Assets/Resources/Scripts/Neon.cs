using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neon : MonoBehaviour
{
    [SerializeField]
    Light l;
    [SerializeField]
    bool activate = true;

    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        //nouvelle instance
        mat = new Material(mat);
        gameObject.GetComponent<MeshRenderer>().material = mat;
        //mat.SetVector("_Light_Pos",new Vector4(l.transform.position.x, l.transform.position.y, l.transform.position.z, 1.0f));
    }
    void Update()
    {
        mat.SetInt("_On", 1);//(activate ? 1 : 0));
        //ca ne sert pas dans Neon, car pour attenuation
        //mat.SetVector("_Light_Pos", new Vector4(l.transform.position.x, l.transform.position.y, l.transform.position.z, 1.0f));
        mat.SetFloat("_Intensity", l.intensity);
        mat.SetColor("_Color",l.color);
    }
}
