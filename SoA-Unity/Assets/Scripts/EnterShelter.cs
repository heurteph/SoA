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

    private GameObject ambianceManager;

    private string shelterTag;

    private void Awake()
    {
        inputs = InputsManager.Instance.Inputs;
        inputs.Player.Interact.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ExitShelter>().enabled = false;

        ambianceManager = GameObject.FindGameObjectWithTag("AmbianceManager");

        if (ambianceManager == null)
        {
            throw new System.NullReferenceException("Missing game object tagged with \"AmbianceManager\"");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shelterManager && inputs.Player.enabled)
        {
            inputs.Player.Interact.performed -= WorldToShelter;
            inputs.Player.Interact.Disable();

            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Shelter Entrance");

            if (Physics.Raycast(transform.position, transform.forward, out hit, shelterManager.MaxDistanceToDoor, mask))
            {
                shelter = shelterManager.GoInside(hit.collider.transform.parent.gameObject);
                shelterTag = hit.transform.parent.transform.tag;
                inputs.Player.Interact.performed += WorldToShelter;
                inputs.Player.Interact.Enable();
            }
        }
    }

    void WorldToShelter(InputAction.CallbackContext ctx)
    {
        StartCoroutine("Enter");
    }

    IEnumerator Enter()
    {
        inputs.Player.Disable();
   
        while (!Mathf.Approximately(shade.color.a, 1))
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, Mathf.Min(shade.color.a + Time.deltaTime / (shelterManager.TransitionDuration * 0.5f), 1));
            yield return null;
        }

        // Reset Character Position and Speed

        if (transform.GetComponent<PlayerFirst>().isActiveAndEnabled)
        {
            Transform warp = shelter.transform.Find("Warp Position");
            transform.GetComponent<PlayerFirst>().ResetTransform(warp.position, warp.rotation.eulerAngles.y);
            transform.GetComponent<PlayerFirst>().SetShelterSpeed();
        }

        // Reset Camera

        mainCamera.enabled = false;
        shelter.transform.Find("Shelter Camera").GetComponent<Camera>().enabled = true;

        // Reset sound

        AkSoundEngine.SetState("Dans_Lieu_Repos", "Oui");

        // Ambiance sound
        if (shelterTag == "Home")
        {
            ambianceManager.GetComponent<AmbianceManager>().PlayHomeAmbiance();
        }
        else if (shelterTag == "Shed")
        {
            ambianceManager.GetComponent<AmbianceManager>().PlayShedAmbiance();
        }
        else if (shelterTag == "Bar")
        {
            ambianceManager.GetComponent<AmbianceManager>().PlayBarAmbiance();
        }

        //GetComponent<PostWwiseAmbiance>().ParkAmbianceEventStop.Post(gameObject);
        //GetComponent<PostWwiseAmbiance>().ShelterAmbianceEventPlay.Post(gameObject);

        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, Mathf.Max(shade.color.a - Time.deltaTime / (shelterManager.TransitionDuration * 0.5f), 0));
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);
        energyBehaviour.IsReloading = true;

        inputs.Player.Interact.performed -= WorldToShelter;
        inputs.Player.Enable();

        GetComponent<ExitShelter>().enabled = true;
        GetComponent<EnterShelter>().enabled = false;
    }

    void OnDisable()
    {
        //inputs.Player.Disable();
    }

}  //FINISH
