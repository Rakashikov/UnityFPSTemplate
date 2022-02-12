using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_JumpPad : MonoBehaviour
{
    [SerializeField] private float boostForce = 1000f;

    private void OnCollisionEnter(Collision collision)
    {
        try
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * boostForce);
        }
        catch
        {
            Debug.LogError("Cant find Rigidbody for Jumppad");
        }
    }
}
