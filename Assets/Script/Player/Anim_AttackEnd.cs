using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_AttackEnd : StateMachineBehaviour
{

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isAttack", false);
    }

}
