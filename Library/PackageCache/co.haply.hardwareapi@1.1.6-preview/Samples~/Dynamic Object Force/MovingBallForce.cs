using Haply.HardwareAPI.Unity;
using UnityEngine;

/// <summary>
/// Because the haptic thread can be thousand more faster than physics, multiple haptic loop calls 
/// can occur during one `FixedUpdate()` call, and scene data used for force calculation can be 
/// in inconsistent state.
/// 
/// So this example shows a thread safe way to synchronize dynamic scene data with the haptic loop.
/// </summary>
public class MovingBallForce : MonoBehaviour
{
    /// <summary>
    /// Safe thread scene data
    /// </summary>
    private struct AdditionalData
    {
        public Vector3 ballPosition;
        public float ballRadius;
    }
    
    [Range(0, 800)]
    public float stiffness = 600f;
    
    [Tooltip("moving/scaling speed (by pressing arrow keys)")]
    public float speed = 0.2f;
    
    private HapticThread m_hapticThread;
    private float m_cursorRadius;

    private void Awake ()
    {
        // find the HapticThread object before the first FixedUpdate() call
        m_hapticThread = FindObjectOfType<HapticThread>();
        
        m_cursorRadius = m_hapticThread.avatar.localScale.x / 2f;
        
        // Run the haptic loop with an initial state returned by AdditionalData.
        var initialState = GetAdditionalData();
        m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, initialState));
    }

    private void FixedUpdate ()
    {
        // change ball scale
        if ( Input.GetKey( KeyCode.UpArrow ) )
            transform.localScale += Vector3.one * (Time.fixedDeltaTime * speed);
        else if ( Input.GetKey( KeyCode.DownArrow ) )
            transform.localScale -= Vector3.one * (Time.fixedDeltaTime * speed);

        // move ball
        if ( Input.GetKey( KeyCode.LeftArrow ) )
            transform.transform.position += Vector3.left * (Time.fixedDeltaTime * speed);
        else if (Input.GetKey(KeyCode.RightArrow))
            transform.transform.position += Vector3.right * (Time.fixedDeltaTime * speed);

        // update AdditionalData 
        var latestData = GetAdditionalData();
        m_hapticThread.SetAdditionalData( latestData );
    }

    /// <summary>
    /// Method used by <see cref="HapticThread.Run(Haply.HardwareAPI.Unity.ForceCalculation1)"/>
    /// and <see cref="HapticThread.GetAdditionalData{T}"/>
    /// to synchronize dynamic data between the unity scene and the haptic thread 
    /// </summary>
    /// <returns>Updated AdditionalData struct</returns>
    private AdditionalData GetAdditionalData ()
    {
        AdditionalData additionalData;

        additionalData.ballPosition = transform.localPosition;
        additionalData.ballRadius = transform.localScale.x / 2f;

        return additionalData;
    }

    /// <summary>
    /// Calculate the force to apply based on the cursor position, velocity and the scene data
    /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
    /// </summary>
    /// <param name="position">cursor position</param>
    /// <param name="velocity">cursor velocity</param>
    /// <param name="additionalData">additional scene data synchronized by <see cref="GetAdditionalData"/> method</param>
    /// <returns>Force to apply</returns>
    private Vector3 ForceCalculation ( in Vector3 position, in Vector3 velocity, in AdditionalData additionalData )
    {
        var force = Vector3.zero;

        var distance = Vector3.Distance( position, additionalData.ballPosition );
        var penetration = additionalData.ballRadius + m_cursorRadius - distance;
        if ( penetration > 0 )
        {
            force = (position - additionalData.ballPosition) / distance * penetration * stiffness;
        }

        return force;
    }
}