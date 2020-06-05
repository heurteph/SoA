using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnergyBehaviour : MonoBehaviour
{
    private Inputs inputs;

    [SerializeField]
    [Range(0,1000)]
    private float energy;
    public float Energy { get { return energy; } set { energy = value; } }

    private MonoBehaviour script;

    [SerializeField]
    private DebuggerBehaviour debuggerBehaviour;

    public delegate void EnergyChangedHandler(float e);
    public event EnergyChangedHandler EnergyChangedEvent;

    public delegate void EnterDamageStateHandler();
    public event EnterDamageStateHandler EnterDamageStateEvent;

    public delegate void OutOfEnergyHandler();
    public event OutOfEnergyHandler OutOfEnergyEvent;

    private bool isReloading;
    public bool IsReloading { get { return isReloading; } set { isReloading = value; } }
    
    [SerializeField]
    [Tooltip("Refilling speed in energy point/second")]
    private int refillRate = 10;

    private void Awake()
    {
        isReloading = false;

        inputs = InputsManager.Instance.Inputs;

        script = GetComponent<PlayerFollow>() ? GetComponent<PlayerFollow>() : (MonoBehaviour)GetComponent<PlayerFirst>();
        EnergyChangedEvent += debuggerBehaviour.DisplayEnergy;
        EnergyChangedEvent += GetComponent<PlayerFirst>().Hurry;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       if(isReloading)
       {
            IncreaseEnergy(refillRate);
       }
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

        EnergyChangedEvent(energy);

        if(energy > 1000)
        {
            energy = 1000;
        }
    }

    void OutOfEnergy() // pour l'instant
    {
        script.enabled = false;
        OutOfEnergyEvent?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsFull()
    {
        return energy == 1000;
    }

}   // FINISH
