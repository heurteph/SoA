using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    [SerializeField]
    private Light insideLight;

    [SerializeField]
    private GameObject lightSquare;

    [SerializeField]
    private Material darkMat;

    [SerializeField]
    private Material lightMat;

    // Start is called before the first frame update
    void Start()
    {
        insideLight.enabled = false;
        lightSquare.GetComponent<MeshRenderer>().material = darkMat;

        StartCoroutine("Flash");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Flash()
    {
        float darkDuration, lightDuration;
        for (; ; )
        {
            darkDuration = Random.Range(10f, 15f);
            lightDuration = Random.Range(6f, 12f);

            yield return new WaitForSeconds(darkDuration);

            insideLight.enabled = true;
            lightSquare.GetComponent<MeshRenderer>().material = lightMat;

            yield return new WaitForSeconds(lightDuration);

            insideLight.enabled = false;
            lightSquare.GetComponent<MeshRenderer>().material = darkMat;
        }
    }
}
