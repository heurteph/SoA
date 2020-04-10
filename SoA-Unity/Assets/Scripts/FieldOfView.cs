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

    private List<Transform> frontVisibleTargets = new List<Transform>();
	private List<Transform> sideVisibleTargets = new List<Transform>();
	private List<Transform> backVisibleTargets = new List<Transform>();


	[SerializeField]
	private EnergyBehaviour energyBehaviour;

	private delegate void CrowdThresholdHandler(float b);
	private event CrowdThresholdHandler crowdThresholdEvent;

	[SerializeField]
	[Range(0,100)]
	private int crowdFrontDamage;

	[SerializeField]
	[Range(0, 100)]
	private int crowdSideDamage;

	[SerializeField]
	[Range(0, 100)]
	private int crowdBackDamage;


	// Start is called before the first frame update
	void Start()
	{
	    StartCoroutine("FindFrontTargetsWithDelay", .2f);

		StartCoroutine("FindSideTargetsWithDelay", .2f);

		StartCoroutine("FindBackTargetsWithDelay", .2f);


		obstacleMask = ~targetMask;

		crowdThresholdEvent += energyBehaviour.DecreaseEnergy;
	}


	// Update is called once per frame
	void Update()
	{
		//Back View
		//	DrawTotalFieldOfBackView();
		DrawFieldOfBackView();

		//Side View
		//	DrawTotalFieldOfSideView();
		DrawFieldOfSideView();

		//Front View
		//	DrawTotalFieldOfFrontView();
		DrawFieldOfFrontView();
	}

	IEnumerator FindFrontTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindFrontVisibleTargets();
		}
	}

	//Adapt to every Views !!!!
	void FindFrontVisibleTargets()
		{
			frontVisibleTargets.Clear();
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
						frontVisibleTargets.Add(target);
					Debug.DrawLine(transform.position, target.position, Color.red);


				    crowdThresholdEvent(crowdFrontDamage);
				}
			}
			}
		}



	IEnumerator FindSideTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindSideVisibleTargets();
		}
	}

	//Adapt to every Views !!!!
	void FindSideVisibleTargets()
	{
		sideVisibleTargets.Clear();
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, sideViewRadius, targetMask);

		for (int i = 0; i < targetsInViewRadius.Length; i++)
		{
			Transform target = targetsInViewRadius[i].transform;
			Vector3 dirToTarget = (target.position - transform.position).normalized;

			if (Vector3.Angle(transform.forward, dirToTarget) < sideViewAngle / 2 
				&& Vector3.Angle(transform.forward, dirToTarget) > frontViewAngle / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.position);

				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
				{
					sideVisibleTargets.Add(target);
					Debug.DrawLine(transform.position, target.position, Color.blue);


					crowdThresholdEvent(crowdSideDamage);
				}
			}
		}
	}




	IEnumerator FindBackTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindBackVisibleTargets();
		}
	}

	//Adapt to every Views !!!!
	void FindBackVisibleTargets()
	{
		backVisibleTargets.Clear();
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, backViewRadius, targetMask);

		for (int i = 0; i < targetsInViewRadius.Length; i++)
		{
			Transform target = targetsInViewRadius[i].transform;
			Vector3 dirToTarget = (target.position - transform.position).normalized;

			if (Vector3.Angle(transform.forward, dirToTarget) < backViewAngle / 2
				&& Vector3.Angle(transform.forward, dirToTarget) > sideViewAngle / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.position);

				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
				{
					backVisibleTargets.Add(target);
					Debug.DrawLine(transform.position, target.position, Color.green);


					crowdThresholdEvent(crowdBackDamage);
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
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * sideViewRadius, Color.white);
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
		float stepAngleSize =  backViewAngle / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - backViewAngle / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * backViewRadius, Color.gray);
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
