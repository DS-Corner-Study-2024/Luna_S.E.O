using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] 
    private float chaseRange = 5f; //target과의 거리
    private float distanceToTarget = Mathf.Infinity;

    public bool isChase;
    public bool isAttack;

    public Transform target;// 목표물
    public BoxCollider meleeArea;

    Rigidbody rigid;
    BoxCollider boxCollider;
    NavMeshAgent nav;

    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 4);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("Walk_Anim", true);
    }

    void Update()
    {
        if (!isChase) return;

        if (nav.enabled)
        {
            //target과의 거리 구하기
            distanceToTarget = Vector3.Distance(target.position, transform.position);

            //target과의 거리가 chaseRange보다 작을 때 추적
            if (distanceToTarget <= chaseRange)
            {
                nav.SetDestination(target.position);
            } else
            {
                nav.isStopped = !isChase;
            }
        }
    }


    void FreezeVeloctiy()
    {
        // 물리력 영향x
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }

    }

    void Targerting()
    {
        float targetRadius = 1f;
        float targetRange = 1.5f;

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if (rayHits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }

    }

    public void StopEnemy()
    {
        // NavMeshAgent 멈추기
        nav.isStopped = true;
        nav.velocity = Vector3.zero;

        // 애니메이션 멈추기
        anim.SetBool("Walk_Anim", false);
        anim.SetBool("Roll_Anim", false);

        // 물리력 멈추기
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        // 추격 및 공격 중지
        isChase = false;
        isAttack = false;
    }


    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("Roll_Anim", true);

        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(1f);
        meleeArea.enabled = false;

        isChase = true;
        isAttack = false;
        anim.SetBool("Roll_Anim", false);

    }

    void FixedUpdate()
    {
        if (!isChase) return;

        Targerting();
        FreezeVeloctiy();
    }

}