using System.Collections;
using System.Collections.Generic;
using Haply.HardwareAPI.Unity;
using UnityEngine;

public class ColllisionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;


    private float baseStiffness = 20f; // �⺻ ����
    private float maxStiffness = 100f; // �ִ� ����
    private float reducedStiffness = 0f; // �浹���� ���� �� ���ҵ� ����
    private float currentStiffness; // ���� ����Ǵ� ����
    private float transitionSpeed = 5f; // ���� ���� ��ȭ�ϴ� �ӵ�
    private Vector3 lastPosition; // ���� �����ӿ����� ��ġ



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
                Vector3 positionChange = transform.position - lastPosition;
                Vector3 movementDirection = new Vector3(0, positionChange.y, 0); // ��ü�� ������ ����
                Debug.Log(movementDirection);
                float targetStiffness = Mathf.Lerp(baseStiffness, maxStiffness, 1 - collisionDetector.HeightFactor);
                currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, transitionSpeed * Time.deltaTime);
                UpdateForceCalculation(movementDirection/8); // �ݴ� �������� �� ����
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
