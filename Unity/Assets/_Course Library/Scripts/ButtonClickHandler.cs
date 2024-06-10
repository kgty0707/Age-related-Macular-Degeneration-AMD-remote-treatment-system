using System.Collections;
using UnityEngine;

public class ButtonClickHandler : MonoBehaviour
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
