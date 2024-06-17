using System.Collections;
using UnityEngine;

public class ButtonClickHandler : MonoBehaviour
/*
코루틴을 설정하여 버튼 클릭 시 오디오 클립을 재생하고, 
오디오 클립이 재생된 후 UI를 비활성화하는 기능
*/
{
    public AudioSource audioSource;
    public AudioClip buttonClip;
    public GameObject introUI;

    public void OnButtonClick()
    {
        // Play the audio clip
        audioSource.PlayOneShot(buttonClip);

        // Start the coroutine to disable the UI after the clip has finished
        StartCoroutine(DisableUIAfterSound(buttonClip.length));
    }

    private IEnumerator DisableUIAfterSound(float clipLength)
    {
        // Wait for the audio clip to finish playing
        yield return new WaitForSeconds(clipLength);

        // Disable the UI object
        introUI.SetActive(false);
    }
}
