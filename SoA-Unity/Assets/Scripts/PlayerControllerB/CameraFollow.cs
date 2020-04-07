using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField]
    private Inputs inputs;

    [Space]
    [Header("Camera Settings")]
    [Space]

    [SerializeField]
    [Tooltip("The character followed by the camera")]
    private GameObject player;

    [Space]

    [SerializeField]
    [Tooltip("Camera's translation offset from the player's position")]
    private Vector3 cameraOffset = new Vector3(0, 1, -10);

    [SerializeField]
    [Tooltip("Camera's angular offset from the player's orientation")]
    private Vector3 cameraAngularOffset = Vector3.zero;

    [Space]

    [SerializeField]
    [Tooltip("Speed at which the camera align itself to the character")]
    [Range(10,500)]
    private float alignSpeed = 100;

    [Space]

    [SerializeField]
    private GameObject arm;

    private float originalYRotation;

    private Vector3 lastPlayerPosition;

    private Vector2 readInputValue = Vector2.zero;

    private Vector2 accumulator = Vector2.zero;

    [SerializeField]
    private float lookAroundSpeed = 5;

    [SerializeField]
    private float maxLookAroundAngle = 45; // degrees

    private void Awake()
    {
       inputs = new Inputs();
    }

    // Start is called before the first frame update
    void Start()
    {
       originalYRotation  = transform.rotation.eulerAngles.y;
       transform.position = player.transform.position + player.transform.rotation * cameraOffset;
       transform.LookAt(player.transform);
       lastPlayerPosition = player.transform.position;
       StartCoroutine("AlignWithCharacter");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdatePosition();

        UpdateRotation();

        readInputValue = inputs.Player.LookAround.ReadValue<Vector2>();

        LookAround(inputs.Player.LookAround.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
    }

    private void OnDisable()
    {
        inputs.Player.Disable();
    }

    private void UpdatePosition ()
    {
        //transform.position = player.transform.position + player.transform.forward * cameraOffset.z + transform.up * cameraOffset.y + transform.right * cameraOffset.x;
        //transform.rotation = player.transform.rotation;
        //transform.rotation *= Quaternion.Euler(cameraAngularOffset.x, cameraAngularOffset.y, cameraAngularOffset.z);
        //transform.position = player.transform.position + transform.rotation * cameraOffset;

        transform.position += (player.transform.position - lastPlayerPosition);
        lastPlayerPosition = player.transform.position;
    }

    private void UpdateRotation ()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((player.transform.position - transform.position), Vector3.up)); // kind of a lookAt but without the x-rotation
    }

    private IEnumerator AlignWithCharacter()
    {
        for(; ;)
        {
            if (/*readInputValue != null && Mathf.Approximately(readInputValue.x,0) && Mathf.Approximately(readInputValue.y,0)*/ true)
            {
                float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, player.transform.forward, Vector3.up) % 360;

                float smooth = 0.95f * -Mathf.Pow((Mathf.Abs(angle) / 180f - 1), 2) + 1;  // [0.05-1]

                if (Mathf.Abs(angle) >= alignSpeed * smooth * Time.deltaTime)
                {
                    transform.RotateAround(player.transform.position, Vector3.up, Mathf.Sign(angle) * alignSpeed * smooth * Time.deltaTime);
                    transform.rotation *= Quaternion.Euler(0, originalYRotation, 0);
                }
                else if (!Mathf.Approximately(angle, 0))
                {
                    transform.RotateAround(player.transform.position, Vector3.up, angle);
                    transform.rotation *= Quaternion.Euler(0, originalYRotation, 0);
                }
            }
            yield return null;
        }
    }

    void LookAround(Vector2 v)
    {
        if (!(Mathf.Approximately(v.x, 0) && Mathf.Approximately(v.y, 0)))
        {
            accumulator += v * lookAroundSpeed * Time.deltaTime;
            accumulator.x = Mathf.Clamp(accumulator.x, -maxLookAroundAngle, maxLookAroundAngle);
            accumulator.y = Mathf.Clamp(accumulator.y, -maxLookAroundAngle, maxLookAroundAngle);
            arm.transform.localRotation = Quaternion.Euler(-accumulator.y, accumulator.x, 0);

            //arm.transform.rotation *= Quaternion.Euler(90 * -v.y, 60 * v.x, 0); // TO DO : make relative and time-based
        }
        else
        {
            accumulator.x = (1-Mathf.Sign(accumulator.x))/2f * Mathf.Min(accumulator.x - Mathf.Sign(accumulator.x) * lookAroundSpeed * Time.deltaTime, 0) + (1 + Mathf.Sign(accumulator.x)) / 2f * Mathf.Max(accumulator.x - Mathf.Sign(accumulator.x) * lookAroundSpeed * Time.deltaTime, 0);
            accumulator.y = (1 - Mathf.Sign(accumulator.y)) / 2f * Mathf.Min(accumulator.y - Mathf.Sign(accumulator.y) * lookAroundSpeed * Time.deltaTime, 0) + (1 + Mathf.Sign(accumulator.y)) / 2f * Mathf.Max(accumulator.y - Mathf.Sign(accumulator.y) * lookAroundSpeed * Time.deltaTime, 0);
            arm.transform.localRotation = Quaternion.Euler(-accumulator.y, accumulator.x, 0);
        }
    }

} //FINISH
