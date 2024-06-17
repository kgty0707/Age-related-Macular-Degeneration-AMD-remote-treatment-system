using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haply.HardwareAPI.Unity;


public class CollsionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;

    private Vector3 lastPosition; // 이전 프레임에서의 위치
    private float forceMultiplier = 3f; // 힘 증폭 계수 (필요에 따라 조절 가능)
    private float safetyForceThreshold = 20f; // 안전을 위한 힘의 최대값
    private Vector3 lastDampingForce = Vector3.zero; // 이전 댐핑 힘
    private Vector3 lastForce = Vector3.zero; // 이전 프레임에서의 힘

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();

    }

    private void Update()
    {
        Vector3 positionChange = transform.position - lastPosition;
        Vector3 movementDirection = positionChange.normalized; // 물체의 움직임 방향 정규화
        Vector3 velocity = positionChange / Time.deltaTime; // 현재 속도 계산

        if (collisionDetector.CollidingObjectTag == "sphere" && collisionDetector.isColliding)
        {
            float elasticModulus = Mathf.Lerp(0.16f, 0.30f, collisionDetector.HeightFactor) * 1e6f; // 탄성 계수를 MPa에서 Pa로 변환
            float area = Mathf.PI * Mathf.Pow(0.012f, 2); // 안구의 추정 면적 (반지름 1.2cm의 원)
            float displacement = positionChange.magnitude; // 변위의 크기
            float springForce = elasticModulus * area * displacement; // 훅의 법칙을 사용하여 계산된 힘
            Vector3 reactiveForce = springForce * -movementDirection * forceMultiplier; // 반작용 힘을 반대 방향으로 적용
            /*Vector3 smoothForce = Vector3.Lerp(lastForce, reactiveForce, 0.5f);// 부드럽게 힘을 적용하기 위해 Lerp를 사용
            lastForce = smoothForce; // 현재 프레임의 힘을 저장
            */
            Debug.Log($"Force (Sphere): {reactiveForce}");
            UpdateForceCalculation(reactiveForce);
            /*
            // Xy축 방향으로 움직이지 못하게 하는 힘 추가
            Vector3 xyRestrictionForce = -new Vector3(velocity.x, velocity.y, 0) * 1f;
            // Xy축 제한 힘과 반작용 힘을 합산
            Vector3 totalForce = reactiveForce + xyRestrictionForce;
          
            Debug.Log(totalForce);*/
        }
    
        else if (collisionDetector.CollidingObjectTag == "eye" && collisionDetector.isColliding)
        {
           Vector3 dampingForce = -velocity * 7f; // 모든 축에 대해 댐핑 힘 계산
            //float yDampingForce = -velocity.y * boxDamping * 5f; // Y축 방향 댐핑 힘 계산
            //Vector3 dampingForce = new Vector3(0, yDampingForce, 0);

            // Lerp를 사용하여 부드러운 댐핑 적용
            dampingForce = Vector3.Lerp(lastDampingForce, dampingForce, 0.1f);
            lastDampingForce = dampingForce;

            Debug.Log($"Damping Force (Eye): {dampingForce}");
            UpdateForceCalculation(dampingForce);
        }
        else
        {
            UpdateForceCalculation(new Vector3(1e-6f, 1e-6f, 1e-6f));
            lastDampingForce = Vector3.zero; // 충돌이 없을 때는 댐핑 힘 초기화
        }
        lastPosition = transform.position; // Update the last position for the next frame
    }

    private void UpdateForceCalculation(Vector3 forceDirection)
    {
        if (forceDirection.magnitude > safetyForceThreshold)
        {
            forceDirection = Vector3.zero;
        }

        if (hapticThread.isInitialized && collisionDetector.isColliding)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                return forceDirection;
            });
        }
    }
}
