using UnityEngine;

public class ColliderDetect : MonoBehaviour
/*
오브젝트가 충돌할 때 색상을 변경하는 기능.
충돌이 시작되면 오브젝트의 색상을 빨간색으로 변경하고,
충돌이 끝나면 원래 색상으로 되돌림.
*/
{
    private Color originalColor; // 오브젝트의 원래 색상을 저장
    private Renderer objectRenderer; // 오브젝트의 Renderer 컴포넌트

    private void Start()
    {
        // 오브젝트의 Renderer 컴포넌트 가져오기
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // 원래 색상 저장
            originalColor = objectRenderer.material.color;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌 시작 시 로그 출력
        Debug.Log("Collision started with " + collision.collider.name);
        if (objectRenderer != null)
        {
            // 충돌 시 오브젝트의 색상을 빨간색으로 변경
            ChangeObjectColor(objectRenderer, Color.red);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 충돌 종료 시 로그 출력
        Debug.Log("Collision ended with " + collision.collider.name);
        if (objectRenderer != null)
        {
            // 충돌 종료 시 오브젝트의 색상을 원래 색상으로 되돌림
            ChangeObjectColor(objectRenderer, originalColor);
        }
    }

    // 오브젝트의 색상을 변경하는 함수
    private void ChangeObjectColor(Renderer renderer, Color color)
    {
        renderer.material.color = color;
    }
}
