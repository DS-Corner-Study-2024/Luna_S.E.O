using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    // ���ǵ����
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

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
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
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
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
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
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

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void CameraRotation()
    {
        float _xRotaion = Input.GetAxisRaw("Mouse Y");
        float _cameraRotaionX = _xRotaion * lookSensitivity;

         
        currentCameraRotationX -= _cameraRotaionX;
        // ���콺 �Ѱ谪 ����
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // ���� ����
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}
