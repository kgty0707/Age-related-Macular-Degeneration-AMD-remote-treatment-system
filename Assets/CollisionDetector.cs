using Haply.HardwareAPI.Unity;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private HapticThread hapticThread;
    private bool isColliding = false;
    private float baseStiffness = 20f; // �⺻ ����
    private float maxStiffness = 100f; // �ִ� ����
    private float reducedStiffness = 0f; // �浹���� ���� �� ���ҵ� ����
    private float currentStiffness; // ���� ����Ǵ� ����
    private float transitionSpeed = 5f; // ���� ���� ��ȭ�ϴ� �ӵ�
    private float penetrationThreshold = -0.02f; // y�� ���� �Ӱ�ġ

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
        // Mathf.Lerp�� ����Ͽ� ���� ������ �ε巴�� ��ǥ �������� ��ȭ��Ŵ
        currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, transitionSpeed * Time.deltaTime);

        UpdateForceCalculation();
    }

    private void UpdateForceCalculation()
    {
        if (hapticThread.isInitialized)
        {
            hapticThread.Run((in Vector3 position) =>
            {
                Vector3 targetPosition = Vector3.zero; // ���ϴ� ��ġ (����)
                return (targetPosition - position ) * currentStiffness;
            });
        }
    }
}
