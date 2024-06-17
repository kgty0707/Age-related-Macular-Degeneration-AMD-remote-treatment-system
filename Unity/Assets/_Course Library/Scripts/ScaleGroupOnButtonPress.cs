using UnityEngine;
using UnityEngine.UI; // UI 네임스페이스 추가
using System.Collections.Generic; // List를 사용하기 위한 네임스페이스 추가

public class ScaleGroupOnButtonPress : MonoBehaviour
/*
버튼 클릭 시 오브젝트들의 크기와 위치를 변경하는 기능.
*/

{
    public float scaleIncrement = 0.1f; // 크기를 증가시킬 값
    public float maxScale = 5f; // 최대 크기 제한
    public float minScale = 0.1f; // 최소 크기 제한
    public float zScaleIncrement = 0.03f; // z축 크기를 변경할 값
    public float zPositionIncrement = 0.01f; // z축 위치를 변경할 값
    public Button zoomInButton; // Zoom In 버튼
    public Button zoomOutButton; // Zoom Out 버튼
    public List<Transform> additionalObjectsToScale; // 함께 크기를 조절할 오브젝트들
    public List<Transform> objectsToMoveY; // y축으로 이동할 오브젝트들
    public List<Transform> objectsToScaleAndMoveZ; // z축의 크기와 위치를 변경할 오브젝트들
    private float positionIncrement = 0.04f; // 위치를 변경할 값

    private SphereCollider sphereCollider; // SphereCollider 변수 추가

    void Start()
    {
        // SphereCollider 컴포넌트 가져오기
        sphereCollider = GetComponent<SphereCollider>();

        // 버튼 이벤트에 메서드 연결
        zoomInButton.onClick.AddListener(() => Zoom(true));
        zoomOutButton.onClick.AddListener(() => Zoom(false));
    }

    // Zoom 메서드: 크기와 위치를 조절
    void Zoom(bool isZoomIn)
    {
        float increment = isZoomIn ? scaleIncrement : -scaleIncrement;
        float zIncrement = isZoomIn ? zScaleIncrement : -zScaleIncrement;
        float zPosIncrement = isZoomIn ? zPositionIncrement : -zPositionIncrement;
        if (CanChangeSize(isZoomIn))
        {
            ChangeScale(increment, isZoomIn);
            MoveObjectsY(objectsToMoveY, increment, isZoomIn);
            ScaleAndMoveObjectsZ(objectsToScaleAndMoveZ, zIncrement, zPosIncrement, isZoomIn);
        }
    }

    // 크기를 변경할 수 있는지 확인하는 메서드
    bool CanChangeSize(bool isIncreasing)
    {
        // 본 오브젝트의 자식들에 대한 크기 변경 가능 여부 확인
        foreach (Transform child in transform)
        {
            if (isIncreasing && (child.localScale.x >= maxScale || child.localScale.y >= maxScale || child.localScale.z >= maxScale) ||
                !isIncreasing && (child.localScale.x <= minScale || child.localScale.y <= minScale || child.localScale.z <= minScale))
            {
                return false;
            }
        }

        // 추가된 오브젝트들에 대한 크기 변경 가능 여부 확인
        foreach (Transform obj in additionalObjectsToScale)
        {
            if (isIncreasing && (obj.localScale.x >= maxScale || obj.localScale.z >= maxScale) ||
                !isIncreasing && (obj.localScale.x <= minScale || obj.localScale.z <= minScale))
            {
                return false;
            }
        }

        return true;
    }

    // 크기를 변경하는 메서드
    void ChangeScale(float increment, bool isZoomIn)
    {
        // 본 오브젝트의 자식들에 대한 크기 변경
        foreach (Transform child in transform)
        {
            Vector3 newScale = new Vector3(
                child.localScale.x + increment,
                child.localScale.y + increment,
                child.localScale.z + increment);
            newScale = new Vector3(
                Mathf.Clamp(newScale.x, minScale, maxScale),
                Mathf.Clamp(newScale.y, minScale, maxScale),
                Mathf.Clamp(newScale.z, minScale, maxScale));
            child.localScale = newScale;
        }

        // SphereCollider의 크기 변경
        if (sphereCollider != null)
        {
            float newRadius = sphereCollider.radius + increment;
            sphereCollider.radius = Mathf.Clamp(newRadius, minScale, maxScale);
        }

        // 추가된 오브젝트들에 대한 x, z 크기 변경 및 x 위치 이동
        float positionChangeX = isZoomIn ? positionIncrement : -positionIncrement;

        foreach (Transform obj in additionalObjectsToScale)
        {
            Vector3 newScale = new Vector3(
                obj.localScale.x + increment,
                obj.localScale.y, // y는 변경하지 않음
                obj.localScale.z + increment);
            newScale = new Vector3(
                Mathf.Clamp(newScale.x, minScale, maxScale),
                obj.localScale.y, // y는 그대로 유지
                Mathf.Clamp(newScale.z, minScale, maxScale));
            obj.localScale = newScale;

            // x 위치 이동
            Vector3 newPosition = obj.localPosition;
            newPosition.x += positionChangeX;
            obj.localPosition = newPosition;
        }
    }

    // y축으로 이동할 오브젝트들의 y 위치 변경 메서드
    void MoveObjectsY(List<Transform> objects, float increment, bool isZoomIn)
    {
        float positionChangeY = isZoomIn ? positionIncrement : -positionIncrement;

        foreach (Transform obj in objects)
        {
            Vector3 newPosition = obj.localPosition;
            newPosition.y += positionChangeY;
            obj.localPosition = newPosition;
        }
    }

    // z축의 크기를 변경하고 위치를 변경할 오브젝트들의 메서드
    void ScaleAndMoveObjectsZ(List<Transform> objects, float zIncrement, float zPosIncrement, bool isZoomIn)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Transform obj = objects[i];
            Vector3 newScale = obj.localScale;
            newScale.z += zIncrement;
            newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);
            obj.localScale = newScale;

            Vector3 newPosition = obj.localPosition;
            // 짝수 인덱스는 +z축, 홀수 인덱스는 -z축으로 이동
            newPosition.z += (i % 2 == 0) ? zPosIncrement : -zPosIncrement;
            obj.localPosition = newPosition;
        }
    }
}
