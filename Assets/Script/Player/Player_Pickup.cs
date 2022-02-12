using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Pickup : MonoBehaviour
{
    [Header("Assingables")]
    [SerializeField] private GameObject gunSocket;
    [SerializeField] private GameObject playerHands;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private string afterPickupLayer = "Weapon";
    [SerializeField] private string objTag = "Equipable";


    [Header("Parameters")]
    [SerializeField] private float pickupDistance;
    [SerializeField] private float pickupSpeed;
    [SerializeField] private float dropUpForce;
    [SerializeField] private float dropForwardForce;

    private GameObject target;

    private Transform objParent;
    private LayerMask objLayer;

    private bool objUseGravity;
    private bool objIsKinematic;
    private RigidbodyInterpolation objInterpolation;


    private void Update()
    {
        myInput();
    }

    private void myInput()
    {
        if (Input.GetButtonDown("Grab") && target == null)
        {
            Ray ray = new Ray(fpsCam.transform.position, fpsCam.transform.forward * pickupDistance);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, pickupDistance);
            if (Physics.Raycast(ray, out hit, pickupDistance) && hit.collider.gameObject.CompareTag(objTag))
            {
                Rigidbody objRb = hit.collider.GetComponent<Rigidbody>();
                if (objRb != null)
                {
                    target = objRb.gameObject;
                    Pickup(objRb);
                }
            }
        }
        if (Input.GetButtonDown("Drop") && target != null)
        {
            Drop(target.GetComponent<Rigidbody>());
        }
    }

    private void Pickup(Rigidbody targetRb)
    {
        playerHands.SetActive(false);

        objParent = target.transform.parent;
        objLayer = target.layer;

        objUseGravity = targetRb.useGravity;
        objIsKinematic = targetRb.isKinematic;
        objInterpolation = targetRb.interpolation;

        target.transform.SetParent(gunSocket.transform);
        StartCoroutine("TransformTransition");
        StartCoroutine("SetScripts", true);
        target.GetComponent<Collider>().enabled = false;
        SetLayerRecursively(target, LayerMask.NameToLayer(afterPickupLayer));
        targetRb.useGravity = false;
        targetRb.isKinematic = true;
        targetRb.interpolation = RigidbodyInterpolation.None;
    }

    private void Drop(Rigidbody targetRb)
    {
        playerHands.SetActive(true);

        target.transform.SetParent(objParent);
        SetLayerRecursively(target, objLayer);

        targetRb.useGravity = objUseGravity;
        targetRb.isKinematic = objIsKinematic;
        targetRb.interpolation = objInterpolation;

        target.GetComponent<Collider>().enabled = true;

        StartCoroutine("SetScripts", false);

        targetRb.velocity = GetComponent<Rigidbody>().velocity;
        targetRb.AddTorque(new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)),ForceMode.Force);
        targetRb.AddForce((fpsCam.transform.forward * dropForwardForce / Mathf.Max(1,(GetComponent<Rigidbody>().velocity.magnitude))) + (Vector3.up * dropUpForce), ForceMode.Force);

        target = null;
    }

    private void SetLayerRecursively(GameObject go, int layerNumber)
    {
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }

    IEnumerator TransformTransition()
    {
        for (float t = 0.01f; t < pickupSpeed; t += 2 * Time.deltaTime)
        {
            if (target == null) break;
            target.transform.localPosition = Vector3.Lerp(target.transform.localPosition, Vector3.zero, (t/pickupSpeed));
            target.transform.localRotation = Quaternion.Lerp(target.transform.localRotation, Quaternion.identity, (t/pickupSpeed));
            yield return null;
        }
    }

    IEnumerator SetScripts(bool isPickup)
    {
        if (isPickup)
        {
            yield return new WaitForSeconds(pickupSpeed / 10f);
            if (target == null) yield break;
            target.transform.localPosition = Vector3.zero;
            StopCoroutine("TransformTransition");
            if (target.GetComponent<Gun_projectile>()) { target.GetComponent<Gun_projectile>().enabled = true; }
            if (target.GetComponent<Gun_sway>()) target.GetComponent<Gun_sway>().enabled = true;
        }
        else
        {
            if (target.GetComponent<Gun_projectile>()) { target.GetComponent<Gun_projectile>().enabled = false; }
            if (target.GetComponent<Gun_sway>()) target.GetComponent<Gun_sway>().enabled = false;
        }
    }


    private void OnDrawGizmos()
    {
        Ray ray = new Ray(fpsCam.transform.position, fpsCam.transform.forward * pickupDistance);
        Gizmos.DrawRay(ray);
    }
}
