using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnterShelter : MonoBehaviour
{
    private Inputs inputs;

    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    [SerializeField]
    private Image shade;

    private GameObject shelter;

    [SerializeField]
    private ShelterManager shelterManager;

    [SerializeField]
    private Camera mainCamera;

    private void Awake()
    {
        inputs = InputsManager.Instance.Inputs;

        inputs.Player.Interact.Disable();


        //GetComponent<BoxCollider>().enabled = false;
        //GetComponent<BoxCollider>().isTrigger = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ExitShelter>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        inputs.Player.Interact.performed -= WorldToShelter;
        inputs.Player.Interact.Disable();
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Shelter Entrance");
        if(Physics.Raycast(transform.position, transform.forward, out hit, 150.0f, mask)) 
        {
      
                Debug.DrawLine(transform.position, hit.point, Color.red);
                shelter = shelterManager.GoInside(hit.collider.transform.parent.gameObject);
                inputs.Player.Interact.performed += WorldToShelter;

                if (inputs.Player.enabled)
                {
                    inputs.Player.Interact.Enable();
                }
        }
    }

    void WorldToShelter(InputAction.CallbackContext ctx)
    {
        Debug.Log("Open door from outside");
        // TO DO : Warp the player to the shelter
        StartCoroutine("Enter");
    }

    IEnumerator Enter()
    {
        inputs.Disable();
   
        while (!Mathf.Approximately(shade.color.a, 1))
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, shade.color.a + 0.05f);
            yield return null;
        }
        this.GetComponent<Transform>().position = shelter.transform.Find("Warp Position").transform.position + Vector3.up * 3.75f;

        inputs.Player.Interact.performed -= WorldToShelter;
        inputs.Player.Interact.Disable();

        Debug.Log("Camera Switch Main to Shed");
        mainCamera.enabled = false;
        shelter.transform.Find("Shed Camera").GetComponent<Camera>().enabled = true;


        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, shade.color.a - 0.05f);
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);
        energyBehaviour.IsReloading = true;

        inputs.Player.Interact.Enable();
        inputs.Enable();

        GetComponent<ExitShelter>().enabled = true;
        GetComponent<EnterShelter>().enabled = false;
    }

    /* private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shelter Entrance"))
        {
            Debug.Log("OnTriggerEnter Outside");
            shelter = shelterManager.GoInside(other.transform.parent.gameObject);
            inputs.Player.Interact.performed += WorldToShelter;

            if (inputs.Player.enabled)
            {
                inputs.Player.Interact.Enable();
            }
        }
    } */

  /*  private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shelter Entrance"))
        {
            Debug.Log("OnTriggerExit Outside");
            inputs.Player.Interact.performed -= WorldToShelter;
            inputs.Player.Interact.Disable();
        }
    } */

    void OnDisable()
    {
        //inputs.Player.Disable();
    }
}  //FINISH
