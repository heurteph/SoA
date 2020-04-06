using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnergyBehaviour : MonoBehaviour
{
    [SerializeField]
    [Range(0,1000)]
    private float energy;

    private MonoBehaviour script;

    // Start is called before the first frame update
    void Start()
    {
        script = GetComponent<PlayerFollow>() ? GetComponent<PlayerFollow>() : (MonoBehaviour)GetComponent<PlayerFirst>();
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
        script.enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}   // FINISH
