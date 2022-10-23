using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickmanWeaponCollision : MonoBehaviour
{

    public bool cantDamage = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") | cantDamage) return;

        PathFollower follower = other.GetComponentInParent<PathFollower>();
        follower.Speed = 0;

        Animator animator = GetComponentInParent<Animator>();
        animator.SetBool("isAttack", false);
        cantDamage = true;

        MenuController.instance.Activate(MenuController.instance.LoseMenu);
    }
}
