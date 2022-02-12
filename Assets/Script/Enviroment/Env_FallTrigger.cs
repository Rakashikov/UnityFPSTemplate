using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_FallTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position = new Vector3(0, 10, 0);
    }
}
