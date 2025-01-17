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
    // 스피드 관련
    [SerializeField]
    private float walkSpeed = 4;
    [SerializeField]
    private float runSpeed = 6;
    private float applySpeed;

    // 점프 관련
    [SerializeField]
    private float jumpForce =  3;

    // 상태 변수
    private bool isRun = false;
    private bool isGround = true;
    private bool isBorder;

    // 착지 여부 확인
    private CapsuleCollider capsuleCollider;

    // 민감도 조절
    [SerializeField]
    private float lookSensitivity = 5; 

    // 카메라관련
    [SerializeField]
    private float cameraRotationLimit = 45; // 카메라 방향 제한
    private float currentCameraRotationX = 0f; // 현재 카메라 방향

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera; // 카메라 지정

    private Rigidbody myRigid;

    MeshRenderer[] meshs;

    // 체력관련
    [SerializeField]
    public int health;
    private bool isDamage;
    private bool isDamage2;
    private bool isDead;

    // 공격받는 경우
    public GameObject gamebg;
    public GameObject damagebg;

    // 열쇠관련
    [SerializeField]
    public int maxHasKey;
    public int key;

    // 드론관련
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
        CameraRotation(); // 화면 위아래 이동
        CharacterRotation(); // 화면 좌우 이동 -> 캐릭터 회전
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
        
        // 벽 통과 방지 & 죽으면 움직임 제한
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
        // 마우스 한계값 지정
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        if (!isDead)
            // 실제 적용
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
        // 앞에 있는 물체 인식
        Debug.DrawRay(transform.position, transform.forward * 1, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 1, LayerMask.GetMask("Wall"));
    }

    private void FixedUpdate()
    {
        // 제자리 회전 방지
        FreezeRotation();
        // 벽 관통 방지
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

                    Debug.Log("열쇠를 획득헀습니다!");

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

                Debug.Log("플레이어가 공격을 당했습니다! 현재 체력: " + health);

                StartCoroutine(OnDamage());
            }
        }
        else if (other.tag == "DroneBullet")
        {
            if (!isDamage2)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                gasCount += enemyBullet.damage;

                Debug.Log("가스에 노출되었습니다! 현재 수치: " + gasCount);

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
