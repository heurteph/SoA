using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FieldOfView : MonoBehaviour
{
	[SerializeField]
	private float viewRadius;
	
	[SerializeField]
	[Range(0, 360)]
	public float viewAngle;

	[SerializeField]
	public LayerMask targetMask;

	[SerializeField] 
	public LayerMask obstacleMask;

    private List<Transform> visibleTargets = new List<Transform>();

	[SerializeField]
	private float meshResolution;


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
		DrawCircularField();
		DrawFieldOfView();
	}

	void FindVisibleTargets()
		{
			visibleTargets.Clear();
			Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

			for (int i = 0; i < targetsInViewRadius.Length; i++)
			{
				Transform target = targetsInViewRadius[i].transform;
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
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



	void DrawFieldOfView()
	{


		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;

		for (int i=0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle (angle, true) * viewRadius, Color.yellow);
		}
	}

	void DrawCircularField()
	{


		int stepCount = Mathf.RoundToInt(360 * meshResolution);
		float stepAngleSize = 360 / stepCount;

		for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - 360 / 2 + stepAngleSize * i;
			Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * viewRadius, Color.white);
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
