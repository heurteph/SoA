using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    private GameObject singleton;

    [SerializeField]
    [Tooltip("Reference to the game manager")]
    private GameObject gameManager;

    private bool done;

    private void Awake()
    {
        // Must exist between scenes to achieve transitions
        if (singleton == null)
        {
            singleton = gameObject;
            DontDestroyOnLoad(transform.parent.gameObject); // save all canvas
        }
        else if (singleton != gameObject)
        {
            Destroy(transform.parent.gameObject); // destroy all canvas
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gameManager == null)
        {
            throw new System.NullReferenceException("Missing reference to the game manager");
        }
        done = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerCredits()
    {
        if (!done)
        {
            Debug.Log("Fin de l'anim de fade out avant les crédits");
            gameManager.GetComponent<GameManager>().DisplayCredits();
            done = true;
        }
    }
}
