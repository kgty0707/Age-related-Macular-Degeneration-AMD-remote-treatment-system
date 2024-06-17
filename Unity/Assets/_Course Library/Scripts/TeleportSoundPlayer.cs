using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportSoundPlayer : MonoBehaviour
/*
텔레포트가 완료되었을 때 사운드 효과를 재생하는 기능.
*/
{
    public AudioSource audioSource; // 사운드를 재생할 AudioSource 컴포넌트
    public AudioClip teleportClip; // 텔레포트 완료 시 재생할 오디오 클립

    // 스크립트가 활성화될 때 호출되는 함수
    private void OnEnable()
    {
        // Teleportation Provider를 구하여 이벤트에 등록
        var teleportProvider = GetComponent<TeleportationProvider>();
        if (teleportProvider != null)
        {
            teleportProvider.endLocomotion += OnTeleportEnded;
        }
    }

    // 스크립트가 비활성화될 때 호출되는 함수
    private void OnDisable()
    {
        // 이벤트에서 등록 해제
        var teleportProvider = GetComponent<TeleportationProvider>();
        if (teleportProvider != null)
        {
            teleportProvider.endLocomotion -= OnTeleportEnded;
        }
    }

    // 텔레포트가 완료되었을 때 호출되는 함수
    private void OnTeleportEnded(LocomotionSystem locomotionSystem)
    {
        // 텔레포트가 완료되었을 때 효과음 재생
        if (audioSource != null && teleportClip != null)
        {
            audioSource.PlayOneShot(teleportClip);
        }
    }
}
