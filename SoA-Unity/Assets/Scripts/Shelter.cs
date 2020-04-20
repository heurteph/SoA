using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shelter : MonoBehaviour
{
    private Inputs inputs;

    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    [SerializeField]
    private Image shade;

    private void Awake()
    {
        inputs = new Inputs();
        inputs.Player.Interact.performed += ctx => OpenDoor();
        inputs.Player.Interact.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OpenDoor()
    {
        //if (!energyBehaviour.IsFull())
        //{
            Debug.Log("Entering the shelter");
            // TO DO : Warp the player to the shelter
            StartCoroutine("Refill");
        //}
    }

    IEnumerator Refill()
    {
        inputs.Player.Disable();
        while(!Mathf.Approximately(shade.color.a, 1))
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, shade.color.a + 0.05f);
            yield return null;
        }
        while(!energyBehaviour.IsFull())
        {
            float step = 15; // TO DO : smoothstep
            energyBehaviour.IncreaseEnergy(step);
            yield return null;
        }
        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, shade.color.a - 0.05f);
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);
        inputs.Player.Enable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shelter"))
        {
            // TO DO : Display UI
            inputs.Player.Interact.Enable();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shelter"))
        {
            // TO DO : Display UI
            inputs.Player.Interact.Disable();
        }
    }

    void OnDisable()
    {
        inputs.Player.Disable();
    }
}
