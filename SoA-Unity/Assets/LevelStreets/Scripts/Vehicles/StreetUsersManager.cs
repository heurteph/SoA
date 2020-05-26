using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class StreetUsersManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] streetUsers;
    public GameObject[] StreetUsers { get { return streetUsers; } set { streetUsers = value; } }

    private List<GameObject> availableUsers;

    // Start is called before the first frame update
    void Start()
    {
        // Each user register to the event
        foreach(GameObject streetUser in streetUsers)
        {
            if(streetUser.GetComponent<StreetUser>() == null)
            {
                throw new System.Exception(streetUser.name + " doesn't contain a StreetUser script");
            }
            streetUser.GetComponent<StreetUser>().AvailableEvent += PushCar;
        }
        availableUsers = new List<GameObject>(streetUsers); // copy constructor
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PushCar(GameObject car)
    {
        // TO DO : Make sure it's not already in there before adding it
        availableUsers.Add(car);
    }

    public GameObject PopCar()
    {
        if (availableUsers.Count < 1)
        {
            throw new System.Exception("No available car to supply to this request");
        }
        int randomIndex = Random.Range(0, availableUsers.Count);
        GameObject car = availableUsers[randomIndex];
        availableUsers.RemoveAt(randomIndex);
        return car;
    }
}
