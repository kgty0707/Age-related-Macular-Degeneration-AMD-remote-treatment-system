using UnityEngine;

public class SetupObjects : MonoBehaviour
{
    public GameObject movingObject; // ù ��° ������Ʈ
    public GameObject stationaryObject; // �� ��° ������Ʈ

    void Start()
    {
        // ù ��° ������Ʈ ����
        Rigidbody movingRb = movingObject.GetComponent<Rigidbody>();
        if (movingRb == null)
        {
            movingRb = movingObject.AddComponent<Rigidbody>();
        }
        movingRb.isKinematic = false; // ù ��° ������Ʈ�� ���������� ������
        movingRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        movingRb.constraints = RigidbodyConstraints.FreezeRotation; // ȸ���� ����

        // �� ��° ������Ʈ ����
        Rigidbody stationaryRb = stationaryObject.GetComponent<Rigidbody>();
        if (stationaryRb == null)
        {
            stationaryRb = stationaryObject.AddComponent<Rigidbody>();
        }
        stationaryRb.isKinematic = true; // �� ��° ������Ʈ�� �������� ����

        // Collider ����
        Collider movingCollider = movingObject.GetComponent<Collider>();
        if (movingCollider == null)
        {
            movingCollider = movingObject.AddComponent<BoxCollider>(); // Collider ������ �ʿ信 ���� ����
        }
        movingCollider.isTrigger = false;

        Collider stationaryCollider = stationaryObject.GetComponent<Collider>();
        if (stationaryCollider == null)
        {
            stationaryCollider = stationaryObject.AddComponent<BoxCollider>(); // Collider ������ �ʿ信 ���� ����
        }
        stationaryCollider.isTrigger = false;
    }
}
