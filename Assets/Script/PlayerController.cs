using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;
using UnityEngine.XR;
using static UnityEditor.Progress;

public class PlayerController : MonoBehaviour
{
    // ���ǵ� ����
    [SerializeField]
    private float walkSpeed = 4;
    [SerializeField]
    private float runSpeed = 6;
    private float applySpeed;

    // ���� ����
    [SerializeField]
    private float jumpForce =  3;

    // ���� ����
    private bool isRun = false;
    private bool isGround = true;
    private bool isBorder;

    // ���� ���� Ȯ��
    private CapsuleCollider capsuleCollider;

    // �ΰ��� ����
    [SerializeField]
    private float lookSensitivity = 5; 

    // ī�޶����
    [SerializeField]
    private float cameraRotationLimit = 45; // ī�޶� ���� ����
    private float currentCameraRotationX = 0f; // ���� ī�޶� ����

    // �ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCamera; // ī�޶� ����

    private Rigidbody myRigid;

    MeshRenderer[] meshs;

    // ü�°���
    [SerializeField]
    public int health;
    private bool isDamage;
    private bool isDamage2;
    private bool isDead;

    // ���ݹ޴� ���
    public GameObject gamebg;
    public GameObject damagebg;

    // �������
    [SerializeField]
    public int maxHasKey;
    public int key;

    // ��а���
    [SerializeField]
    private int maxGas;
    public int gasCount;

    public GameManager gameManager;

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        meshs = GetComponents<MeshRenderer>();
        applySpeed = walkSpeed;
    }

    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        Move();
        CameraRotation(); // ȭ�� ���Ʒ� �̵�
        CharacterRotation(); // ȭ�� �¿� �̵� -> ĳ���� ȸ��
    }

    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && !isDead)
        {
            Jump();
        }
    }

    private void Jump()
    {
        myRigid.velocity = transform.up * jumpForce;
    }


    private void TryRun()
    {
        if(!isDead)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Running();
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                RunningCancel();
            }
        }
        
    }

    private void Running()
    {
        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        
        // �� ��� ���� & ������ ������ ����
        if (!isBorder && !isDead)
        {
            myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        }
    }

    private void CameraRotation()
    {
        float _xRotaion = Input.GetAxisRaw("Mouse Y");
        float _cameraRotaionX = _xRotaion * lookSensitivity;

         
        currentCameraRotationX -= _cameraRotaionX;
        // ���콺 �Ѱ谪 ����
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        if (!isDead)
            // ���� ����
            theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }

    private void FreezeRotation()
    {
        myRigid.angularVelocity = Vector3.zero;
    }

    private void StopToWall()
    {
        // �տ� �ִ� ��ü �ν�
        Debug.DrawRay(transform.position, transform.forward * 1, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 1, LayerMask.GetMask("Wall"));
    }

    private void FixedUpdate()
    {
        // ���ڸ� ȸ�� ����
        FreezeRotation();
        // �� ���� ����
        StopToWall();
    }

  
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Key")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Key:
                    key += item.value;

                    Debug.Log("���踦 ȹ�������ϴ�!");

                    if (key > maxHasKey)
                        key = maxHasKey;
                    break;
            }

            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                Debug.Log("�÷��̾ ������ ���߽��ϴ�! ���� ü��: " + health);

                StartCoroutine(OnDamage());
            }
        }
        else if (other.tag == "DroneBullet")
        {
            if (!isDamage2)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                gasCount += enemyBullet.damage;

                Debug.Log("������ ����Ǿ����ϴ�! ���� ��ġ: " + gasCount);

                StartCoroutine(OnDamage2());
            }
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true;

        if (gamebg != null && damagebg != null)
        {
            damagebg.SetActive(true);
            gamebg.SetActive(false);

            yield return new WaitForSeconds(0.3f);

            damagebg.SetActive(false);
            gamebg.SetActive(true);

            yield return new WaitForSeconds(0.3f);
        }

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        if (!isDead)
        {
            if (health <= 0 || gasCount == 100)
            {
                OnDie();
            }
        }

        yield return new WaitForSeconds(0.5f);

        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }

    IEnumerator OnDamage2()
    {
        isDamage2 = true;

        if (gamebg != null && damagebg != null)
        {
            damagebg.SetActive(true);
            gamebg.SetActive(false);

            yield return new WaitForSeconds(0.3f);

            damagebg.SetActive(false);
            gamebg.SetActive(true);

            yield return new WaitForSeconds(0.3f);
        }

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        if (!isDead)
        {
            if (health <= 0 || gasCount == 100)
            {
                OnDie();
            }
        }

        yield return new WaitForSeconds(0.5f);

        isDamage2 = false;

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }

    void OnDie()
    {
        isDead = true;
        gameManager.GameOver();
    }
    
}
