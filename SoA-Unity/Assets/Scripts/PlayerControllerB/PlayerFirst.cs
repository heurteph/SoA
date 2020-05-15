using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public interface IAnimable
{
    Animator Anim { get; }
    bool IsProtectingEyes { get; set; }
    bool IsProtectingEars { get; set; }
}
public class PlayerFirst : MonoBehaviour, IAnimable
{

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
    private float turnBackTime = 0.35f; // seconds

    [SerializeField]
    [Tooltip("Sensitivity of the Y-Axis to trigger the half-turn")]
    [Range(-1,0)]
    private float turnBackThreshold = -0.5f;

    private float steeringAngle;
    public float SteeringAngle { get { return steeringAngle; } set { steeringAngle = value; } }
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

    private float backToNormalSpeedTimer = 0; // s

    private bool isHurry;
    public bool IsHurry { get { return isHurry; } }

    private bool isProtectingEyes;
    public bool IsProtectingEyes { get { return isProtectingEyes; } set { isProtectingEyes = value; } }


    private bool isProtectingEars;
    public bool IsProtectingEars { get { return isProtectingEars; } set { isProtectingEars = value; } }

    private bool isDamaged;
    public bool IsDamaged { get { return isDamaged; } set { isDamaged = value; } }

    private bool isRunning;
    public bool IsRunning { get { return isRunning; } set { isRunning = value; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    private Vector3 movement;

    [Space]
    [Header("Ground Detector")]

    [SerializeField]
    private Transform groundLevelPosition;

    [SerializeField]
    private Transform raycastPosition;

    [SerializeField]
    private GameObject wwiseGameObjectFootstep;

    [SerializeField]
    private GameObject wwiseGameObjectBreath;

    void Awake()
    {
        steeringAngle = player.transform.rotation.eulerAngles.y;

        inputs = InputsManager.Instance.Inputs;

        // TO MOVE TO GAME MANAGER
        inputs.Player.Quit.performed += _ctx => Application.Quit();

        isTurningBack = false;
        isHurry = false;
        isProtectingEyes = false;
        isProtectingEars = false;
        isRunning = false;

        movement = Vector3.zero;

        if(wwiseGameObjectFootstep == null)
        {
            throw new System.Exception("Missing reference to a Wwise GameObject");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        speed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputs.Player.enabled) // Compulsory, as Disabling or Enabling an Action also Enable the ActionGroup !!!
        {
            StickToGround();

            Walk(inputs.Player.Walk.ReadValue<Vector2>());

            characterController.Move(movement);
            movement = Vector3.zero;

            if (isDamaged)
            {
                //inputs.Player.Walk.Disable();
                //inputs.Player.ProtectEyes.Enable();
                //inputs.Player.ProtectEars.Enable();
                AKRESULT result;
                result = AkSoundEngine.SetSwitch("Court_Marche", "Court", wwiseGameObjectFootstep); // Running step sounds
                result = AkSoundEngine.SetSwitch("Court_Marche", "Court", wwiseGameObjectBreath); // Running step sounds
            }

            if (isProtectingEyes || isProtectingEars)
            {
                //inputs.Player.Walk.Enable();
                isDamaged = false; // To check
                AKRESULT result;
                result = AkSoundEngine.SetSwitch("Court_Marche", "Court", wwiseGameObjectFootstep); // Running step sounds
                result = AkSoundEngine.SetSwitch("Court_Marche", "Court", wwiseGameObjectBreath); // Running step sounds
            }

            if (!isDamaged)
            {
                inputs.Player.Walk.Enable();
            }

            anim.SetBool("isProtectingEyes", isProtectingEyes);
            anim.SetBool("isProtectingEars", isProtectingEars);
            anim.SetBool("isDamaged", isDamaged);
        }
        else // World-shelter transition
        {
            anim.SetBool("isWalking", false); // stop animation when warping
            AKRESULT result;
            result = AkSoundEngine.SetSwitch("Court_Marche", "Idle", wwiseGameObjectFootstep); // Idle step sounds
            result = AkSoundEngine.SetSwitch("Court_Marche", "Idle", wwiseGameObjectBreath); // Idle step sounds
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
            movement += translation * speed * Time.deltaTime;
            steeringAngle += v.x * rotationSpeed * Time.deltaTime;
            characterController.transform.rotation = Quaternion.Euler(0, steeringAngle, 0);
            
            if (v.y < turnBackThreshold) { StartCoroutine("TurnBack"); }

            if (v.magnitude < Mathf.Epsilon)
            {
                isRunning = false;
                anim.SetBool("isWalking", false);
                AKRESULT result;
                result = AkSoundEngine.SetSwitch("Court_Marche", "Idle", wwiseGameObjectFootstep); // Idle step sounds
                result = AkSoundEngine.SetSwitch("Court_Marche", "Idle", wwiseGameObjectBreath); // Idle step sounds
            } else
            {
                isRunning = true;
                anim.SetBool("isWalking", true);
                AKRESULT result;
                result = AkSoundEngine.SetSwitch("Court_Marche", "Marche", wwiseGameObjectFootstep); // Walking step sounds
                result = AkSoundEngine.SetSwitch("Court_Marche", "Marche", wwiseGameObjectBreath); // Walking step sounds
            }
        }
    }

    public void ResetTransform(Vector3 position, float angle)
    {
        transform.position = position;
        StickToGround();
        transform.position += movement;
        movement = Vector3.zero;
        steeringAngle = angle;
        characterController.transform.rotation = Quaternion.Euler(0, steeringAngle, 0);
    }

    void StickToGround()
    {
        if (raycastPosition && groundLevelPosition)
        {
            LayerMask ground = LayerMask.GetMask("AsphaltGround") | LayerMask.GetMask("GrassGround") | LayerMask.GetMask("ConcreteGround") | LayerMask.GetMask("SoilGround");

            if (Physics.Raycast(raycastPosition.position, -Vector3.up, out RaycastHit hit, Mathf.Infinity, ground))
            {
                movement = new Vector3(0, (hit.point - groundLevelPosition.position).y, 0);
            }
            else
            {
                movement = Vector3.zero;
            }
        }
    }


    IEnumerator TurnBack()
    {
        isTurningBack = true;

        AKRESULT result;
        result = AkSoundEngine.SetSwitch("Court_Marche", "Marche", wwiseGameObjectFootstep);
        result = AkSoundEngine.SetSwitch("Court_Marche", "Marche", wwiseGameObjectBreath);

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
        steeringAngle -= 180; // synchronize with update
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
