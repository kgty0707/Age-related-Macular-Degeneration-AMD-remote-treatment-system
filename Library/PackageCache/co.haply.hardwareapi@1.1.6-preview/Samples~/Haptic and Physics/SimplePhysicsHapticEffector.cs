using Haply.HardwareAPI.Unity;
using UnityEngine;

/// <summary>
/// <b>EXPERIMENTAL: </b><br/>
/// This is a simplified example of using Unity's physics engine to implement haptics feedback in a complex scene
///
/// <para>
/// The following two effectors are being used in the scene:
/// <list type="bullet">
///     <item>The Cursor (kinematic) directly controlled by the device on each haptic frame</item>
///     <item>The PhysicEffector (non-kinematic) with colliders linked to the Cursor by a fixed joint</item>
/// </list>
///
/// The haptic force is relative to the distance between the two effectors such that when the physics effector is blocked by
/// an object in the scene, an opposing force proportional to the distance will be generated.
/// </para>
///
/// <remarks>
/// The haptics feeling of drag or friction that occurs on moving effector is caused by the difference in update frequency between
/// Unity's physics engine (between 60Hz to 120Hz) and the haptics thread (~1000Hz). This difference means that the physics
/// effector will always be lagging behind the Cursor's true position which leads to forces that resemble a step function
/// instead of having continuous forces.
///
/// <para>Solutions :</para>
/// <list type="bullet">
///     <item>Decrease the value of ProjectSettings.FixedTimestep as close to 0.001 as possible which will have
///     significant impact on performances for complex scenes.</item>
///     <item>Apply forces only when collisions occur (see <see cref="AdvancedPhysicsHapticEffector"/>)</item>
///     <item>Use a third-party physic/haptic engine like (TOIA, SOFA, etc...) as a middleware between the two physics
///     engine to simulate the contact points.</item>
/// </list>
/// </remarks>
/// 
/// </summary>
public class SimplePhysicsHapticEffector : MonoBehaviour
{
    /// <summary>
    /// Safe thread scene data
    /// </summary>
    private struct AdditionalData
    {
        public Vector3 physicsCursorPosition;
    }

    [Tooltip("Enable/Disable force feedback")]
    public bool forceEnabled;
    
    [Range(0, 800)]
    public float stiffness = 400f;
    [Range(0, 3)]
    public float damping = 1;
    
    private HapticThread m_hapticThread;

    private void Awake ()
    {
        // find the HapticThread object before the first FixedUpdate() call
        m_hapticThread = FindObjectOfType<HapticThread>();

        // create the physics link between physic effector and device cursor
        AttachCursor( m_hapticThread.avatar.gameObject );
    }
    
    private void OnEnable ()
    {
        // Run haptic loop with AdditionalData method to get initial values
        if (m_hapticThread.isInitialized)
            m_hapticThread.Run(ForceCalculation, GetAdditionalData());
        else
            m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, GetAdditionalData()) );
    }

    private void FixedUpdate () =>
        // Update AdditionalData 
        m_hapticThread.SetAdditionalData( GetAdditionalData() );

    /// <summary>
    /// Attach the current physics effector to device end-effector with a joint
    /// </summary>
    /// <param name="cursor">Cursor to attach with</param>
    private void AttachCursor (GameObject cursor)
    {
        // add kinematic rigidbody to cursor
        var rbCursor = cursor.GetComponent<Rigidbody>();
        if ( !rbCursor )
        {
            rbCursor = cursor.AddComponent<Rigidbody>();
            rbCursor.useGravity = false;
            rbCursor.isKinematic = true;
        }
        
        // add non-kinematic rigidbody to self
        if ( !gameObject.GetComponent<Rigidbody>() )
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }
        
        // connect with cursor rigidbody
        if ( !gameObject.GetComponent<FixedJoint>() )
        {
            var joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rbCursor;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = joint.connectedAnchor = Vector3.zero;
            joint.axis = Vector3.zero;
        }
    }

    /// <summary>
    /// Method used by <see cref="HapticThread.Run(Haply.HardwareAPI.Unity.ForceCalculation1)"/>
    /// and <see cref="HapticThread.GetAdditionalData{T}"/>
    /// to transfer dynamic data between the unity scene and the haptic thread 
    /// </summary>
    /// <returns>Updated AdditionalData struct</returns>
    private AdditionalData GetAdditionalData ()
    {
        AdditionalData additionalData;
        additionalData.physicsCursorPosition = transform.localPosition;
        return additionalData;
    }

    /// <summary>
    /// Calculate the force to apply based on the cursor position and the scene data
    /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
    /// </summary>
    /// <param name="position">cursor position</param>
    /// <param name="velocity">cursor velocity</param>
    /// <param name="additionalData">additional scene data synchronized by <see cref="GetAdditionalData"/> method</param>
    /// <returns>Force to apply</returns>
    private Vector3 ForceCalculation ( in Vector3 position, in Vector3 velocity, in AdditionalData additionalData )
    {
        if ( !forceEnabled )
        {
            return Vector3.zero;
        }
        var force = additionalData.physicsCursorPosition - position;
        force *= stiffness;
        force -= velocity * damping;
        return force;
    }
}
