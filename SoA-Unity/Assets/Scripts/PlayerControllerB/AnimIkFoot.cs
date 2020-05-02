using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimIkFoot : MonoBehaviour
{

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject ground;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    [Range(0, 1)]
    private float distanceToGround; 

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private float skinWidth = 0.2f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            RaycastHit hit;

            //Left Foot
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

            // Left Foot Up
            Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, layerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;

                    anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);

                    Vector3 IKforward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(IKforward, hit.normal));

                }
            }

            //Stand Still
            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;

                    Vector3 prevPos = ground.transform.position;
                    prevPos.y = footPosition.y - skinWidth;
                    ground.transform.position = prevPos;
                }
            }


            // Right Foot
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            //Right Foot Up
            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, layerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                   footPosition.y += distanceToGround;

                    anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);

                    Vector3 IKforward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(IKforward, hit.normal));

                    
                }
            }


        }
    }

} //FINISH
