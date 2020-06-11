using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pixelplacement;

public enum CLIENTSTATE { WALKING, FREEZE, SHOPPING }

public class Client : MonoBehaviour
{
    private Spline spline;
    private float percentage;

    private float speed;
    private float minShoppingDuration;
    private float maxShoppingDuration;

    [Header("Ground")]
    [Space]

    [SerializeField]
    [Tooltip("The raycaster")]
    private Transform raycaster;

    [SerializeField]
    [Tooltip("The ground level")]
    private Transform groundLevel;
    private float groundOffset;

    private CLIENTSTATE clientState;
    public CLIENTSTATE ClientState { get { return clientState; } set { clientState = value; } }

    private void Awake()
    {
        groundOffset = transform.position.y - groundLevel.transform.position.y;
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (clientState == CLIENTSTATE.WALKING)
        {
            /* UPDATE PERCENTAGE */

            percentage = Mathf.Min(percentage + (speed * Time.deltaTime) / spline.Length, 1);

            /* UPDATE TRANSFORM */

            Vector3 position = spline.GetPosition(percentage);
            transform.position = StickToTheGround(position);
            transform.rotation = Quaternion.LookRotation(spline.GetDirection(percentage));

            /* SHOPPING CONDITION */

            if (percentage == 1)
            {
                GetComponent<Animator>().SetBool("IsWalking", false);
                ClientState = CLIENTSTATE.SHOPPING;
                StartCoroutine("GoShopping");
            }
        }
    }

    private IEnumerator GoShopping()
    {
        yield return new WaitForSeconds(Random.Range(minShoppingDuration, maxShoppingDuration));

        // Reset walking
        percentage = 0;
        ClientState = CLIENTSTATE.WALKING;
        GetComponent<Animator>().SetBool("IsWalking", true);
    }

    public void SetConstants(float speed, float minShoppingDuration, float maxShoppingDuration)
    {
        this.speed = speed;
        this.minShoppingDuration = minShoppingDuration;
        this.maxShoppingDuration = maxShoppingDuration;
    }

    public void Initialize(Spline startSpline, float startPercentage, CLIENTSTATE startState)
    {
        spline = startSpline;
        clientState = startState;
        percentage = startPercentage;
        GetComponent<Animator>().SetBool("IsWalking", clientState == CLIENTSTATE.WALKING);
    }

    /* PHYSICS RELATED FUNCTIONS */

    private Vector3 StickToTheGround(Vector3 position)
    {
        LayerMask mask = LayerMask.GetMask("AsphaltGround") | LayerMask.GetMask("GrassGround") | LayerMask.GetMask("SoilGround");
        if (Physics.Raycast(raycaster.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, mask))
        {
            return new Vector3(position.x, hit.point.y + groundOffset, position.z);
        }
        return position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Shop"))
        {
            // Get the next shop to go to
            spline = other.GetComponent<Shop>().GetExitPath();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (clientState != CLIENTSTATE.FREEZE)
            {
                clientState = CLIENTSTATE.FREEZE;
                speed = 0; // TO DO : Find a progressive way
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            clientState = CLIENTSTATE.WALKING;
        }
    }
}
