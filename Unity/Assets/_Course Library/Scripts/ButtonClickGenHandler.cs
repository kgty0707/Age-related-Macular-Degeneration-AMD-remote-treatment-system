using System.Collections;
using UnityEngine;

public class ButtonClickGenHandler : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip buttonClip;
    public GameObject introUI;
    public GameObject newUI; // 새롭게 나타날 UI

    public void OnButtonClick()
    {
        // Play the audio clip
        audioSource.PlayOneShot(buttonClip);

        // Start the coroutine to disable the intro UI and enable the new UI after the clip has finished
        StartCoroutine(SwitchUIAfterSound(buttonClip.length));
    }

    private IEnumerator SwitchUIAfterSound(float clipLength)
    {
        // Wait for the audio clip to finish playing
        yield return new WaitForSeconds(clipLength);

        // Disable the intro UI object
        introUI.SetActive(false);

        // Enable the new UI object
        newUI.SetActive(true);
    }
}
