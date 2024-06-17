using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MusicManager : MonoBehaviour
/*
배경 음악을 관리, 텔레포트가 끝났을 때 배경 음악을 멈추는 기능.
*/
{
    public AudioSource audioSource; // 오디오 소스 컴포넌트
    public AudioClip backgroundMusic; // 배경 음악 오디오 클립
    public TeleportationProvider teleportationProvider; // 텔레포트 제공자

    // 스크립트가 시작될 때 호출되는 함수
    private void Start()
    {
        Debug.Log("MusicManager Start");
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic; // 오디오 소스에 배경 음악 클립 설정
            audioSource.loop = true; // 오디오 소스를 반복 재생으로 설정
            audioSource.Play(); // 배경 음악 재생
            Debug.Log("Background music started");
        }
    }

    // 스크립트가 활성화될 때 호출되는 함수
    private void OnEnable()
    {
        Debug.Log("MusicManager OnEnable");
        if (teleportationProvider != null)
        {
            // 텔레포트가 끝날 때 호출될 이벤트 핸들러 등록
            teleportationProvider.endLocomotion += OnTeleportEnded;
        }
    }

    // 스크립트가 비활성화될 때 호출되는 함수
    private void OnDisable()
    {
        Debug.Log("MusicManager OnDisable");
        if (teleportationProvider != null)
        {
            // 텔레포트가 끝날 때 호출될 이벤트 핸들러 등록 해제
            teleportationProvider.endLocomotion -= OnTeleportEnded;
        }
    }

    // 텔레포트가 끝났을 때 호출되는 함수
    private void OnTeleportEnded(LocomotionSystem locomotionSystem)
    {
        Debug.Log("Teleportation ended");
        if (audioSource != null)
        {
            audioSource.Stop(); // 배경 음악 멈춤
            Debug.Log("Background music stopped");
        }
    }
}
