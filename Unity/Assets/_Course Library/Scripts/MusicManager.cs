using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip backgroundMusic;
    public TeleportationProvider teleportationProvider;

    private void Start()
    {
        Debug.Log("MusicManager Start");
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Background music started");
        }
    }

    private void OnEnable()
    {
        Debug.Log("MusicManager OnEnable");
        if (teleportationProvider != null)
        {
            teleportationProvider.endLocomotion += OnTeleportEnded;
        }
    }

    private void OnDisable()
    {
        Debug.Log("MusicManager OnDisable");
        if (teleportationProvider != null)
        {
            teleportationProvider.endLocomotion -= OnTeleportEnded;
        }
    }

    private void OnTeleportEnded(LocomotionSystem locomotionSystem)
    {
        Debug.Log("Teleportation ended");
        if (audioSource != null)
        {
            audioSource.Stop();
            Debug.Log("Background music stopped");
        }
    }
}
