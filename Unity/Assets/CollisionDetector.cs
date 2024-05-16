using Haply.HardwareAPI.Unity;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool isColliding = false;
    public Vector3 CollisionPoint { get; private set; } // �浹 ���� ����
    public float HeightFactor { get; private set; } // �ܺο��� ���� ������ heightFactor
    public float penetrationThreshold = -0.02f; // y�� ���� �Ӱ�ġ
    public string CollidingObjectTag; // ���� �浹 ���� ��ü�� �±� ����
    public Vector3 EntryDirection { get; private set; }  // �浹 ���� ����

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision started with " + other.name);
        isColliding = true;
        CollisionPoint = other.ClosestPointOnBounds(transform.position); // �浹 ���� ������Ʈ
        EntryDirection = (CollisionPoint - transform.position).normalized;  // ���� ���� ��� �� ����
        CollidingObjectTag = other.tag;
        UpdateHeightFactor(CollisionPoint.y); // �浹 ������ �������� HeightFactor ���
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Collision ended with " + other.name);
        if (other.tag == CollidingObjectTag)
        {
            isColliding = false;
            CollidingObjectTag = null; // �浹 ���� �� �±� �ʱ�ȭ
        }
        UpdateHeightFactor(transform.position.y); // �浹 ���� �� ���� ��ġ�� �������� HeightFactor ���
    }

    private void UpdateHeightFactor(float yPosition)
    {
        // �־��� y ��ġ�� ����Ͽ� HeightFactor ���
        HeightFactor = Mathf.Clamp01((yPosition - penetrationThreshold) / -penetrationThreshold);
    }
}
