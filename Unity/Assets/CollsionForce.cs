using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haply.HardwareAPI.Unity;


public class CollsionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;

    private Vector3 lastPosition; // 이전 프레임에서의 위치
    private float forceMultiplier = 1f; // 힘 증폭 계수 (필요에 따라 조절 가능)
    private float punctureThreshold = 0.5f; // 천공 임계값 (변위의 크기)

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();
    }

    private void Update()
    {
        Vector3 positionChange = transform.position - lastPosition;
        Vector3 movementDirection = positionChange.normalized; // 물체의 움직임 방향 정규화

        if (collisionDetector.CollidingObjectTag == "sphere" && collisionDetector.isColliding)
        {
            float elasticModulus = Mathf.Lerp(0.16f, 0.30f, collisionDetector.HeightFactor) * 1e6f; // 탄성 계수를 MPa에서 Pa로 변환
            float area = Mathf.PI * Mathf.Pow(0.012f, 2); // 안구의 추정 면적 (반지름 1.2cm의 원)
            float displacement = positionChange.magnitude; // 변위의 크기
            float springForce = elasticModulus * area * displacement; // 훅의 법칙을 사용하여 계산된 힘
            Vector3 reactiveForce = springForce * -movementDirection * forceMultiplier; // 반작용 힘을 반대 방향으로 적용
            Debug.Log(reactiveForce);
            UpdateForceCalculation(reactiveForce);
        }
        else
        {
            UpdateForceCalculation(Vector3.zero);
        }
        lastPosition = transform.position; // Update the last position for the next frame
    }

    private void UpdateForceCalculation(Vector3 forceDirection)
    {
        if (hapticThread.isInitialized && collisionDetector.isColliding)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                return forceDirection;

            });
        }
    }
}
