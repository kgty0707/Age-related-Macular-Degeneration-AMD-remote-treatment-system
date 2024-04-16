using Haply.HardwareAPI.Unity;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool isColliding = false;
    public Vector3 CollisionPoint { get; private set; } // 충돌 지점 저장
    public float HeightFactor { get; private set; } // 외부에서 접근 가능한 heightFactor
    public float penetrationThreshold = -0.02f; // y축 방향 임계치


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision started with " + other.name);
        isColliding = true;
        CollisionPoint = other.ClosestPointOnBounds(transform.position); // 충돌 지점 업데이트
        UpdateHeightFactor(CollisionPoint.y); // 충돌 지점을 기준으로 HeightFactor 계산
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Collision ended with " + other.name);
        isColliding = false;
        UpdateHeightFactor(transform.position.y); // 충돌 종료 시 현재 위치를 기준으로 HeightFactor 계산
    }

    private void UpdateHeightFactor(float yPosition)
    {
        // 주어진 y 위치를 사용하여 HeightFactor 계산
        HeightFactor = Mathf.Clamp01((yPosition - penetrationThreshold) / -penetrationThreshold);
    }

}
