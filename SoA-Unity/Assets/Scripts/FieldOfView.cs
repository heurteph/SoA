using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FieldOfView : MonoBehaviour
{
	[SerializeField]
	private float frontViewRadius;
	
	[SerializeField]
	[Range(0, 360)]
	public float frontViewAngle;

	[SerializeField]
	private float frontMeshResolution;

	[SerializeField]
	private float sideViewRadius;

	[SerializeField]
	[Range(0, 360)]
	public float sideViewAngle;

	[SerializeField]
	private float sideMeshResolution;

	[SerializeField]
	private float backViewRadius;

	[SerializeField]
	[Range(0, 360)]
	public float backViewAngle;

	[SerializeField]
	private float backMeshResolution;



	[SerializeField]
	public LayerMask targetMask;

	[SerializeField] 
	public LayerMask obstacleMask;

    private List<Transform> visibleTargets = new List<Transform>();


	[SerializeField]
	private EnergyBehaviour energyBehaviour;

	private delegate void CrowdThresholdHandler(float b);
	private event CrowdThresholdHandler crowdThresholdEvent;

	[SerializeField]
	[Range(0,100)]
	private int crowdDamage;

	void Start()
	{
	    StartCoroutine("FindTargetsWithDelay", .2f);
        obstacleMask = ~targetMask;
		crowdThresholdEvent += energyBehaviour.DecreaseEnergy;
	}


	IEnumerator FindTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindVisibleTargets();
		}
	}


	void Update()
	{
		//Front View
		DrawTotalFieldOfFrontView();
		DrawFieldOfFrontView();

		//Side View
		DrawTotalFieldOfSideView();
		DrawFieldOfSideView();

		//Back View
		DrawTotalFieldOfBackView();
		DrawFieldOfBackView();
	}

	//Adapt to every Views !!!!
	void FindVisibleTargets()
		{
			visibleTargets.Clear();
			Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, frontViewRadius, targetMask);

			for (int i = 0; i < targetsInViewRadius.Length; i++)
			{
				Transform target = targetsInViewRadius[i].transform;
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				if (Vector3.Angle(transform.forward, dirToTarget) < frontViewAngle / 2)
				{
					float dstToTarget = Vector3.Distance(transform.position, target.position);

					if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
					{
						visibleTargets.Add(target);
					Debug.DrawLine(transform.position, target.position, Color.red);


				    crowdThresholdEvent(crowdDamage);
				}
			}
			}
		}


	//FrontView
	void DrawTotalFieldOfFrontView()
	{


		int stepCount = Mathf.RoundToInt(360 * frontMeshResolution);
		float stepAngleSize = 360 / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - 360 / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * frontViewRadius, Color.white);
		}
	}
	void DrawFieldOfFrontView()
	{


		int stepCount = Mathf.RoundToInt(frontViewAngle * frontMeshResolution);
		float stepAngleSize = frontViewAngle / stepCount;

		for (int i=0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - frontViewAngle / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle (angle, true) * frontViewRadius, Color.yellow);
		}
	}


	//SideView
	void DrawTotalFieldOfSideView()
	{


		int stepCount = Mathf.RoundToInt(360 * sideMeshResolution);
		float stepAngleSize = 360 / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - 360 / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * sideViewRadius, Color.white);
		}
	}
	void DrawFieldOfSideView()
	{


		int stepCount = Mathf.RoundToInt(sideViewAngle * sideMeshResolution);
		float stepAngleSize = sideViewAngle / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - sideViewAngle / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * sideViewRadius, Color.yellow);
		}
	}


	//BackView
	void DrawTotalFieldOfBackView()
	{


		int stepCount = Mathf.RoundToInt(360 * backMeshResolution);
		float stepAngleSize = 360 / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - 360 / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * backViewRadius, Color.white);
		}
	}
	void DrawFieldOfBackView()
	{


		int stepCount = Mathf.RoundToInt(backViewAngle * backMeshResolution);
		float stepAngleSize = backViewAngle / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - backViewAngle / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * backViewRadius, Color.yellow);
		}
	}




	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
		{
			if (!angleIsGlobal)
			{
				angleInDegrees += transform.eulerAngles.y;
			}
			return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		}
	} // FINISH
