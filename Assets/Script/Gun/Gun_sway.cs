using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_sway : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float intensity;
    [SerializeField] private float smooth;

    private Transform player;
    private Quaternion originRotation;

    private void Start()
    {
        player = transform.root;
        originRotation = transform.localRotation;
    }

    private void Update()
    {
        UpdateSway();
    }

    private void UpdateSway()
    {
        float t_xMouse = Input.GetAxis("Mouse X");
        float t_yMouse = Input.GetAxis("Mouse Y");

        Quaternion tXAdj = Quaternion.AngleAxis(-intensity * Mathf.Clamp(t_xMouse, -3f, 3f), -Vector3.up);
        Quaternion tYAdj = Quaternion.AngleAxis(intensity * Mathf.Clamp(t_yMouse, -3f, 3f), -Vector3.right);
        Quaternion targetRotation = originRotation * tXAdj * tYAdj;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);
    }
}
