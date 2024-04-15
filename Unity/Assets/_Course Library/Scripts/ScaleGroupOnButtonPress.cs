using UnityEngine;
using UnityEngine.UI; // UI 네임스페이스 추가

public class ScaleGroupOnButtonPress : MonoBehaviour
{
    public float scaleIncrement = 0.1f; // 크기를 증가시킬 값
    public float maxScale = 5f; // 최대 크기 제한
    public float minScale = 0.1f; // 최소 크기 제한
    public Button zoomInButton; // Zoom In 버튼
    public Button zoomOutButton; // Zoom Out 버튼

    void Start()
    {
        // 버튼 이벤트에 메서드 연결
        zoomInButton.onClick.AddListener(ZoomIn);
        zoomOutButton.onClick.AddListener(ZoomOut);
    }

    // Zoom In 메서드
    void ZoomIn()
    {
        if (CanChangeSize(true))
        {
            ChangeScale(scaleIncrement);
        }
    }

    // Zoom Out 메서드
    void ZoomOut()
    {
        if (CanChangeSize(false))
        {
            ChangeScale(-scaleIncrement);
        }
    }

    // 크기를 변경할 수 있는지 확인하는 메서드
    bool CanChangeSize(bool isIncreasing)
    {
        foreach (Transform child in transform)
        {
            if (isIncreasing && (child.localScale.x >= maxScale || child.localScale.y >= maxScale || child.localScale.z >= maxScale) ||
                !isIncreasing && (child.localScale.x <= minScale || child.localScale.y <= minScale || child.localScale.z <= minScale))
            {
                return false;
            }
        }
        return true;
    }

    // 크기를 변경하는 메서드
    void ChangeScale(float increment)
    {
        foreach (Transform child in transform)
        {
            Vector3 newScale = new Vector3(
                child.localScale.x + increment,
                child.localScale.y + increment,
                child.localScale.z);
            newScale = new Vector3(
                Mathf.Clamp(newScale.x, minScale, maxScale),
                Mathf.Clamp(newScale.y, minScale, maxScale),
                child.localScale.z);
            child.localScale = newScale;
        }
    }
}
