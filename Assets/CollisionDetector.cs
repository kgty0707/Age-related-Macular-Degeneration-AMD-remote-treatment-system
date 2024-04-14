using Haply.HardwareAPI.Unity;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private HapticThread hapticThread;
    private bool isColliding = false;
    private float baseStiffness = 20f; // 기본 강성
    private float maxStiffness = 100f; // 최대 강성
    private float reducedStiffness = 0f; // 충돌하지 않을 때 감소된 강성
    private float currentStiffness; // 현재 적용되는 강성
    private float transitionSpeed = 5f; // 강성 값이 변화하는 속도
    private float penetrationThreshold = -0.02f; // y축 방향 임계치

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        currentStiffness = reducedStiffness;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision started with " + other.name);
        isColliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Collision ended with " + other.name);
        isColliding = false;
    }

    private void Update()
    {
        float heightFactor = Mathf.Clamp01((transform.position.y - penetrationThreshold) / -penetrationThreshold);
        float targetStiffness = isColliding ? Mathf.Lerp(baseStiffness, maxStiffness, 1 - heightFactor) : reducedStiffness;
        // Mathf.Lerp를 사용하여 현재 강성을 부드럽게 목표 강성으로 변화시킴
        currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, transitionSpeed * Time.deltaTime);

        UpdateForceCalculation();
    }

    private void UpdateForceCalculation()
    {
        if (hapticThread.isInitialized)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                Vector3 targetPosition = Vector3.zero; // 원하는 위치 (예시)
                return (targetPosition - position ) * currentStiffness;
            });
        }
    }
}
