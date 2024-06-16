using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ObjectScaler : MonoBehaviour
{
    // FastAPI 서버의 엔드포인트 URL
    string endpointUrl = "http://166.104.232.81:9876/import_image";

    // UI 버튼
    public Button requestButton;
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
        using (UnityWebRequest webRequest = UnityWebRequest.Get(endpointUrl))
        {
            // 요청 보내기
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // 응답으로 받은 JSON 데이터 출력
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                // 여기에 JSON 데이터를 파싱하고 활용하는 코드를 추가할 수 있습니다.
                string jsonData = webRequest.downloadHandler.text;
                UserData responseData = JsonUtility.FromJson<UserData>(jsonData);

                // 파싱된 데이터 활용 예시
                Debug.Log("Received width: " + responseData.width);
                Debug.Log("Received height: " + responseData.height);

                transform.localScale = new Vector3(responseData.width, responseData.height, 1);
            }
        }
    }
}

[System.Serializable]
public class UserData
{
    public string message;
    public float width;
    public float height;
}
