using System.Collections;
using System.Collections.Generic;
using Haply.HardwareAPI.Unity;
using UnityEngine;

public class ColllisionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;

    private float baseStiffness = 20f; // 기본 강성
    private float maxStiffness = 100f; // 최대 강성
    private float reducedStiffness = 0f; // 충돌하지 않을 때 감소된 강성
    private float currentStiffness; // 현재 적용되는 강성
    private float transitionSpeed = 5f; // 강성 값이 변화하는 속도

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();
        currentStiffness = reducedStiffness;

    }
    private void Update()
    {
        switch (collisionDetector.CollidingObjectTag)
        {
            case "target":
                currentStiffness = -1;
                break;

            default:
                float targetStiffness = collisionDetector.isColliding ? Mathf.Lerp(baseStiffness, maxStiffness, 1 - collisionDetector.HeightFactor) : reducedStiffness;
                currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, transitionSpeed * Time.deltaTime);
                break;
        }

        UpdateForceCalculation();
    }

    private void UpdateForceCalculation()
    {
        if (hapticThread.isInitialized && collisionDetector.isColliding)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                Vector3 targetPosition = Vector3.zero; // 원하는 위치 (예시)
                return (targetPosition - position) * currentStiffness;
            });
        }
    }
}
