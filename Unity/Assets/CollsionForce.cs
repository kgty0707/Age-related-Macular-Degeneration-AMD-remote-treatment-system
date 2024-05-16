using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haply.HardwareAPI.Unity;


public class CollsionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;

    private Vector3 lastPosition; // ���� �����ӿ����� ��ġ
    private float forceMultiplier = 1f; // �� ���� ��� (�ʿ信 ���� ���� ����)
    private float punctureThreshold = 0.5f; // õ�� �Ӱ谪 (������ ũ��)

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();
    }

    private void Update()
    {
        Vector3 positionChange = transform.position - lastPosition;
        Vector3 movementDirection = positionChange.normalized; // ��ü�� ������ ���� ����ȭ

        if (collisionDetector.CollidingObjectTag == "sphere" && collisionDetector.isColliding)
        {
            float elasticModulus = Mathf.Lerp(0.16f, 0.30f, collisionDetector.HeightFactor) * 1e6f; // ź�� ����� MPa���� Pa�� ��ȯ
            float area = Mathf.PI * Mathf.Pow(0.012f, 2); // �ȱ��� ���� ���� (������ 1.2cm�� ��)
            float displacement = positionChange.magnitude; // ������ ũ��
            float springForce = elasticModulus * area * displacement; // ���� ��Ģ�� ����Ͽ� ���� ��
            Vector3 reactiveForce = springForce * -movementDirection * forceMultiplier; // ���ۿ� ���� �ݴ� �������� ����
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
