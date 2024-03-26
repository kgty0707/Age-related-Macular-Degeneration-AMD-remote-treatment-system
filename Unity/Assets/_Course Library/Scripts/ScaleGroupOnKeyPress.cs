using UnityEngine;

public class ScaleGroupOnKeyPress : MonoBehaviour
{
    public float scaleIncrement = 0.1f; // 크기를 증가시킬 값
    public float maxScale = 5f; // 최대 크기 제한
    public float minScale = 0.1f; // 최소 크기 제한

    void Update()
    {
        // 'E' 키를 눌렀을 때 크기 증가
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 최대 크기에 도달하지 않았는지 확인
            if (CanChangeSize(true))
            {
                ChangeScale(scaleIncrement);
            }
        }
        // 'W' 키를 눌렀을 때 크기 감소
        else if (Input.GetKeyDown(KeyCode.W))
        {
            // 최소 크기 이상인지 확인
            if (CanChangeSize(false))
            {
                ChangeScale(-scaleIncrement);
            }
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
            // Y축 크기는 현재 값으로 유지하면서 X축과 Z축의 크기만 변경
            Vector3 newScale = new Vector3(
                child.localScale.x + increment,
                child.localScale.y, // Y축 크기 고정
                child.localScale.z + increment);

            // 최대 및 최소 크기 제한 적용 (Y축은 변경 없음)
            newScale = new Vector3(
                Mathf.Clamp(newScale.x, minScale, maxScale),
                child.localScale.y, // Y축 값은 Clamp 적용 없이 그대로
                Mathf.Clamp(newScale.z, minScale, maxScale));

            child.localScale = newScale;
        }
    }
}
