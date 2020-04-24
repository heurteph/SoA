using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ExitShelter : MonoBehaviour
{
    private Inputs inputs;

    
    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    [SerializeField]
    private Image shade;

    private GameObject shelter;
    private Camera shelterCamera;

    [SerializeField]
    private ShelterManager shelterManager;

    [SerializeField]
    private Camera mainCamera;

    private void Awake()
    {
        inputs = InputsManager.Instance.Inputs;

        inputs.Player.Interact.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        inputs.Player.Interact.performed -= ShelterToWorld;
        inputs.Player.Interact.Disable();

        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Shelter Exit");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10.0f, mask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            
            shelterCamera = hit.transform.parent.transform.Find("Shed Camera").GetComponent<Camera>();

            shelter = shelterManager.GoOutside(hit.collider.transform.parent.gameObject);
            inputs.Player.Interact.performed += ShelterToWorld;

            if (inputs.Player.enabled)
            {
                inputs.Player.Interact.Enable();
            }
        }
    }

    void ShelterToWorld(InputAction.CallbackContext ctx)
    {
        Debug.Log("Open door from inside");
        // TO DO : Warp the player to the main world
        StartCoroutine("Exit");
    }

    IEnumerator Exit()
    {
        inputs.Disable();

        energyBehaviour.IsReloading = false;
        while (!Mathf.Approximately(shade.color.a, 1))
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, shade.color.a + 0.05f);
            yield return null;
        }
        this.GetComponent<Transform>().position = shelter.transform.Find("Warp Position").transform.position + Vector3.up * 3.75f;

        inputs.Player.Interact.performed -= ShelterToWorld;
        inputs.Player.Interact.Disable();


        Debug.Log("Camera Switch Shed to Main");
        mainCamera.enabled = true;
        shelterCamera.enabled = false;

        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, shade.color.a - 0.05f);
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);


        inputs.Player.Interact.Enable();
        inputs.Enable();

        GetComponent<EnterShelter>().enabled = true;
        GetComponent<ExitShelter>().enabled = false;
    }

   /* private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shelter Exit"))
        {
            Debug.Log("OnTriggerEnter Inside");
            shelterCamera = other.transform.parent.transform.Find("Shed Camera").GetComponent<Camera>();
            shelter = shelterManager.GoOutside(other.transform.parent.gameObject);
            inputs.Player.Interact.performed += ShelterToWorld;

            if (inputs.Player.enabled)
            {
                inputs.Player.Interact.Enable();
            }
        }
    } */

    /* private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shelter Exit"))
        {
            Debug.Log("OnTriggerExit Inside");
            inputs.Player.Interact.performed -= ShelterToWorld;
            inputs.Player.Interact.Disable();
        }
    } */

    void OnDisable()
    {
        //inputs.Player.Disable();
    }
}
