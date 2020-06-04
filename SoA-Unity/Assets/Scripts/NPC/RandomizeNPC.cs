using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeNPC : MonoBehaviour
{
    private List<GameObject> props;
    private List<GameObject> beards;
    private List<GameObject> hairs;
    private List<GameObject> eyes;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        props = new List<GameObject>();
        beards = new List<GameObject>();
        hairs = new List<GameObject>();
        eyes = new List<GameObject>();

        foreach (Transform category in transform)
        {
            if(category.name.StartsWith("eyes"))
            {
                foreach (Transform eye in category)
                {
                    eyes.Add(eye.gameObject);
                    eye.gameObject.SetActive(false);
                }
            }
            else if (category.name.StartsWith("hair"))
            {
                foreach (Transform hair in category)
                {
                    hairs.Add(hair.gameObject);
                    hair.gameObject.SetActive(false);
                }
            }
            else if (category.name.StartsWith("beard"))
            {
                foreach (Transform beard in category)
                {
                    beards.Add(beard.gameObject);
                    beard.gameObject.SetActive(false);
                }
            }
            else if (category.name.StartsWith("props"))
            {
                foreach (Transform prop in category)
                {
                    props.Add(prop.gameObject);
                    prop.gameObject.SetActive(false);
                }
            }
        }

        RandomizeAppearance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RandomizeAppearance()
    {
        if (eyes.Count > 0)
        {
            eyes[Random.Range(0, eyes.Count)].SetActive(true);
        }

        if (beards.Count > 0)
        {
            index = Random.Range(0, beards.Count + 1);
            if (index < beards.Count) { beards[index]?.SetActive(true); } // can be beardless
        }

        if (props.Count > 0)
        {
            index = Random.Range(0, props.Count + 1);
            if (index < props.Count) { props[index].SetActive(true); } // can wear no hat
        }

        if (hairs.Count > 0)
        {
            index = Random.Range(0, hairs.Count + 1);
            if (index < hairs.Count) { hairs[index].SetActive(true); } // can be bald
        }
    }
}
