using UnityEngine;

public class ColliderDetect : MonoBehaviour
{
    private Color originalColor; // Store the original color of the object
    private Renderer objectRenderer; // Renderer component of the object

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color; // Save the original color
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision started with " + collision.collider.name);
        if (objectRenderer != null)
        {
            ChangeObjectColor(objectRenderer, Color.red); // Change color to red on collision
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collision ended with " + collision.collider.name);
        if (objectRenderer != null)
        {
            ChangeObjectColor(objectRenderer, originalColor); // Revert to the original color on collision exit
        }
    }

    private void ChangeObjectColor(Renderer renderer, Color color)
    {
        renderer.material.color = color;
    }
}
