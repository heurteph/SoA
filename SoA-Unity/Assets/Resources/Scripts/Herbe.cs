using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbe : MonoBehaviour
{
    [SerializeField]
    GameObject Player;
    [SerializeField]
    readonly int RADIUS = 20;
    [SerializeField]
    readonly float PAS = 5.0f;

    Material mat;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = (Player.transform.position - transform.position).magnitude;
        //mat.SetFloat("distance", distance);
        //fonction inverse linéaire en fonction de la distance
        float taille_base = 7;
        float taille = taille_base;
        int mode = 1;
        if (distance > RADIUS)
        {
            taille = taille_base - ((distance - RADIUS) /PAS);
            mode = 0;
            if (taille <= 1.0f)
                taille = 1.0f;
        }
        //pour le moment mode a 1
        //mat.SetInt("_DynamicRender", mode);
        mat.SetInt("_DynamicRender", 1);
        mat.SetFloat("_TessellationUniform", taille);
        //mat.shader.maximumLOD = 1;
        /*if (distance > 20)
        {
            mat.SetFloat("_TessellationUniform",1);
        }
        else
        {
            mat.SetFloat("_TessellationUniform", 8);
        }*/
    }
}
