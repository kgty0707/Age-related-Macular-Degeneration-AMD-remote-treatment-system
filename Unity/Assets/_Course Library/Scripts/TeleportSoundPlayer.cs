using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportSoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip teleportClip;

    private void OnEnable()
    {
        // Teleportation Provider를 구하여 이벤트에 등록
        var teleportProvider = GetComponent<TeleportationProvider>();
        if (teleportProvider != null)
        {
            teleportProvider.endLocomotion += OnTeleportEnded;
        }
    }

    private void OnDisable()
    {
        // 이벤트에서 등록 해제
        var teleportProvider = GetComponent<TeleportationProvider>();
        if (teleportProvider != null)
        {
            teleportProvider.endLocomotion -= OnTeleportEnded;
        }
    }

    private void OnTeleportEnded(LocomotionSystem locomotionSystem)
    {
        // 텔레포트가 완료되었을 때 효과음 재생
        if (audioSource != null && teleportClip != null)
        {
            audioSource.PlayOneShot(teleportClip);
        }
    }
}
