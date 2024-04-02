using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class DataSender : MonoBehaviour
{
    // FastAPI 서버의 엔드포인트 URL
    private string url = "http://localhost:8000/create-item";

    // 시작 시 서버로 데이터 보내기
    void Start()
    {
        StartCoroutine(PostItem("TestItem", 1));
    }

    // POST 요청을 위한 코루틴
    IEnumerator PostItem(string name, int number)
    {
        // Item 객체를 JSON 문자열로 변환
        string jsonData = JsonUtility.ToJson(new Item { name = name, number = number });

        // UnityWebRequest 객체 생성
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 보내기 및 응답 대기
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}

// JSON 변환을 위한 Item 클래스 정의
[System.Serializable]
public class Item
{
    public string name;
    public int number;
}
