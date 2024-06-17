using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ObjectScaler : MonoBehaviour
/*
서버에서 데이터를 가져와 객체의 크기를 조정하는 기능.
요청 버튼을 클릭하면 서버로부터 데이터를 요청하고, 응답으로 받은 데이터에 따라 객체의 크기를 변경.
*/
{
    // FastAPI 서버의 엔드포인트 URL
    string endpointUrl = "http://166.104.232.81:9876/import_image";

    // UI 버튼
    public Button requestButton; // 서버 요청을 보낼 버튼
    public List<Button> otherButtons; // 다른 버튼들을 관리하는 리스트

    void Start()
    {
        // 다른 버튼들을 비활성화
        foreach (Button button in otherButtons)
        {
            button.interactable = false;
        }

        // 요청 버튼 클릭 이벤트에 메서드 연결
        requestButton.onClick.AddListener(OnRequestButtonClick);
    }

    // 요청 버튼 클릭 시 호출될 메서드
    void OnRequestButtonClick()
    {
        // 서버로부터 데이터를 받아오는 코루틴 시작
        StartCoroutine(GetDataFromServer());

        // 요청 버튼 비활성화
        requestButton.interactable = false;

        // 다른 버튼들을 활성화
        foreach (Button button in otherButtons)
        {
            button.interactable = true;
        }
    }

    // 서버로부터 데이터를 받아오는 코루틴
    IEnumerator GetDataFromServer()
    {
        // GET 요청을 보내기 위해 UnityWebRequest 객체 생성
        using (UnityWebRequest webRequest = UnityWebRequest.Get(endpointUrl))
        {
            // 요청 보내기 및 응답 대기
            yield return webRequest.SendWebRequest();

            // 요청 결과 확인
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                // 요청이 실패한 경우 에러 메시지 출력
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // 요청이 성공한 경우 서버로부터 받은 JSON 데이터 출력
                Debug.Log("Received: " + webRequest.downloadHandler.text);

                // JSON 데이터를 파싱하고 활용
                string jsonData = webRequest.downloadHandler.text;
                UserData responseData = JsonUtility.FromJson<UserData>(jsonData);

                // 파싱된 데이터 활용 예시
                Debug.Log("Received width: " + responseData.width);
                Debug.Log("Received height: " + responseData.height);

                // 객체의 크기를 서버로부터 받은 데이터에 따라 조정
                transform.localScale = new Vector3(responseData.width, responseData.height, 1);
            }
        }
    }
}

// 서버 응답 데이터 구조를 나타내는 클래스
[System.Serializable]
public class UserData
{
    public string message; // 응답 메시지
    public float width; // 객체의 너비
    public float height; // 객체의 높이
}
