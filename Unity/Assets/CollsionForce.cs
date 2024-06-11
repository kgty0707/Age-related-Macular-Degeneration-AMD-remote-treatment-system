using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haply.HardwareAPI.Unity;


public class CollsionForce : MonoBehaviour
{
    private CollisionDetector collisionDetector;
    private HapticThread hapticThread;

    private Vector3 lastPosition; // ���� �����ӿ����� ��ġ
    private float forceMultiplier = 3f; // �� ���� ��� (�ʿ信 ���� ���� ����)
    private float safetyForceThreshold = 20f; // ������ ���� ���� �ִ밪
    private Vector3 lastDampingForce = Vector3.zero; // ���� ���� ��
    private Vector3 lastForce = Vector3.zero; // ���� �����ӿ����� ��

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();

    }

    private void Update()
    {
        Vector3 positionChange = transform.position - lastPosition;
        Vector3 movementDirection = positionChange.normalized; // ��ü�� ������ ���� ����ȭ
        Vector3 velocity = positionChange / Time.deltaTime; // ���� �ӵ� ���

        if (collisionDetector.CollidingObjectTag == "sphere" && collisionDetector.isColliding)
        {
            float elasticModulus = Mathf.Lerp(0.16f, 0.30f, collisionDetector.HeightFactor) * 1e6f; // ź�� ����� MPa���� Pa�� ��ȯ
            float area = Mathf.PI * Mathf.Pow(0.012f, 2); // �ȱ��� ���� ���� (������ 1.2cm�� ��)
            float displacement = positionChange.magnitude; // ������ ũ��
            float springForce = elasticModulus * area * displacement; // ���� ��Ģ�� ����Ͽ� ���� ��
            Vector3 reactiveForce = springForce * -movementDirection * forceMultiplier; // ���ۿ� ���� �ݴ� �������� ����
            /*Vector3 smoothForce = Vector3.Lerp(lastForce, reactiveForce, 0.5f);// �ε巴�� ���� �����ϱ� ���� Lerp�� ���
            lastForce = smoothForce; // ���� �������� ���� ����
            */
            Debug.Log($"Force (Sphere): {reactiveForce}");
            UpdateForceCalculation(reactiveForce);
            /*
            // Xy�� �������� �������� ���ϰ� �ϴ� �� �߰�
            Vector3 xyRestrictionForce = -new Vector3(velocity.x, velocity.y, 0) * 1f;
            // Xy�� ���� ���� ���ۿ� ���� �ջ�
            Vector3 totalForce = reactiveForce + xyRestrictionForce;
          
            Debug.Log(totalForce);*/
        }
    
        else if (collisionDetector.CollidingObjectTag == "eye" && collisionDetector.isColliding)
        {
           Vector3 dampingForce = -velocity * 7f; // ��� �࿡ ���� ���� �� ���
            //float yDampingForce = -velocity.y * boxDamping * 5f; // Y�� ���� ���� �� ���
            //Vector3 dampingForce = new Vector3(0, yDampingForce, 0);

            // Lerp�� ����Ͽ� �ε巯�� ���� ����
            dampingForce = Vector3.Lerp(lastDampingForce, dampingForce, 0.1f);
            lastDampingForce = dampingForce;

            Debug.Log($"Damping Force (Eye): {dampingForce}");
            UpdateForceCalculation(dampingForce);
        }
        else
        {
            UpdateForceCalculation(new Vector3(1e-6f, 1e-6f, 1e-6f));
            lastDampingForce = Vector3.zero; // �浹�� ���� ���� ���� �� �ʱ�ȭ
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
