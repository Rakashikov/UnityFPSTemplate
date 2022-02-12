using UnityEngine;
using TMPro;

public class Gun_projectile : MonoBehaviour
{
    [Header("Assingables")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private Transform attackPoint;

    private Animator animController;

    [Header("Bullet force")]
    [SerializeField] private float shootForce;
    [SerializeField] private float upwardForce;

    [Header("Gun stats")]
    [SerializeField] private float timeBetweenShooting;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private float spread;
    [SerializeField] private float reloadTime;
    [SerializeField] private int magazineSize;
    [SerializeField] private int bulletsPerTap;
    [SerializeField] private float kickback;
    [SerializeField] private float recoil;
    [SerializeField] private bool allowButtonHold;
    

    [Header("Particles")]
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private TextMeshProUGUI ammunitionDisplay;

    //private variables
    int bulletsLeft;
    int bulletsShot;

    bool shooting;
    bool readyToShoot;
    bool reloading;

    Vector3 originPosition;
    Quaternion originRotation;

    bool allowInvoke = true;

    private void Start()
    {
        if (GetComponent<Animator>()) animController = GetComponent<Animator>();
        bulletsLeft = magazineSize;
        readyToShoot = true;
        originPosition = transform.localPosition;
        originRotation = transform.localRotation;
    }

    private void Update()
    {
        MyInput();
        ReturnTransform();
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + "/" + magazineSize / bulletsPerTap);
    }

    private void ReturnTransform()
    {
        if (transform.localPosition != originPosition)
            transform.localPosition = Vector3.Lerp(transform.localPosition, originPosition, Time.deltaTime*5);
        if (transform.localRotation != originRotation)
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originRotation, Time.deltaTime*5);
    }

    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");

        if (Input.GetButtonDown("Reload") && bulletsLeft < magazineSize && !reloading) Reload();
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shot();
        }
    }

    private void Shot()
    {
        readyToShoot = false;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit) && hit.distance > 5f)
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        //Vector3 directionWithoutSpread = targetPoint - attackPoint.position;
        Vector3 directionWithoutSpread = targetPoint - fpsCam.transform.position + (fpsCam.transform.forward / 2);
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        //GameObject currentBullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
        GameObject currentBullet = Instantiate(bulletPrefab, fpsCam.transform.position + (fpsCam.transform.forward / 2), Quaternion.identity);
        currentBullet.transform.forward = directionWithSpread.normalized;

        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);

        currentBullet.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity;

        transform.position -= transform.forward * kickback;
        transform.Rotate(0, 0, -recoil);

        bulletsLeft--;
        bulletsShot++;

        if (muzzleFlash != null)
            muzzleFlash.GetComponent<ParticleSystem>().Play();

        if (animController != null)
            animController.SetBool("isShot", true);

        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
        if (animController != null)
            animController.SetBool("isShot", false);
    }

    private void Reload()
    {
        reloading = true;
        if (animController != null)
            animController.SetBool("isReload", true);
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
        if (animController != null)
            animController.SetBool("isReload", false);
    }
}
