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

    private Image shade;

    private GameObject shelter;
    private Camera shelterCamera;

    [SerializeField]
    private ShelterManager shelterManager;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GameObject cameraHolder;

    private GameObject ambianceManager;

    private string shelterTag;

    private void Awake()
    {
        inputs = InputsManager.Instance.Inputs;
        inputs.Player.Interact.Disable();

        ambianceManager = GameObject.FindGameObjectWithTag("AmbianceManager");
        shade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Image>();

        if (ambianceManager == null)
        {
            throw new System.NullReferenceException("Missing game object tagged with \"AmbianceManager\"");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<PlayerFirst>().SetShelterSpeed();

        if (shelterManager && inputs.Player.enabled)
        {
            inputs.Player.Interact.performed -= ShelterToWorld;
            inputs.Player.Interact.Disable();

            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Shelter Exit");

            if (Physics.Raycast(transform.position, transform.forward, out hit, shelterManager.MaxDistanceToDoor, mask))
            {
                shelterCamera = hit.transform.parent.transform.Find("Shelter Camera").GetComponent<Camera>();
                shelter = shelterManager.GoOutside(hit.collider.transform.parent.gameObject);
                shelterTag = hit.transform.parent.transform.tag;
                inputs.Player.Interact.performed += ShelterToWorld;

                if (inputs.Player.enabled)
                {
                    inputs.Player.Interact.Enable();
                }
            }
        }
    }

    void ShelterToWorld(InputAction.CallbackContext ctx)
    {
        StartCoroutine("Exit");
    }

    IEnumerator Exit()
    {
        inputs.Player.Disable();

        energyBehaviour.IsReloading = false;
        while (!Mathf.Approximately(shade.color.a, 1))
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, Mathf.Min(shade.color.a + Time.deltaTime / (shelterManager.TransitionDuration * 0.5f),1));
            yield return null;
        }

        // Reset character position and speed

        if(transform.GetComponent<PlayerFirst>().isActiveAndEnabled)
        {
            Transform warp = shelter.transform.Find("Warp Position");
            GetComponent<PlayerFirst>().ResetTransform(warp.position, warp.rotation.eulerAngles.y);
            GetComponent<PlayerFirst>().ResetSpeed();
        }

        // Reset camera

        mainCamera.enabled = true;
        if (cameraHolder.GetComponent<CameraFollow>().isActiveAndEnabled)
        {
            cameraHolder.GetComponent<CameraFollow>().ResetCameraToFrontView();
        }
        shelterCamera.enabled = false;

        // Reset sound
        AkSoundEngine.SetState("Dans_Lieu_Repos", "Non");

        // Ambiance sound
        if (shelterTag == "Home" || shelterTag == "Bar")
        {
            ambianceManager.GetComponent<AmbianceManager>().PlayCityAmbiance();
        }
        else if (shelterTag == "Shed")
        {
            ambianceManager.GetComponent<AmbianceManager>().PlayParkAmbiance();
        }
        
        //GetComponent<PostWwiseAmbiance>().ShelterAmbianceEventStop.Post(gameObject);
        //GetComponent<PostWwiseAmbiance>().ParkAmbianceEventPlay.Post(gameObject);

        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, Mathf.Max(shade.color.a - Time.deltaTime / (shelterManager.TransitionDuration * 0.5f),0));
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);

        inputs.Player.Interact.performed -= ShelterToWorld;
        inputs.Player.Enable();

        GetComponent<PlayerFirst>().IsInsideShelter = false;

        GetComponent<EnterShelter>().enabled = true;
        GetComponent<ExitShelter>().enabled = false;
    }

    void OnDisable()
    {
        //inputs.Player.Disable();
    }
}
