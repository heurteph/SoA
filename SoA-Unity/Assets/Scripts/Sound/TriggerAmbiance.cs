using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AMBIANCE { PARK, CITY, NONE };

public class TriggerAmbiance : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The type of ambiance that is triggered when crossing this collider")]
    private AMBIANCE ambiance = AMBIANCE.NONE;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.GetComponent<PostWwiseAmbiance>() == null)
            {
                throw new System.NullReferenceException("No PostWwiseAmbiance script set on the player");
            }

            if (ambiance == AMBIANCE.PARK)
            {
                other.GetComponent<PostWwiseAmbiance>().CityAmbianceEvent.Stop(other.gameObject);
                other.GetComponent<PostWwiseAmbiance>().ParkAmbianceEvent.Post(other.gameObject);
            }
            else if (ambiance == AMBIANCE.CITY)
            {
                other.GetComponent<PostWwiseAmbiance>().ParkAmbianceEvent.Stop(other.gameObject);
                other.GetComponent<PostWwiseAmbiance>().CityAmbianceEvent.Post(other.gameObject);
            }
        }
    }
}
