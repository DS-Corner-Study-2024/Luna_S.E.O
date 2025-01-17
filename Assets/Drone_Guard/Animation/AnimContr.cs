using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimContr : MonoBehaviour
{
    public Animator anim;
    public Transform drone;  // 드론 객체 Transform
    public float targetHeight = 5.0f;  // 원하는 높이
    public float moveSpeed = 2.0f;     // 높이 이동 속도

    private bool isMovingUp = false;   // 높이 이동 여부 확인

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 드론의 높이를 이동
        if (isMovingUp)
        {
            Vector3 targetPosition = new Vector3(drone.position.x, targetHeight, drone.position.z);
            drone.position = Vector3.MoveTowards(drone.position, targetPosition, moveSpeed * Time.deltaTime);

            // 목표 높이에 도달하면 이동 중지
            if (drone.position.y >= targetHeight)
            {
                isMovingUp = false;
            }
        }
    }
}
