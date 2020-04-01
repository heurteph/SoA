using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnergyBehaviour : MonoBehaviour
{
    [SerializeField]
    [Range(0,1000)]
    private float energy;

    [SerializeField]
    private DebuggerBehaviour debuggerBehaviour;

    public delegate void EnergyChangedHandler(float e);
    public event EnergyChangedHandler EnergyChangedEvent;

    // Start is called before the first frame update
    void Start()
    {
        EnergyChangedEvent += debuggerBehaviour.DisplayEnergy;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void DecreaseEnergy(float e)
    {
        energy -= e;

        EnergyChangedEvent(energy);

        if (energy <= 0)
        {
            energy = 0;
            OutOfEnergy();
        }

    }

    public void IncreaseEnergy(float e)
    {
        energy += e;
    }

    void OutOfEnergy() // pour l'instant
    {
        GetComponent<PlayerBehaviour>().enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}   // FINISH
