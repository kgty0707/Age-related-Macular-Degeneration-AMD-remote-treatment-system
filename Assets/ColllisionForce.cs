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

    private void Awake()
    {
        hapticThread = GetComponent<HapticThread>();
        collisionDetector = GetComponentInChildren<CollisionDetector>();
        currentStiffness = reducedStiffness;

    }
    private void Update()
    {
        
        float targetStiffness = collisionDetector.isColliding ? Mathf.Lerp(baseStiffness, maxStiffness, 1 - collisionDetector.HeightFactor) : reducedStiffness;
        // Mathf.Lerp�� ����Ͽ� ���� ������ �ε巴�� ��ǥ �������� ��ȭ��Ŵ
        currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, transitionSpeed * Time.deltaTime);
        UpdateForceCalculation();
    }

    private void UpdateForceCalculation()
    {
        if (hapticThread.isInitialized && collisionDetector.isColliding)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                Vector3 targetPosition = Vector3.zero; // ���ϴ� ��ġ (����)
                return (targetPosition - position) * currentStiffness;
            });
        }
    }
}
