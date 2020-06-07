﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JamCar : MonoBehaviour
{
    private readonly float minDelay = 2; // seconds
    private readonly float maxDelay = 4; // seconds

    [SerializeField]
    [Tooltip("Reference to the zone manager")]
    private GameObject zoneManager;

    // Start is called before the first frame update
    void Start()
    {
        if(zoneManager == null)
        {
            throw new System.NullReferenceException("No reference to the zone manager on " + transform.name);
        }
        // Define which car horn type to use
        AkSoundEngine.SetSwitch("Klaxons", new string[5] { "A", "B", "C", "D", "E" }[Random.Range(0, 5)], gameObject);
        StartCoroutine("CarHornCountdown");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CarHornCountdown()
    {
        float delay;
        for (; ; )
        {
            delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
            if (zoneManager.GetComponent<ZoneManager>().PlayerZone != ZoneManager.ZONE.PARK) // TO DO : Update when there are more zones
            {
                AkSoundEngine.PostEvent("Play_Klaxons", gameObject);
            }
        }
    }
}
