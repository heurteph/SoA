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

    private float originalYRotation;

    private Vector3 lastPlayerPosition;

    private Vector2 readInputValue = Vector2.zero;

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
        UpdateCamera();
        
        transform.LookAt(player.transform);

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

    void LookAround(Vector2 v)
    {
        transform.rotation *= Quaternion.Euler( 90 * v.y, 60 * v.x, 0); // add time-based
    }

    private void UpdateCamera ()
    {
        //transform.position = player.transform.position + player.transform.forward * cameraOffset.z + transform.up * cameraOffset.y + transform.right * cameraOffset.x;
        //transform.rotation = player.transform.rotation;
        //transform.rotation *= Quaternion.Euler(cameraAngularOffset.x, cameraAngularOffset.y, cameraAngularOffset.z);
        //transform.position = player.transform.position + transform.rotation * cameraOffset;

        Debug.Log("Delta : " + (player.transform.position - lastPlayerPosition));
        transform.position += (player.transform.position - lastPlayerPosition);
        lastPlayerPosition = player.transform.position;
    }

    private IEnumerator AlignWithCharacter()
    {
        for(; ;)
        {
            if (readInputValue != null && Mathf.Approximately(readInputValue.x,0) && Mathf.Approximately(readInputValue.y,0) )
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


} //FINISH
