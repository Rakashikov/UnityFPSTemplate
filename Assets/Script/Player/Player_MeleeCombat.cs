using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MeleeCombat : MonoBehaviour
{
    [Header("Assingables")]
    [SerializeField] GameObject playerHands;
    [SerializeField] GameObject fpsCam;

    [Header("Parameters")]
    [SerializeField] int numOfAttackAnims;
    [SerializeField] float attackDelay;
    [SerializeField] float effectDelay;
    [SerializeField] float attackRange;
    [SerializeField] float attackForce;

    private bool isAttacking = false;
    private Animator handsAnimator;

    private int numAnim = -1;
    private int prevNumAnim = -1;

    GameObject hitPointObject;

    private void Awake()
    {
        handsAnimator = playerHands.GetComponent<Animator>();
        hitPointObject = new GameObject("AttackPoint");
    }

    // Update is called once per frame
    private void Update()
    {
        myInput();
    }

    private void myInput()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            Attack();
        }
    }

    private void Attack()
    {
        while (prevNumAnim == numAnim) numAnim = (int)Mathf.Clamp(Random.Range(0.5f, 4.5f), 1, 4);
        prevNumAnim = numAnim;
        isAttacking = true;
        handsAnimator.SetBool("isAttack", isAttacking);
        handsAnimator.SetInteger("NumAttack", numAnim);
        Invoke("ResetAttack", attackDelay);
        Invoke("AttackReaction", effectDelay);
    }

    private void ResetAttack()
    {
        isAttacking = false;
        handsAnimator.SetBool("isAttack", isAttacking);
    }

    private void AttackReaction()
    {
        RaycastHit hit;
        if (Physics.CapsuleCast(fpsCam.transform.position, fpsCam.transform.position + fpsCam.transform.forward * attackRange, 0.1f, fpsCam.transform.forward, out hit, attackRange))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.GetComponent<Rigidbody>())
            {
                hitPointObject.transform.position = hit.point;
                hitPointObject.transform.SetParent(hit.collider.transform);
                hit.collider.GetComponent<Rigidbody>().AddForce(fpsCam.transform.position + fpsCam.transform.forward * attackForce, ForceMode.Force);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(fpsCam.transform.position, fpsCam.transform.position + fpsCam.transform.forward * attackRange);
    }
}
