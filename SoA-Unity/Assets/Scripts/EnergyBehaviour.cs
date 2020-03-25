using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBehaviour : MonoBehaviour
{
    [SerializeField]
    [Range(0,1000)]
    private float energy;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void DecreaseEnergy(float e)
    {
        energy -= e;
    }

    public void IncreaseEnergy(float e)
    {
        energy += e;
    }

    

}   // FINISH
