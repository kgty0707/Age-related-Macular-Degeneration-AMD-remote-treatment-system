using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class DataSender : MonoBehaviour
{
    IEnumerator Start()
    {
        string url = "http://localhost:8000/create-dummy-data/";
        string jsonData = "{\"user_name\":\"John Doe\", \"ori_image_path\":\"path/to/image.jpg\", \"mask_image_path\":\"path/to/mask.jpg\", \"sclera_x\":\"100\", \"sclera_y\":\"100\", \"cornea_x\":\"50\", \"cornea_y\":\"50\", \"created_dt\":\"2023-03-26T12:34:56\"}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
    }
}