﻿using System.Collections;
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

        if(Physics.Raycast(transform.position, transform.forward, out hit, shelterManager.MaxDistanceToDoor, mask)) 
        {
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
        this.GetComponent<Transform>().position = shelter.transform.Find("Warp Position").transform.position + Vector3.up * 3.75f;

        inputs.Player.Interact.performed -= WorldToShelter;
        inputs.Player.Interact.Disable();

        mainCamera.enabled = false;
        shelter.transform.Find("Shed Camera").GetComponent<Camera>().enabled = true;

        while (shade.color.a > 0)
        {
            shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, Mathf.Max(shade.color.a - Time.deltaTime / (shelterManager.TransitionDuration * 0.5f), 0));
            yield return null;
        }
        shade.color = new Color(shade.color.r, shade.color.g, shade.color.b, 0);
        energyBehaviour.IsReloading = true;

        inputs.Player.Interact.Enable();
        inputs.Player.Enable();

        GetComponent<ExitShelter>().enabled = true;
        GetComponent<EnterShelter>().enabled = false;
    }

    void OnDisable()
    {
        //inputs.Player.Disable();
    }

}  //FINISH
