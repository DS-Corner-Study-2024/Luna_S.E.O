using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] 
    private float chaseRange = 5f; //target���� �Ÿ�
    private float distanceToTarget = Mathf.Infinity;

    public bool isChase;
    public bool isAttack;

    public Transform target;// ��ǥ��
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
            //target���� �Ÿ� ���ϱ�
            distanceToTarget = Vector3.Distance(target.position, transform.position);

            //target���� �Ÿ��� chaseRange���� ���� �� ����
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
        // ������ ����x
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
        // NavMeshAgent ���߱�
        nav.isStopped = true;
        nav.velocity = Vector3.zero;

        // �ִϸ��̼� ���߱�
        anim.SetBool("Walk_Anim", false);
        anim.SetBool("Roll_Anim", false);

        // ������ ���߱�
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        // �߰� �� ���� ����
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