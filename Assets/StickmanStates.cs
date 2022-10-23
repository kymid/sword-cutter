using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickmanStates : MonoBehaviour
{
    [SerializeField] LayerMask attackLayer;
    public enum Weapon { Sword, Shootgun}
    public enum State { Stay, Run, Attack}
    [HideInInspector] public State currentState;
    public Weapon weapon;
    [Header("Shootgun settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawn;

    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        currentState = State.Stay;
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon == Weapon.Sword)
            CheckMeleeAttack();
        else if (weapon == Weapon.Shootgun)
            CheckRangeAttack();
    }

    private void CheckMeleeAttack()
    {
        if (Physics.Raycast(transform.position + (Vector3.up + Vector3.back) * .6f, Vector3.back, 1.5f, attackLayer) & currentState != State.Attack)
        {
            currentState = State.Attack;
            animator.SetBool("isAttack", true);
        }
    }

    private void CheckRangeAttack()
    {
        if (Physics.Raycast(transform.position + (Vector3.up + Vector3.back) * .6f, Vector3.back, 15f, attackLayer) & currentState != State.Attack)
        {
            currentState = State.Attack;
            animator.SetBool("isShoot", true);
        }
    }

    private void Shoot()
    {
        Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        if (weapon == Weapon.Sword)
            Debug.DrawRay(transform.position + (Vector3.up + Vector3.back) * .6f, Vector3.back * 1.5f, Color.red);
        else if (weapon == Weapon.Shootgun)
            Debug.DrawRay(transform.position + (Vector3.up + Vector3.back) * .6f, Vector3.back * 15f, Color.cyan);
    }
}
