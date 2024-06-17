using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MessageReceiver : MonoBehaviour
/*
FastAPI URL로 GET 요청을 보내고,
서버로부터 메시지를 받아오는 기능.
*/
{
    // 스크립트가 시작될 때 호출되는 함수
    void Start()
    {
        // 서버로부터 메시지를 받아오는 코루틴 시작
        StartCoroutine(GetMessageCoroutine("http://166.104.232.81:9876/get-message/"));
    }

    // 서버로부터 메시지를 받아오는 코루틴
    IEnumerator GetMessageCoroutine(string uri)
    {
        // GET 요청을 보내기 위해 UnityWebRequest 객체 생성
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // 요청 보내기 및 응답 대기
            yield return webRequest.SendWebRequest();

            // 요청 결과 확인
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // 요청이 실패한 경우 에러 메시지 출력
                Debug.LogError(webRequest.error);
            }
            else
            {
                // 요청이 성공한 경우 서버로부터 받은 메시지 출력
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }
}
