using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneController : MonoBehaviour
{
    public bool isChase;
    public bool isAttack;

    public Transform target;// ��ǥ��
    public BoxCollider meleeArea;

    Rigidbody rigid;
    BoxCollider boxCollider;
    NavMeshAgent nav;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();

        Invoke("ChaseStart", 4);
    }

    void ChaseStart()
    {
        isChase = true;
    }

    void Update()
    {
        if (!isChase) return;

        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
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

        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.1f);

        meleeArea.enabled = false;

        isChase = true;
        isAttack = false;

    }   

    void FixedUpdate()
    {
        if (!isChase) return;

        Targerting();
        FreezeVeloctiy();
    }

}