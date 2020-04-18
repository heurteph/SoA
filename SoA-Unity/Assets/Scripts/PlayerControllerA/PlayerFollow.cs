using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Player settings")]
    [Space]

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    [Tooltip("The camera from which the player's forward direction is given")]
    private GameObject cameraHolder;

    [Space]
    [SerializeField]
    [Range(1.0f, 10.0f)]
    private float normalSpeed = 1;

    [SerializeField]
    [Tooltip("Speed when the character is losing energy")]
    [Range(1.0f, 10.0f)]
    private float hurrySpeed = 10;

    private float speed;

    [SerializeField]
    [Tooltip("Time in seconds to transition from normal state to hurry state")]
    [Range(0, 5)]
    private float timeTransitionToHurrySpeed = 3; // s

    [SerializeField]
    [Tooltip("Time in seconds to transition from hurry state to normal state")]
    [Range(0, 5)]
    private float timeTransitionToNormalSpeed = 3; // s

    private float backToNormalSpeedTimer = 0; // s

    [SerializeField]
    private Animator anim;


    void Awake()
    {
     //    angle = player.transform.rotation.eulerAngles.y;
        inputs = new Inputs();
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
        
         /* Vector3 translationZ = cameraMain.transform.forward*v.y;
         Vector3 translationX = cameraMain.transform.right * v.x;
         characterController.Move((translationX + translationZ)*speed*Time.deltaTime); */

        if (v.magnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.Euler(0, cameraHolder.transform.rotation.eulerAngles.y + Mathf.Rad2Deg * Mathf.Atan2(v.x, v.y), 0);   // cartesian to polar, starting from the Y+ axis as it's the one mapped to the camera's forward, thus using tan-1(x,y) and not tan-1(y,x) / No rotationSpeed * Time.deltaTime as it takes absolute orientation
            characterController.Move(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * v.magnitude * speed * Time.deltaTime);  // projection normalized to have the speed independant from the camera angle

        
                anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    public void Hurry(float energy)
    {
        backToNormalSpeedTimer = timeTransitionToNormalSpeed;
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
    }

} // FINISH
