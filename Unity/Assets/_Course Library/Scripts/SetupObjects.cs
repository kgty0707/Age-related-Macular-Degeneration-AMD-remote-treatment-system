using UnityEngine;

public class SetupObjects : MonoBehaviour
{
    public GameObject movingObject; // 첫 번째 오브젝트
    public GameObject stationaryObject; // 두 번째 오브젝트

    void Start()
    {
        // 첫 번째 오브젝트 설정
        Rigidbody movingRb = movingObject.GetComponent<Rigidbody>();
        if (movingRb == null)
        {
            movingRb = movingObject.AddComponent<Rigidbody>();
        }
        movingRb.isKinematic = false; // 첫 번째 오브젝트는 물리적으로 움직임
        movingRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        movingRb.constraints = RigidbodyConstraints.FreezeRotation; // 회전을 제한

        // 두 번째 오브젝트 설정
        Rigidbody stationaryRb = stationaryObject.GetComponent<Rigidbody>();
        if (stationaryRb == null)
        {
            stationaryRb = stationaryObject.AddComponent<Rigidbody>();
        }
        stationaryRb.isKinematic = true; // 두 번째 오브젝트는 움직이지 않음

        // Collider 설정
        Collider movingCollider = movingObject.GetComponent<Collider>();
        if (movingCollider == null)
        {
            movingCollider = movingObject.AddComponent<BoxCollider>(); // Collider 유형은 필요에 따라 조정
        }
        movingCollider.isTrigger = false;

        Collider stationaryCollider = stationaryObject.GetComponent<Collider>();
        if (stationaryCollider == null)
        {
            stationaryCollider = stationaryObject.AddComponent<BoxCollider>(); // Collider 유형은 필요에 따라 조정
        }
        stationaryCollider.isTrigger = false;
    }
}
