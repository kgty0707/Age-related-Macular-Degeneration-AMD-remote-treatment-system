using Haply.HardwareAPI.Unity;
using UnityEngine;

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
                collidingObjectRenderer.material.color = Color.green; // Change color to green if tag is "sphere"
            }
            else if (other.CompareTag("eye"))
            {
                count++;
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

            if (collidingObjectRenderer != null)
            {
                collidingObjectRenderer.material.color = Color.white; // Revert to the original material on trigger exit
                collidingObjectRenderer = null; // Clear the renderer reference
            }
        }
        UpdateHeightFactor(transform.position.y); // Recalculate HeightFactor based on current position
    }

    private void UpdateHeightFactor(float yPosition)
    {
        // Calculate HeightFactor using the given y position
        HeightFactor = Mathf.Clamp01((yPosition - penetrationThreshold) / -penetrationThreshold);
    }

    private void Update()
    {
        if (count >= 70)
        {
            ChangeEyeObjectsColorToRed();
        }
    }

    private void ChangeEyeObjectsColorToRed()
    {
        GameObject[] eyeObjects = GameObject.FindGameObjectsWithTag("eye");
        foreach (GameObject eyeObject in eyeObjects)
        {
            Renderer renderer = eyeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
    }
}
