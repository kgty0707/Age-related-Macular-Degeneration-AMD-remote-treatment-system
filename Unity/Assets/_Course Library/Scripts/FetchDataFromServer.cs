using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FetchDataFromServer : MonoBehaviour
/*
FastAPI 서버로부터 데이터를 가져오는 기능을 담당.
서버로 GET 요청을 보내고, 응답으로 받은 데이터 처리.
*/
{
    // FastAPI 서버의 엔드포인트 URL
    string endpointUrl = "http://166.104.232.81:9876/results/latest";

    // 시작 시 서버로부터 데이터 요청
    void Start()
    {
        // 서버로부터 데이터를 받아오는 코루틴 시작
        StartCoroutine(GetDataFromServer());
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
                // 요청이 성공한 경우 응답으로 받은 JSON 데이터 출력
                Debug.Log("Received: " + webRequest.downloadHandler.text);
            }
        }
    }
}
