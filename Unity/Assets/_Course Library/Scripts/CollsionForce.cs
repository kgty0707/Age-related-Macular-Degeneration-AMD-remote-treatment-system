using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haply.HardwareAPI.Unity;

public class CollsionForce : MonoBehaviour
/*
이 클래스는 충돌 시 힘을 계산하고 적용하는 기능을 담당.
충돌 객체에 따라 탄성 힘 또는 댐핑 힘을 적용,
힘의 크기를 안전한 범위 내로 제한.
*/

{
    private CollisionDetector collisionDetector; // CollisionDetector 컴포넌트 참조
    private HapticThread hapticThread; // HapticThread 컴포넌트 참조

    private Vector3 lastPosition; // 이전 프레임에서의 위치
    private float forceMultiplier = 3f; // 힘 증폭 계수 (필요에 따라 조절 가능)
    private float safetyForceThreshold = 20f; // 안전을 위한 힘의 최대값
    private Vector3 lastDampingForce = Vector3.zero; // 이전 댐핑 힘
    private Vector3 lastForce = Vector3.zero; // 이전 프레임에서의 힘

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>(); // HapticThread 컴포넌트 가져오기
        collisionDetector = GetComponentInChildren<CollisionDetector>(); // 자식 객체에서 CollisionDetector 컴포넌트 가져오기
    }

    private void Update()
    {
        Vector3 positionChange = transform.position - lastPosition; // 위치 변화 계산
        Vector3 movementDirection = positionChange.normalized; // 물체의 움직임 방향 정규화
        Vector3 velocity = positionChange / Time.deltaTime; // 현재 속도 계산

        if (collisionDetector.CollidingObjectTag == "sphere" && collisionDetector.isColliding)
        {
            // 탄성 계수를 높이 계수에 따라 선형 보간하고, MPa에서 Pa로 변환
            float elasticModulus = Mathf.Lerp(0.16f, 0.30f, collisionDetector.HeightFactor) * 1e6f;
            // 안구의 추정 면적 (반지름 1.2cm의 원)
            float area = Mathf.PI * Mathf.Pow(0.012f, 2);
            // 변위의 크기
            float displacement = positionChange.magnitude;
            // 훅의 법칙을 사용하여 계산된 힘
            float springForce = elasticModulus * area * displacement;
            // 반작용 힘을 반대 방향으로 적용
            Vector3 reactiveForce = springForce * -movementDirection * forceMultiplier;
            Debug.Log($"Force (Sphere): {reactiveForce}");
            // 힘 계산 업데이트
            UpdateForceCalculation(reactiveForce);
        }
        else if (collisionDetector.CollidingObjectTag == "eye" && collisionDetector.isColliding)
        {
            // 모든 축에 대해 댐핑 힘 계산
            Vector3 dampingForce = -velocity * 7f;

            // Lerp를 사용하여 부드러운 댐핑 적용
            dampingForce = Vector3.Lerp(lastDampingForce, dampingForce, 0.1f);
            lastDampingForce = dampingForce;

            Debug.Log($"Damping Force (Eye): {dampingForce}");
            // 힘 계산 업데이트
            UpdateForceCalculation(dampingForce);
        }
        else
        {
            // 힘 계산 업데이트
            UpdateForceCalculation(new Vector3(1e-6f, 1e-6f, 1e-6f));
            // 충돌이 없을 때는 댐핑 힘 초기화
            lastDampingForce = Vector3.zero;
        }
        // 다음 프레임을 위해 이전 위치 업데이트
        lastPosition = transform.position;
    }

    private void UpdateForceCalculation(Vector3 forceDirection)
    {
        // 안전을 위해 힘의 최대값 초과 시 힘을 0으로 설정
        if (forceDirection.magnitude > safetyForceThreshold)
        {
            forceDirection = Vector3.zero;
        }

        // HapticThread가 초기화되고 충돌이 발생한 경우
        if (hapticThread.isInitialized && collisionDetector.isColliding)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                return forceDirection; // 계산된 힘 반환
            });
        }
    }
}
