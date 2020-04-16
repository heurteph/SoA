using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFirst : MonoBehaviour
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
    private float normalSpeed = 18;

    [SerializeField]
    [Range(1.0f, 100.0f)]
    [Tooltip("Speed when the character is losing energy")]
    private float hurrySpeed = 36;

    private float speed;

    [SerializeField]
    [Range(1.0f, 360.0f)]
    private float rotationSpeed = 150;

    [SerializeField]
    [Range(0.1f,10)]
    float turnBackTime = 0.35f; // seconds

    private float angle;
    private bool isTurningBack;

    [SerializeField]
    [Tooltip("Time in seconds to transition from normal state to hurry state")]
    [Range(0, 5)]
    private float timeTransitionToHurrySpeed = 3; // s

    [SerializeField]
    [Tooltip("Time in seconds to transition from hurry state to normal state")]
    [Range(0,5)]
    private float timeTransitionToNormalSpeed  = 3; // s

    private float backToNormalSpeedTimer       = 0; // s

    void Awake()
    {
        angle = player.transform.rotation.eulerAngles.y;
        inputs = new Inputs();
        isTurningBack = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        speed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputs.Player.Walk != null)
        {
            Walk(inputs.Player.Walk.ReadValue<Vector2>());
        }
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
            characterController.Move(translation * speed * Time.deltaTime);
            angle += v.x * rotationSpeed * Time.deltaTime;
            characterController.transform.rotation = Quaternion.Euler(0, angle, 0);
            
            if (v.y < 0) { StartCoroutine("TurnBack"); }
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
        backToNormalSpeedTimer = timeTransitionToNormalSpeed;
        if (speed != hurrySpeed)
        {
            StartCoroutine("BackToNormalSpeed");
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
    }
} // FINISH
