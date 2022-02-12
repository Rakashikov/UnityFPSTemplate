using UnityEngine;

[System.Serializable]
public class GrabObjectProperties{
	
	public bool useGravity = false;
	public float drag = 10;
	public float angularDrag = 10;
	public RigidbodyConstraints constraints = RigidbodyConstraints.FreezeRotation;		

}

public class Player_Grab : MonoBehaviour {

	[Header("Assingables")]
	[SerializeField] private Camera fpsCam;
	[SerializeField] private Collider playerCollider;

	[Header("Grab properties")]

	[SerializeField]
	[Range(0,50)]
	private float grabSpeed = 7;

	[SerializeField]
	[Range(1 ,10)]
	private float grabDistance = 1;

	[SerializeField]
	[Range(2 ,5)]
	private float targetDistance = 10;

	[SerializeField]
	[Range(10,50)]
	private float impulseMagnitude = 25;

	[SerializeField]
	private float breakDistance = 20;

	[SerializeField]
	private bool ignoreCollision = true;


	

	[Header("Affected Rigidbody Properties")]
	[SerializeField] GrabObjectProperties grabProperties = new GrabObjectProperties();	

	GrabObjectProperties defaultProperties = new GrabObjectProperties();

	[Header("Layers")]
	[SerializeField]
	LayerMask collisionMask;

	

	Rigidbody targetRB = null;
	Transform cameraTransform;	

	Vector3 targetPos;
	GameObject hitPointObject;
	GameObject ArtiBodyObject;
	ArticulationBody ArtyBodyObjComponent;
	HingeJoint objHingeJoint;
	float calcTargetDistance;

	bool grabbing = false;
	bool applyImpulse = false;
	bool isHingeJoint = false;

	//Debug
	LineRenderer lineRenderer;

	private void Awake()
	{
		cameraTransform = fpsCam.transform;
		hitPointObject = new GameObject("Point");
		lineRenderer = GetComponent<LineRenderer>();
	}


	private void Update()
	{
		myInput();
		RenderLine();
	}

	private void RenderLine()
    {
		Vector3 hitPointPos = hitPointObject.transform.position;

		if (lineRenderer != null)
        {
            if (grabbing)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPositions(new Vector3[] { targetPos, hitPointPos });
            }
            else
            {
				lineRenderer.enabled = false;
			}
        }
    }

	private void myInput()
    {
		if (grabbing)
		{
			targetPos = cameraTransform.position + cameraTransform.forward * calcTargetDistance;

			if (Input.GetButtonDown("Grab"))
			{
				Reset();
				grabbing = false;
			}
			else if (Input.GetButtonDown("Fire1"))
			{
				applyImpulse = true;
			}


		}
		else
		{

			if (Input.GetButtonDown("Grab"))
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, grabDistance, collisionMask))
				{
					Rigidbody rb = hitInfo.collider.GetComponent<Rigidbody>();
					if (rb != null)
					{
						Set(rb, hitInfo.distance);
						grabbing = true;
					}
				}
			}
		}
	}

	private void Set(Rigidbody target, float distance)
	{	
		targetRB = target;

		ArtiBodyObject = new GameObject("ArtiBody");

		ArtiBodyObject.transform.SetParent(target.transform);
		ArtiBodyObject.transform.localPosition = Vector3.zero;
		ArtyBodyObjComponent = ArtiBodyObject.AddComponent<ArticulationBody>();

		objHingeJoint = target.gameObject.AddComponent<HingeJoint>();
        objHingeJoint.connectedArticulationBody = ArtyBodyObjComponent;
		objHingeJoint.connectedAnchor = Vector3.zero;
        objHingeJoint.anchor = Vector3.zero;

		//Debug.Break();

		isHingeJoint = target.GetComponent<HingeJoint>() != null;		

		//Rigidbody default properties	
		defaultProperties.useGravity = targetRB.useGravity;	
		defaultProperties.drag = targetRB.drag;
		defaultProperties.angularDrag = targetRB.angularDrag;
		defaultProperties.constraints = targetRB.constraints;

		//Grab Properties	
		targetRB.useGravity = grabProperties.useGravity;
		targetRB.drag = grabProperties.drag;
		targetRB.angularDrag = grabProperties.angularDrag;
		targetRB.constraints = isHingeJoint? RigidbodyConstraints.None : grabProperties.constraints;

		if(ignoreCollision)
		Physics.IgnoreCollision(targetRB.GetComponent<Collider>(), playerCollider);

		hitPointObject.transform.SetParent(target.transform);							

		calcTargetDistance = distance;
		targetPos = cameraTransform.position + cameraTransform.forward * calcTargetDistance;

		hitPointObject.transform.position = targetPos;
		hitPointObject.transform.LookAt(cameraTransform);
				
	}

	private void Reset()
	{
		
		//Grab Properties	
		targetRB.useGravity = defaultProperties.useGravity;
		targetRB.drag = defaultProperties.drag;
		targetRB.angularDrag = defaultProperties.angularDrag;
		targetRB.constraints = defaultProperties.constraints;

		if(ignoreCollision)
        Physics.IgnoreCollision(targetRB.GetComponent<Collider>(), playerCollider, false);


        targetRB = null;

		hitPointObject.transform.SetParent(null);
		ArtiBodyObject.transform.SetParent(null);

		Destroy(objHingeJoint);
		Destroy(ArtyBodyObjComponent);
		Destroy(ArtiBodyObject);

		//if(lineRenderer != null)
		//	lineRenderer.enabled = false;
	}

	private void Grab()
	{
		Vector3 hitPointPos = hitPointObject.transform.position;
		//Vector3 dif = targetPos - hitPointPos;
		Vector3 dif = (cameraTransform.position + cameraTransform.forward * targetDistance) - hitPointPos;

		if (isHingeJoint)
		{
			targetRB.AddForceAtPosition(grabSpeed * dif * 100, hitPointPos, ForceMode.Force);
			targetRB.AddForce(Vector3.down * 100 * targetRB.mass);
		}
		else
		{
			targetRB.velocity += grabSpeed * dif * (Vector3.Distance(targetPos, targetRB.transform.position) / targetRB.mass);
			targetRB.AddForce(Vector3.down * 100 * targetRB.mass);
		}

		//if (lineRenderer != null){
		//	lineRenderer.enabled = true;
		//	lineRenderer.SetPositions( new Vector3[]{ targetPos , hitPointPos });
		//}

		if (Vector3.Distance(targetRB.transform.position, targetPos) > breakDistance)
		{
			targetRB.velocity = Vector3.zero;
			Reset();
			grabbing = false;
		}
	}



	private void FixedUpdate()
	{
		if(!grabbing)
			return;

        Grab();

        if (applyImpulse){
			targetRB.velocity += cameraTransform.forward * impulseMagnitude / targetRB.mass;
			Reset();
			grabbing = false;
			applyImpulse = false;
		}
		
	}

}
