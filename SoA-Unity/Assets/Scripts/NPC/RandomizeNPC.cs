using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeNPC : MonoBehaviour
{
    private List<GameObject> props;

    private List<GameObject> beards;

    private List<GameObject> hairs;

    private List<GameObject> eyes;

    // Start is called before the first frame update
    void Start()
    {
        props = new List<GameObject>();
        beards = new List<GameObject>();
        hairs = new List<GameObject>();
        eyes = new List<GameObject>();
        foreach (Transform prop in transform.Find("props_M_set01"))
        {
            props.Add(prop.gameObject);
            prop.gameObject.SetActive(false);
        }
        foreach (Transform beard in transform.Find("beard_M_set01"))
        {
            beards.Add(beard.gameObject);
            beard.gameObject.SetActive(false);
        }
        foreach (Transform hair in transform.Find("hair_M_set01"))
        {
            hairs.Add(hair.gameObject);
            hair.gameObject.SetActive(false);
        }
        foreach (Transform eye in transform.Find("eyes_set05"))
        {
            eyes.Add(eye.gameObject);
            eye.gameObject.SetActive(false);
        }

        RandomizeAppearance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RandomizeAppearance()
    {
        eyes[Random.Range(0, eyes.Count)].SetActive(true);

        beards[Random.Range(0, beards.Count + 1)]?.SetActive(true); // can be beardless

        props[Random.Range(0, props.Count + 1)]?.SetActive(true); // can wear no hat

        hairs[Random.Range(0, hairs.Count + 1)]?.SetActive(true); // can be bald
    }
}
