using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FetchDataFromServer : MonoBehaviour
{
    // FastAPI 서버의 엔드포인트 URL
    string endpointUrl = "http://166.104.232.81:9876/results/latest";

    // 시작 시 서버로부터 데이터 요청
    void Start()
    {
        StartCoroutine(GetDataFromServer());
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
            }
        }
    }
}
