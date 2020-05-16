using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Protect : MonoBehaviour
{
    [SerializeField]
    private HearingScript hearingScript;

    [SerializeField]
    private VisionBehaviour visionScript;

    private Inputs inputs;

    private IAnimable player;

    private void Awake()
    {
        foreach(IAnimable controller in GetComponents<IAnimable>()) // does getcomponents works with interfaces ?
        {
            if((controller as MonoBehaviour).enabled)
            {
                player = controller;
            }
        }
        if(player == null)
        {
            throw new System.NullReferenceException("No player reference passed to Protect script");
        }
        if(hearingScript == null)
        {
            throw new System.NullReferenceException("No hearing script reference passed to Protect script");
        }

        inputs = InputsManager.Instance.Inputs;

        inputs.Player.ProtectEyes.performed += _ctx =>
        {
            player.IsProtectingEyes = true;    
            visionScript.CoverEyes();
            AkSoundEngine.SetState("Protection_Oui_Non", "Active");
        };
        inputs.Player.ProtectEyes.canceled += _ctx =>
        {
            player.IsProtectingEyes = false;
            visionScript.UncoverEyes();
            AkSoundEngine.SetState("Protection_Oui_Non", "Pas_active");
        };

        inputs.Player.ProtectEars.performed += _ctx =>
        {
            player.IsProtectingEars = true;
            hearingScript.PlugEars();
        };
        inputs.Player.ProtectEars.canceled += _ctx =>
        {
            player.IsProtectingEars = false;
            hearingScript.UnplugEars();
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        inputs.Player.Enable();
    }
    void OnDisable()
    {
        inputs.Player.Disable();
    }
}
