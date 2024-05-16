using Haply.HardwareAPI.Unity;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool isColliding = false;
    public Vector3 CollisionPoint { get; private set; } // 충돌 지점 저장
    public float HeightFactor { get; private set; } // 외부에서 접근 가능한 heightFactor
    public float penetrationThreshold = -0.02f; // y축 방향 임계치
    public string CollidingObjectTag; // 현재 충돌 중인 객체의 태그 저장
    public Vector3 EntryDirection { get; private set; }  // 충돌 진입 방향

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision started with " + other.name);
        isColliding = true;
        CollisionPoint = other.ClosestPointOnBounds(transform.position); // 충돌 지점 업데이트
        EntryDirection = (CollisionPoint - transform.position).normalized;  // 진입 방향 계산 및 저장
        CollidingObjectTag = other.tag;
        UpdateHeightFactor(CollisionPoint.y); // 충돌 지점을 기준으로 HeightFactor 계산
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Collision ended with " + other.name);
        if (other.tag == CollidingObjectTag)
        {
            isColliding = false;
            CollidingObjectTag = null; // 충돌 종료 시 태그 초기화
        }
        UpdateHeightFactor(transform.position.y); // 충돌 종료 시 현재 위치를 기준으로 HeightFactor 계산
    }

    private void UpdateHeightFactor(float yPosition)
    {
        // 주어진 y 위치를 사용하여 HeightFactor 계산
        HeightFactor = Mathf.Clamp01((yPosition - penetrationThreshold) / -penetrationThreshold);
    }
}
