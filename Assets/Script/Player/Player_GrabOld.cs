using UnityEngine;

public class Player_GrabObject : MonoBehaviour
{
    [Header("Assingables")]
    [SerializeField] private Transform holdParentCamera;
    [SerializeField] private Collider playerCollider;
    //[SerializeField] private Camera fpsCam;

    [Header("Grab Variables")]
    [SerializeField] private float pickUpRange = 5f;
    [SerializeField] private float distanceFromCamera = 10f;
    [SerializeField] private float moveForce = 250f;
    [SerializeField] private float dropForce = 250f;

    [Header("Bool Variables")]
    [SerializeField] private bool disableCollisions = true;
    [SerializeField] private bool disableGravity = true;
    [SerializeField] private bool disableRotation = true;

    private GameObject heldObj = null;
    private Vector3 movePosition;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Grab"))
            if (heldObj == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(holdParentCamera.transform.position,holdParentCamera.transform.forward, out hit, pickUpRange))
                {
                    PickupObject(hit.transform.gameObject);
                    Debug.Log("Grab");
                }
            }
            else
            {
                DropObject(0);
            }

        if (heldObj != null)
        {
            if (Vector3.Distance(heldObj.transform.position, holdParentCamera.transform.position + holdParentCamera.transform.forward * distanceFromCamera) > pickUpRange)
            {
                DropObject(0);
            }
            else
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    DropObject(dropForce);
                }
                else
                {
                    MoveObject();
                }
            }
        }
        

    }

    void MoveObject()
    {
        movePosition = holdParentCamera.position + holdParentCamera.transform.forward * distanceFromCamera;
        Vector3 moveDirection = (movePosition - heldObj.transform.position) * Mathf.Max(Vector3.Distance(heldObj.transform.position, movePosition),10f);
        if (heldObj.transform.position != movePosition)
            heldObj.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce * Time.deltaTime, ForceMode.Force);
        else
            heldObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    void PickupObject(GameObject pickObject)
    {
        if (pickObject.GetComponent<Rigidbody>())
        {
            Rigidbody objectRig = pickObject.GetComponent<Rigidbody>();
            if (disableCollisions)
                Physics.IgnoreCollision(playerCollider, pickObject.GetComponent<Collider>());
            if (disableGravity)
                objectRig.useGravity = false;
            if (disableRotation)
                objectRig.constraints = RigidbodyConstraints.FreezeRotation;
            objectRig.drag = 20;

            heldObj = pickObject;

            heldObj.gameObject.layer = LayerMask.NameToLayer("Object");

            //heldObj.transform.parent = fpsCam.transform;
        }
    }

    void DropObject(float force)
    {
        Rigidbody heldRig = heldObj.GetComponent<Rigidbody>();
        heldRig.drag = 1;

        heldRig.AddForce(holdParentCamera.transform.forward * force * Time.deltaTime);
        if(disableCollisions)
            Physics.IgnoreCollision(playerCollider, heldObj.GetComponent<Collider>(), false);
        if (disableGravity)
            heldRig.useGravity = true;
        if (disableRotation)
            heldRig.constraints = RigidbodyConstraints.None;

        //heldObj.transform.parent = null;
        heldObj.gameObject.layer = LayerMask.NameToLayer("Ground");
        heldObj = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(holdParentCamera.transform.position, holdParentCamera.transform.position + holdParentCamera.transform.forward * pickUpRange);
        if (heldObj != null)
            Gizmos.DrawLine(heldObj.transform.position, holdParentCamera.transform.position + holdParentCamera.transform.forward);
    }
}
