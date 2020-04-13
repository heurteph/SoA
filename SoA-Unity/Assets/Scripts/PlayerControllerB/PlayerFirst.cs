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
    private float speed = 8;

    [SerializeField]
    [Range(1.0f, 360.0f)]
    private float rotationSpeed = 150;

    [SerializeField]
    [Range(0.1f,10)]
    float turnBackTime = 0.35f; // seconds

    private float angle;
    private bool isTurningBack;

    void Awake()
    {
        angle = player.transform.rotation.eulerAngles.y;
        inputs = new Inputs();
        isTurningBack = false;
    }

    // Start is called before the first frame update
    void Start()
    {

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
        Vector3 newForward = beginForward;

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
} // FINISH
