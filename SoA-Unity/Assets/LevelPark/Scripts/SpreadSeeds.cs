using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadSeeds : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The prefab of the flower")]
    private GameObject flowerPrefab;

    [SerializeField]
    [Tooltip("The number of seed")]
    private int seedsNumber;

    [SerializeField]
    [Tooltip("Arbitrary constant")]
    private int constant;

    // Start is called before the first frame update
    void Start()
    {
        float phi = 1.61803f; // golden ratio
        float theta, radius;
        Vector3 position;
        Quaternion rotation;
        for (int i = 0; i < seedsNumber; i++)
        {
            theta = i * 2 * Mathf.PI / (phi * phi);
            radius = constant * Mathf.Sqrt(i);
            position = new Vector3();
            position.y = transform.position.y;
            position.x = transform.position.x + radius * Mathf.Cos(theta);
            position.z = transform.position.z + radius * Mathf.Sin(theta);
            rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject flower = Object.Instantiate(flowerPrefab, position, rotation);
            flower.name = transform.name + " " + (i + 1).ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
