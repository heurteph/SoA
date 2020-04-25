﻿using System.Collections;
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

        if (Physics.Raycast(transform.position, transform.forward, out hit, shelterManager.MaxDistanceToDoor, mask))
        {
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
        this.GetComponent<Transform>().position = shelter.transform.Find("Warp Position").transform.position + Vector3.up * 3.75f;

        inputs.Player.Interact.performed -= ShelterToWorld;
        inputs.Player.Interact.Disable();

        mainCamera.enabled = true;
        shelterCamera.enabled = false;

        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, Mathf.Max(shade.color.a - Time.deltaTime / (shelterManager.TransitionDuration * 0.5f),0));
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);

        inputs.Player.Interact.Enable();
        inputs.Enable();

        GetComponent<EnterShelter>().enabled = true;
        GetComponent<ExitShelter>().enabled = false;
    }

    void OnDisable()
    {
        //inputs.Player.Disable();
    }
}
