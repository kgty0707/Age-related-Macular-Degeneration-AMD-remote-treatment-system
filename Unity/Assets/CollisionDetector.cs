using Haply.HardwareAPI.Unity;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가

public class CollisionDetector : MonoBehaviour
{
    public bool isColliding = false;
    public Vector3 CollisionPoint { get; private set; } // Store collision point
    public float HeightFactor { get; private set; } // Accessible height factor
    public float penetrationThreshold = -0.02f; // Threshold in the y-axis direction
    public string CollidingObjectTag { get; private set; } // Store the tag of the currently colliding object
    public GameObject CollidingObject { get; private set; } // Store the currently colliding object
    public Vector3 EntryDirection { get; private set; }  // Store entry direction of the collision
    private Color originalColor; // Store the original color of the object
    private Renderer collidingObjectRenderer; // Renderer component of the colliding object
    public int count = 0; // Counter for collisions with objects other than "sphere"

    public AudioClip sphereSound; // Sphere sound clip
    public AudioClip eyeSound; // Eye sound clip
    private AudioSource audioSource; // Audio source component
    public TextMeshProUGUI countText; // UI Text to display the count

    private bool sphereCollided = false; // Flag to check if sphere has collided
    private bool eyeCollided = false; // Flag to check if eye has collided
    public GameObject bloodSprayFX; // Reference to the BloodSprayFX GameObject
    public GameObject bloodSprayFX2; // Reference to the BloodSprayFX GameObject

    private void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
        UpdateCountText(); // Update the count text initially
        bloodSprayFX.SetActive(false); // Ensure BloodSprayFX is inactive at the start
        bloodSprayFX2.SetActive(false); // Ensure BloodSprayFX is inactive at the start
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger started with " + other.name);
        isColliding = true;
        CollidingObject = other.gameObject;
        CollisionPoint = other.ClosestPoint(transform.position); // Get the closest point of the trigger collider
        EntryDirection = (CollisionPoint - transform.position).normalized; // Calculate and store entry direction
        CollidingObjectTag = other.tag;
        UpdateHeightFactor(CollisionPoint.y); // Calculate HeightFactor based on collision point

        collidingObjectRenderer = CollidingObject.GetComponent<Renderer>();
        if (collidingObjectRenderer != null)
        {

            if (other.CompareTag("sphere"))
            {
                if (!sphereCollided)
                {
                    if (collidingObjectRenderer.material.color != Color.green)
                    {
                        collidingObjectRenderer.material.color = Color.green;
                    }
                    if (!audioSource.isPlaying || audioSource.clip != sphereSound)
                    {
                        audioSource.clip = sphereSound;
                        audioSource.Play();
                    }
                    sphereCollided = true; // Set flag
                }
            }

            else if (other.CompareTag("eye"))
            {
                if (!eyeCollided)
                {
                    count++;
                    UpdateCountText();
                    if (!audioSource.isPlaying || audioSource.clip != eyeSound)
                    {
                        audioSource.clip = eyeSound;
                        audioSource.Play();
                    }
                }
                if (count >= 20)
                {
                    ChangeEyeObjectsColorToRed();
                    bloodSprayFX.SetActive(true); // Activate BloodSprayFX on collision with eye
                    bloodSprayFX2.SetActive(true); // Activate BloodSprayFX on collision with eye
                }
                eyeCollided = true;
            }
                else
            {
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger ended with " + other.name);
        if (other.tag == CollidingObjectTag)
        {
            isColliding = false;
            CollidingObject = null; // Clear the colliding object on exit
            CollidingObjectTag = null; // Clear the tag on exit

            if (other.CompareTag("sphere"))
            {
                sphereCollided = false;
                collidingObjectRenderer.material.color = Color.white; // Revert to the original material on trigger exit
                collidingObjectRenderer = null; // Clear the renderer reference
            }
            else if (other.CompareTag("eye"))
            {
                eyeCollided = false;
            }
        }
        UpdateHeightFactor(transform.position.y); // Recalculate HeightFactor based on current position
    }

    private void UpdateHeightFactor(float yPosition)
    {
        // Calculate HeightFactor using the given y position
        HeightFactor = Mathf.Clamp01((yPosition - penetrationThreshold) / -penetrationThreshold);
    }
    private void UpdateCountText()
    {
        countText.text = $"틀린 횟수: {count}회"; // Update the text to show the current count
    }


    private void ChangeEyeObjectsColorToRed()
    {
        GameObject[] eyeObjects = GameObject.FindGameObjectsWithTag("eye");
        foreach (GameObject eyeObject in eyeObjects)
        {
            Renderer renderer = eyeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (renderer.material.color != Color.red) // Only change color if it is not already red
                {
                    renderer.material.color = Color.red;
                }
            }
        }
    }
}
