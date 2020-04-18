using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private Animator anim = null;
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private PlayerStats playerStats = null;

    [Header("config")]
    [SerializeField] private bool combatEnabled = true;
    [SerializeField] private float inputTimer = 0.2f;
    [SerializeField] private float attack1Radius = 0.8f;
    [SerializeField] private float attack1Damage = 10f;
    [SerializeField] private Transform attack1HitBoxPos;
    [SerializeField] private LayerMask whatIsDamageable;
    
    private bool gotInput, isAttacking, isFirstAttack;

    private float lastInputTime = Mathf.NegativeInfinity;

    private Collider2D[] detectedObjects = new Collider2D[1];

    
    private void Start()
    {
        anim.SetBool("canAttack", combatEnabled);
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {
                //Attempt combat
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            //Perform Attack1
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }

        if(Time.time >= lastInputTime + inputTimer)
        {
            //Wait for new input
            gotInput = false;
        }
    }
    
   
    private void CheckAttackHitBox()
    {

       

        Physics2D.OverlapCircleNonAlloc(attack1HitBoxPos.position, attack1Radius, detectedObjects, whatIsDamageable);
        //Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        var stop = System.Diagnostics.Stopwatch.StartNew();
        stop.Start();

        // en teoria dentro del layer whatisdamageable deberia de tener basicenemycontroller.
        for (ushort i = 0; i < detectedObjects.Length; i++)
        { 
            if (detectedObjects[i] is null) continue;

            if (detectedObjects[i].CompareTag("ObjectedDestroyed"))
            { 
            
                detectedObjects[i].GetComponentInParent<ObjectDestroyed>().SetObjectDestroyed(detectedObjects[i].transform.parent.gameObject, 
                    this.transform.position.x , this.transform.position.y);

            }
            if (detectedObjects[i].CompareTag("ObjectedHealed"))
            {
                

            }
            
            detectedObjects[i] = default;


        }

        
        stop.Stop();
        print("check collider tiempo ms=" + stop.Elapsed.TotalMilliseconds);

    }

    private void FinishAttack1()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void Damage(float[] attackDetails)
    {
        if (!playerController.GetDashStatus())
        {
            int direction;

            playerStats.DecreaseHealth(attackDetails[0]);

            if (attackDetails[1] < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }

            playerController.Knockback(direction);
        }        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
