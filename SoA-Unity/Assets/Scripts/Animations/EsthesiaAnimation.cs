using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsthesiaAnimation : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The reference to the player")]
    private GameObject player;

    [SerializeField]
    [Tooltip("Duration of the transition from idle to walk in seconds")]
    [Range(0.01f, 0.5f)]
    private float idleToWalkDuration = 0.2f; // s

    [SerializeField]
    [Tooltip("Duration of the transition from walk to idle in seconds")]
    [Range(0.01f,0.5f)]
    private float walkToIdleDuration = 0.05f; // s

    private void Awake()
    {
        InitializeAnimationLayers();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            throw new System.NullReferenceException("No reference to the player set in FinishAnimation script");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FinishedEyesDamageAnimation()
    {
        player.GetComponent<PlayerFirst>().IsDamagedEyes = false;
        //SelectAnimationLayer(0);

    }

    public void FinishedEarsDamageAnimation()
    {
        player.GetComponent<PlayerFirst>().IsDamagedEars = false;
        //SelectAnimationLayer(0);
    }

    /*
    public void SelectBaseLayer()
    {
        SelectAnimationLayer(0);
    }

    public void SelectEyesDamageLayer()
    {
        SelectAnimationLayer(1);
    }

    public void SelectEarsDamageLayer()
    {
        SelectAnimationLayer(2);
    }

    private void SelectAnimationLayer(int index)
    {
        GetComponent<Animator>().SetLayerWeight(0, index == 0 ? 1 : 0);
        GetComponent<Animator>().SetLayerWeight(1, index == 1 ? 1 : 0);
        GetComponent<Animator>().SetLayerWeight(2, index == 2 ? 1 : 0);
    }
    */

    public void InitializeAnimationLayers()
    {
        GetComponent<Animator>().SetLayerWeight(0, 1);
        GetComponent<Animator>().SetLayerWeight(1, 0);
    }

    public void SelectIdleLayer()
    {
        if (GetComponent<Animator>().GetLayerWeight(1) == 1)
        {
            StartCoroutine("WalkToIdleTransition");
        }
    }

    public void SelectWalkLayer()
    {
        if (GetComponent<Animator>().GetLayerWeight(0) == 1)
        {
            StartCoroutine("IdleToWalkTransition");
        }
    }

    private IEnumerator IdleToWalkTransition()
    {
        float weight = 0;
        while(weight < 1)
        {
            weight = Mathf.Min(weight + Time.deltaTime / idleToWalkDuration, 1f);
            GetComponent<Animator>().SetLayerWeight(0, 1 - weight);
            GetComponent<Animator>().SetLayerWeight(1, weight);
            yield return null;
        }
    }

    private IEnumerator WalkToIdleTransition()
    {
        float weight = 0;
        while (weight < 1)
        {
            weight = Mathf.Min(weight + Time.deltaTime / walkToIdleDuration, 1f);
            GetComponent<Animator>().SetLayerWeight(0, weight);
            GetComponent<Animator>().SetLayerWeight(1, 1 - weight);
            yield return null;
        }
    }
}
