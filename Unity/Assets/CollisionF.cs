using System.Collections;
using System.Collections.Generic;
using Haply.HardwareAPI.Unity;
using UnityEngine;

public class ColllisionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;


    private float baseStiffness = 20f; // 기본 강성
    private float maxStiffness = 200f; // 최대 강성
    private float reducedStiffness = 0f; // 충돌하지 않을 때 감소된 강성
    private float currentStiffness; // 현재 적용되는 강성
    private float transitionSpeed = 5f; // 강성 값이 변화하는 속도
    private float targetStiffness;
    private Vector3 lastPosition; // 이전 프레임에서의 위치


    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();
        currentStiffness = reducedStiffness;

    }

    private void Update()
    {
        Vector3 positionChange = transform.position - lastPosition;
        Vector3 movementDirection = new Vector3(0, positionChange.y, 0); // 물체의 움직임 방향
        Debug.Log(movementDirection);

        switch (collisionDetector.CollidingObjectTag)
        {
            case "eye":
                currentStiffness = -1;
                break;

            case "sphere":
                Debug.Log(movementDirection);
                targetStiffness = Mathf.Lerp(baseStiffness, maxStiffness, 1 - collisionDetector.HeightFactor);
                currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, transitionSpeed * Time.deltaTime);
                UpdateForceCalculation(movementDirection / 10); // 반대 방향으로 힘 적용
                break;


            default:
                currentStiffness = 200;
                break;

        }

    }

    private void UpdateForceCalculation(Vector3 forceDirection)
    {
        if (hapticThread.isInitialized && collisionDetector.isColliding)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                return forceDirection * currentStiffness;

            });
        }
    }
}
