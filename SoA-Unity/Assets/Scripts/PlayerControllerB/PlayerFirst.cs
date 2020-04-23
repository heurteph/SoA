﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAnimable
{
    Animator Anim { get; }
    bool IsProtected { get; set; }
}
public class PlayerFirst : MonoBehaviour, IAnimable
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Player Settings")]
    [Space]

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private CharacterController characterController;

    [Space]

    [SerializeField]
    [Range(1.0f, 20.0f)]
    [Tooltip("Speed of the character")]
    private float normalSpeed = 18;

    private float speed;

    [SerializeField]
    [Tooltip("Speed of turning right or left")]
    [Range(1.0f, 360.0f)]
    private float rotationSpeed = 150;

    [SerializeField]
    [Tooltip("Duration of the half-turn")]
    [Range(0.1f,10)]
    float turnBackTime = 0.35f; // seconds

    private float angle;
    private bool isTurningBack;

    [Space]
    [Header("Hurry State")]

    [SerializeField]
    [Range(1.0f, 100.0f)]
    [Tooltip("Speed when the character is losing energy")]
    private float hurrySpeed = 36;

    [SerializeField]
    [Tooltip("Delay in seconds to transition to normal state after the danger has passed")]
    [Range(0,30)]
    private float delayToNormalState  = 3; // s

    private float backToNormalSpeedTimer       = 0; // s

    private bool isHurry;
    public bool IsHurry { get { return isHurry; } }

    private bool isProtected;
    public bool IsProtected { get { return isProtected; } set { isProtected = value; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    private Vector3 movement;

    [SerializeField]
    private Transform groundedPosition;

    void Awake()
    {
        angle = player.transform.rotation.eulerAngles.y;

        inputs = new Inputs();

        // TO MOVE TO GAME MANAGER
        inputs.Player.Quit.performed += _ctx => Application.Quit();

        isTurningBack = false;
        isHurry = false;
        isProtected = false;

        movement = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        speed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //StickToGround();

        if (inputs.Player.Walk != null)
        {
            Walk(inputs.Player.Walk.ReadValue<Vector2>());
        }

        characterController.Move(movement);
        movement = Vector3.zero;
    }

    void OnEnable()
    {
        inputs.Player.Enable();
    }

    void OnDisable()
    {
        inputs.Player.Disable();
    }

    void Walk(Vector2 v)
    {
        if (!isTurningBack)
        {
            Vector3 translation = player.transform.forward * v.y;
            movement += translation * speed * Time.deltaTime;
            angle += v.x * rotationSpeed * Time.deltaTime;
            characterController.transform.rotation = Quaternion.Euler(0, angle, 0);
            
            if (v.y < 0) { StartCoroutine("TurnBack"); }

            if (v.magnitude < Mathf.Epsilon)
            {
                anim.SetBool("isWalking", false);
            } else
            {
                anim.SetBool("isWalking", true);
            }
        }
    }

    void StickToGround()
    {
        RaycastHit hit;
        LayerMask ground = LayerMask.GetMask("Ground");

        if (Physics.Raycast(groundedPosition.position, -Vector3.up, out hit, Mathf.Infinity, ground) || Physics.Raycast(groundedPosition.position, Vector3.up, out hit, Mathf.Infinity, ground))
        {
            movement = (hit.point - groundedPosition.position);
        }
        else
        {
            movement = Vector3.zero;
        }
    }

    IEnumerator TurnBack()
    {
        isTurningBack = true;

        Vector3 beginForward =  transform.forward;
        Vector3 endForward   = -transform.forward;
        Vector3 newForward   = beginForward;

        float elapsedTime  = 0;

        while (elapsedTime != turnBackTime)
        {
            elapsedTime = Mathf.Min(elapsedTime + Time.deltaTime, turnBackTime);
            newForward = Vector3.RotateTowards(beginForward, endForward, Mathf.PI * elapsedTime / turnBackTime, 0.0f); // turn back = PI
            transform.rotation = Quaternion.LookRotation(newForward);
            yield return null;
        }
        angle -= 180; // synchronize with update
        isTurningBack = false;
    }

    public void Hurry(float energy)
    {
        isHurry = true;
        backToNormalSpeedTimer = delayToNormalState;
        if (speed != hurrySpeed)
        {
            StartCoroutine("TransitionToNormalSpeed");
        }
        speed = hurrySpeed;
    }

    IEnumerator TransitionToNormalSpeed()
    {
        while (backToNormalSpeedTimer > 0)
        {
            backToNormalSpeedTimer -= Time.deltaTime;
            yield return null;
        }
        backToNormalSpeedTimer = 0;
        speed = normalSpeed;
        isHurry = false;
    }

} // FINISH
