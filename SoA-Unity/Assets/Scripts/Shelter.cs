using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelter : MonoBehaviour
{
    private Inputs inputs;

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
        Debug.Log("Entering the shelter");
        // TO DO : Warp the player to the shelter
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
