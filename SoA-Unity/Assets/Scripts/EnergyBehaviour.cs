using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
