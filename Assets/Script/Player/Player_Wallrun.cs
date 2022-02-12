using UnityEngine;

public class Player_Wallrun : MonoBehaviour
{
    #region Assingables Variable
    [Header("Assingables")]
    [SerializeField] Transform orientation;
    [SerializeField] Camera cam;

    private Rigidbody rigidBody;
    #endregion

    #region Detection Variable
    [Header("Detection")]
    [SerializeField] private float wallDistance = .5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;
    #endregion

    #region WallRuning Variable
    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float minVelocity;

    private bool wallLeft = false;
    private bool wallRight = false;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    #endregion

    #region Camera Variable
    [Header("Camera")]
    [SerializeField] private bool enableChangeFov;
    [SerializeField] private float wallRunFovAdd;
    [SerializeField] private float wallRunFovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;

    private float fov;

    public float tilt { get; private set; }
    #endregion


    private void Start()
    {
        fov = cam.fieldOfView;
        rigidBody = GetComponent<Rigidbody>();
    }

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    void CheckWall()
    {
        if (Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance))
            wallLeft = leftWallHit.transform.gameObject.layer == LayerMask.NameToLayer("Wall");
        else
            wallLeft = false;
        //wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance);
        if (Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance))
            wallRight = rightWallHit.transform.gameObject.layer == LayerMask.NameToLayer("Wall");
        else
            wallRight = false;
        //wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance);
    }

    private void Update()
    {
        cam.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, tilt);
        //cam.transform.Rotate(new Vector3(0, 0, tilt));

        CheckWall();

        if (CanWallRun() && (wallLeft || wallRight) && new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z).magnitude > minVelocity)
            StartWallRun();
        else
            StopWallRun();
    }

    void StartWallRun()
    {
        rigidBody.useGravity = false;

        if (rigidBody.velocity.y > -1)
            rigidBody.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
        else
            rigidBody.AddForce(Vector3.up * wallRunGravity, ForceMode.Force);

        if(enableChangeFov)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov + wallRunFovAdd, wallRunFovTime * Time.deltaTime);

        if (wallLeft)
        {
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
            //rigidBody.AddForce(-leftWallHit.normal * 10000f * Time.deltaTime, ForceMode.Force);
        }
        else if (wallRight)
        {
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);
            //rigidBody.AddForce(-rightWallHit.normal * 10000f * Time.deltaTime, ForceMode.Force);
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
                rigidBody.AddForce(wallRunJumpDirection * wallRunForce, ForceMode.Force);
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
                rigidBody.AddForce(wallRunJumpDirection * wallRunForce, ForceMode.Force);
            }
        }
    }

    void StopWallRun()
    {
        rigidBody.useGravity = true;

        if (enableChangeFov)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunFovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }
}
