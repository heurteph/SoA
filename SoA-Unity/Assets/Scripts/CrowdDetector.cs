using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdDetector : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private List <GameObject> NPCs;

    [SerializeField]
    private int maxCrowdNumber;

    [SerializeField]
    private int crowdDamage;

    [SerializeField]
    private float maxDistance;


    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    private delegate void CrowdThresholdHandler(float b);
    private event CrowdThresholdHandler crowdThresholdEvent;

    // Start is called before the first frame update
    void Start()
    {
        crowdThresholdEvent += energyBehaviour.DecreaseEnergy;
    }

    // Update is called once per frame
    void Update()
    {
        DetectCrowd();
    }



    void DetectCrowd()
    {

        RaycastHit hit;

        LayerMask mask = LayerMask.GetMask("NPC");

        int crowdNumber = 0;

        foreach (GameObject o in NPCs)
        {
           
            if ( Physics.Raycast(transform.position, (o.transform.position - transform.position).normalized, out hit, maxDistance, mask) )
            {
                crowdNumber++;

                Debug.Log(crowdNumber + "crowd" );
            } 
        }

        if (crowdNumber > maxCrowdNumber)
        {
            crowdThresholdEvent(crowdDamage);
        }

    }

}  // FINISH
