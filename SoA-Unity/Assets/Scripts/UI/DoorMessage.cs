using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorMessage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Display message
            // transform.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, 1);
            transform.GetChild(0).GetComponent<Animation>().Play("DoorTextFadeIn");

            //transform.rotation = Quaternion.Euler(0,180,0) * Quaternion.LookRotation(Vector3.ProjectOnPlane(other.transform.position - transform.position, Vector3.up));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Hide message
            // transform.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, 1);
            transform.GetChild(0).GetComponent<Animation>().Play("DoorTextFadeOut");
        }
    }
}
