using UnityEngine;

public class ExplodingProjectile : MonoBehaviour
{
    [Header("Assingables")]
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Rigidbody thisRigidbody;
    [SerializeField] private GameObject particleKillGroup;
    [SerializeField] private Transform missileTarget;

    private Collider thisCollider;

    [Header("Parameters")]
    [SerializeField] private float thrust;
    [SerializeField] private bool LookRotation = true;
    [SerializeField] private bool Missile = false;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileSpeedMultiplier;
    [SerializeField] private bool ignorePrevRotation = false;
    [SerializeField] private bool explodeOnTimer = false;
    [SerializeField] private float explosionTimer;
    [SerializeField] private float targetForce = 500f;

    float timer;
    private Vector3 previousPosition;

    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        if (Missile)
        {
            missileTarget = GameObject.FindWithTag("Target").transform;
        }
        thisCollider = GetComponent<Collider>();
        previousPosition = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= explosionTimer && explodeOnTimer == true)
        {
            Explode();
        }
    }

    void FixedUpdate()
    {
        if (Missile)
        {
            projectileSpeed += projectileSpeed * projectileSpeedMultiplier;

            transform.LookAt(missileTarget);

            thisRigidbody.AddForce(transform.forward * projectileSpeed);
        }

        if (LookRotation && timer >= 0.05f)
        {
            transform.rotation = Quaternion.LookRotation(thisRigidbody.velocity);
        }

        CheckCollision(previousPosition);

        previousPosition = transform.position;
    }

    void CheckCollision(Vector3 prevPos)
    {
        RaycastHit hit;
        Vector3 direction = transform.position - prevPos;
        Ray ray = new Ray(prevPos, direction);
        float dist = Vector3.Distance(transform.position, prevPos);
        if (Physics.Raycast(ray, out hit, dist))
        {
            transform.position = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            Vector3 pos = hit.point;
            GameObject impact = Instantiate(impactPrefab, pos, rot);
            impact.transform.SetParent(hit.collider.gameObject.transform); 
            if (!explodeOnTimer && Missile == false)
            {
                if (hit.collider.gameObject.GetComponent<Rigidbody>()) hit.collider.gameObject.GetComponent<Rigidbody>().AddForce(thisRigidbody.velocity.normalized * targetForce, ForceMode.Force);
                Destroy(gameObject);
            }
            else if (Missile == true)
            {
                thisCollider.enabled = false;
                particleKillGroup.SetActive(false);
                thisRigidbody.velocity = Vector3.zero;
                Destroy(gameObject, 5);
            }

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "FX")
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
            if (ignorePrevRotation)
            {
                rot = Quaternion.Euler(0, 0, 0);
            }
            Vector3 pos = contact.point;
            GameObject impact = Instantiate(impactPrefab, pos, rot);
            impact.transform.SetParent(collision.gameObject.transform);
            if (!explodeOnTimer && Missile == false)
            {
                Destroy(gameObject);
            }
            else if (Missile == true)
            {

                thisCollider.enabled = false;
                particleKillGroup.SetActive(false);
                thisRigidbody.velocity = Vector3.zero;

                Destroy(gameObject, 5);

            }
        }
    }

    void Explode()
    {
        Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
        Destroy(gameObject);
    }

}