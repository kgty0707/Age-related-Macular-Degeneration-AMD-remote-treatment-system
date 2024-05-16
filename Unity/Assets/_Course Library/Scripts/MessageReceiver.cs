using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MessageReceiver : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetMessageCoroutine("http://localhost:8000/get-message/"));
    }

    IEnumerator GetMessageCoroutine(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // 요청 보내기
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                // 응답 받기
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }
}
