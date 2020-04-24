using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelterManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] shelterOutsides;

    [SerializeField]
    private GameObject[] shelterInsides;

    // Start is called before the first frame update
    void Start()
    {
        if(shelterOutsides.Length != shelterInsides.Length)
        {
            throw new System.SystemException("Shelter's entrances and exits list do not match");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GoInside(GameObject o)
    {
        for(int i = 0; i < shelterOutsides.Length; i++)
        {
            if (Object.ReferenceEquals(o, shelterOutsides[i]))
            {
                return shelterInsides[i];
            }
            Debug.Log(o.name + " not equal to " + shelterOutsides[i]);
        }
        throw new System.SystemException(o.name + " not found in the shelter list");
    }

    public GameObject GoOutside(GameObject o)
    {
        for (int i = 0; i < shelterInsides.Length; i++)
        {
            if (Object.ReferenceEquals(o, shelterInsides[i]))
            {
                return shelterOutsides[i];
            }
        }
        throw new System.SystemException(o.name + " not found in the shelter list");
    }
}
