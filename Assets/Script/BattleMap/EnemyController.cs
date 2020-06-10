using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private enum BehaviorState 
    { 
        chaseTarget,
        attack,
        die,
    }
    //AI行为
    private enum TypeState
    {
        RangeType,
        CloseType,
    }

    Animator EnemyAnim;

    public HealthBar healthBar;
    public int MaxHealth = 100;
    public int CurrentHealth;
    public int Damge = 10;
    public float attackrange ;
    private BehaviorState EnemyState;
    // private bool  IsDead = false;
    // private Vector3 startPosition;
    // private Vector3 clostsetEnemyPosition;

    Transform playerTarget;
    NavMeshAgent agent;

    private void Awake()
    {
        EnemyState = BehaviorState.chaseTarget;
    }




    public void ChaseTarget()
    {
        // EnemyAnim.SetInteger("condition", 0);
        //EnemyAnim.SetBool("BoolMoving" , true);
        EnemyAnim.SetTrigger("TriggerMoving");
        agent.SetDestination(playerTarget.position);
    } 
    //追踪目标逻辑
    public void Attack()
    {
        EnemyAnim.SetTrigger("TriggerRangedAttacking");
        //EnemyAnim.SetInteger("condition", 1);
        //EnemyAnim.SetBool("BoolMoving", false);
        //EnemyAnim.SetBool("BoolRangeAttack" , true);
        agent.SetDestination(transform.position);
       
    }
    //攻击逻辑
    void TakeDamage(int damge) 
    {
        CurrentHealth -= damge;
        healthBar.SetCurrentHealth(CurrentHealth);

    }

    public void Die()
    {
        EnemyAnim.SetTrigger("TriggerMeleeAttacking");
        agent.SetDestination(transform.position);
        Debug.Log("AWSL");
        Destroy(gameObject, 5f);
    }
    //死亡逻辑

    private void EnemyBehavior(BehaviorState EnemyState) 
    {
        switch (EnemyState)
        {
            case BehaviorState.chaseTarget:
                    ChaseTarget();
                break;

            case BehaviorState.attack:
                    Attack();
                break;

            case BehaviorState.die:
                Die();
                break;
        }
    }
    //行为的选择

    void Start()
    {
        
        playerTarget = PlayerController.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        EnemyAnim = GetComponent<Animator>();
        CurrentHealth = MaxHealth;
        healthBar.SetMaxhealth(MaxHealth);

        // startPosition = transform.position;

    }
    void Update()
    {
           
        float distance = Vector3.Distance(playerTarget.position, transform.position);
        //敌人和玩家的距离

        if (distance > attackrange&&
            CurrentHealth > 0)
        {
            EnemyBehavior(BehaviorState.chaseTarget);
        }
        else if (distance <= attackrange&&
            CurrentHealth > 0)
        {
            EnemyBehavior(BehaviorState.attack);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(Damge);
        }
        if (CurrentHealth <=0)
        {
            EnemyBehavior(BehaviorState.die);
        }
        
        

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackrange);
    }
}
