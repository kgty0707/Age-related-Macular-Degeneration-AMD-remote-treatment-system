using System.Collections;
using UnityEngine;

public class ButtonClickGenHandler : MonoBehaviour
/*
코루틴을 설정하여 버튼 클릭 시 오디오 클립을 재생하고, 
오디오 클립이 끝난 후 UI를 전환하는 기능
*/
{
    public AudioSource audioSource; // 오디오 소스 컴포넌트
    public AudioClip buttonClip; // 버튼 클릭 시 재생할 오디오 클립
    public GameObject introUI; // 초기 UI 오브젝트
    public GameObject newUI; // 새롭게 나타날 UI 오브젝트

    // 버튼 클릭 시 호출되는 함수
    public void OnButtonClick()
    {
        // 오디오 클립 재생
        audioSource.PlayOneShot(buttonClip);

        // 오디오 클립이 끝난 후 UI를 전환하는 코루틴 시작
        StartCoroutine(SwitchUIAfterSound(buttonClip.length));
    }

    // 오디오 클립 재생이 끝난 후 UI를 전환하는 코루틴
    private IEnumerator SwitchUIAfterSound(float clipLength)
    {
        // 오디오 클립 재생이 끝날 때까지 대기
        yield return new WaitForSeconds(clipLength);

        // 초기 UI 오브젝트 비활성화
        introUI.SetActive(false);

        // 새로운 UI 오브젝트 활성화
        newUI.SetActive(true);
    }
}
