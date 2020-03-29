using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIco : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Inputs inputs;


    private void Awake()
    {
       inputs = new Inputs();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
        transform.rotation = Quaternion.Euler( 90*v.y, 60*v.x, 0);
    }


} //FINISH
