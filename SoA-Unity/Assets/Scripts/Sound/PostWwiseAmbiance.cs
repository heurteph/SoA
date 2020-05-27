using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class PostWwiseAmbiance : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The event of the park ambiance")]
    private AK.Wwise.Event parkAmbianceEvent;
    public AK.Wwise.Event ParkAmbianceEvent { get { return parkAmbianceEvent; } }

    [SerializeField]
    [Tooltip("The event of the city ambiance")]
    private AK.Wwise.Event cityAmbianceEvent;
    public AK.Wwise.Event CityAmbianceEvent { get { return cityAmbianceEvent; } }

    [SerializeField]
    [Tooltip("The ambiance the game starts with")]
    private AMBIANCE ambiance = AMBIANCE.NONE;

    // Start is called before the first frame update
    void Start()
    {
        if(parkAmbianceEvent == null)
        {
            throw new System.NullReferenceException("No parkAmbiance sound event set on the script PostWwiseAmbiance");
        }
        if (cityAmbianceEvent == null)
        {
            throw new System.NullReferenceException("No cityAmbiance sound event set on the script PostWwiseAmbiance");
        }

        switch (ambiance)
        {
            case AMBIANCE.PARK:
                CityAmbianceEvent.Stop(gameObject);
                ParkAmbianceEvent.Post(gameObject);
                break;
            case AMBIANCE.CITY:
                ParkAmbianceEvent.Stop(gameObject);
                CityAmbianceEvent.Post(gameObject);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
