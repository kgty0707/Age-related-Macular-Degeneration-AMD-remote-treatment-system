using System;
using Haply.HardwareAPI.Unity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>EXPERIMENTAL: </b><br/>
/// This is a more advanced example of using Unity's physics engine to implement haptics feedback in a complex scene.
///
/// <para>
/// The following two effectors are being used in the scene:
/// </para>
/// <list type="bullet">
///     <item>The Cursor (kinematic) directly controlled by the device on each haptic frame</item>
///     <item>The PhysicEffector (non-kinematic) with colliders linked to the Cursor by a joint
/// attached to a sprint and damper configured with a large constant</item>
/// </list>
///
/// The haptic force is relative to the distance between the two effectors such that when the physics effector is blocked by
/// an object in the scene, an opposing force proportional to the distance will be generated.
///
/// <para>
/// Thanks to the spring/damper, movable objects mass can be felt and Unity Physics Materials on scene objects can be used
/// to have different haptic feelings.
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
///     <item>Apply forces only when collisions occur (see <see cref="collisionDetection"/>)</item>
///     <item>Use a third-party physic/haptic engine like (TOIA, SOFA, etc...) as a middleware between the two physics
///     engine to simulate the contact points.</item>
/// </list>
/// </remarks>
/// 
/// </summary>
public class AdvancedPhysicsHapticEffector : MonoBehaviour
{
    /// <summary>
    /// Safe thread scene data
    /// </summary>
    private struct AdditionalData
    {
        public Vector3 physicsCursorPosition;
        public bool isTouching;
    }
    
    // HAPTICS
    [Header("Haptics")]
    [Tooltip("Enable/Disable force feedback")]
    public bool forceEnabled;
    
    [Range(0, 800)]
    public float stiffness = 400f;
    [Range(0, 3)]
    public float damping = 1;
    
    private HapticThread m_hapticThread;
    
    // PHYSICS
    [Header("Physics")]
    [Tooltip("Use it to enable friction and mass force feeling")]
    public bool complexJoint;
    public float drag = 20f;
    public float linearLimit = 0.001f;
    public float limitSpring = 500000f;
    public float limitDamper = 10000f;

    private ConfigurableJoint m_joint;
    private Rigidbody m_rigidbody;

    private const float MinimumReconfigureDelta = 0.5f;
    private bool needConfigure =>
        (complexJoint && m_joint.zMotion != ConfigurableJointMotion.Limited)
        || Mathf.Abs(m_joint.linearLimit.limit - linearLimit) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.spring - limitSpring) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.damper - limitDamper) > MinimumReconfigureDelta
        || Mathf.Abs(m_rigidbody.drag - drag) > MinimumReconfigureDelta;

    [Header("Collision detection")]
    [Tooltip("Apply force only when a collision is detected (prevent air friction feeling)")]
    public bool collisionDetection;
    public List<Collider> touched = new();

    private void Awake ()
    {
        // find the HapticThread object before the first FixedUpdate() call
        m_hapticThread = FindObjectOfType<HapticThread>();

        // create the physics link between physic effector and device cursor
        AttachCursor( m_hapticThread.avatar.gameObject );
        SetupCollisionDetection();
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
    
    private void Update()
    {
#if UNITY_EDITOR
        if (needConfigure)
        {
            ConfigureJoint();
        }
#endif
    }

    //PHYSICS
    #region Physics Joint

    /// <summary>
    /// Attach the current physics effector to device end-effector with a joint
    /// </summary>
    /// <param name="cursor">Cursor to attach with</param>
    private void AttachCursor (GameObject cursor)
    {
        // Add kinematic rigidbody to cursor
        var rbCursor = cursor.GetComponent<Rigidbody>();
        if ( !rbCursor )
        {
            rbCursor = cursor.AddComponent<Rigidbody>();
            rbCursor.useGravity = false;
            rbCursor.isKinematic = true;
        }
        
        // Add non-kinematic rigidbody to self
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
        if ( !m_rigidbody )
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.useGravity = false;
            m_rigidbody.isKinematic = false;
            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Connect with cursor rigidbody with a spring/damper joint and locked rotation
        m_joint = gameObject.GetComponent<ConfigurableJoint>();
        if ( !m_joint )
        {
            m_joint = gameObject.AddComponent<ConfigurableJoint>();
            m_joint.connectedBody = rbCursor;
            m_joint.autoConfigureConnectedAnchor = false;
            m_joint.anchor = m_joint.connectedAnchor = Vector3.zero;
            m_joint.axis = m_joint.secondaryAxis = Vector3.zero;
        }
        
        ConfigureJoint();
        
    }

    private void ConfigureJoint()
    {
        if (!complexJoint)
        {
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Locked;
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;
            
            m_rigidbody.drag = 0;
        }
        else
        {
            // limited linear movements
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Limited;
            
            // lock rotation to avoid sphere roll caused by physics material friction instead of feel it
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;

            // configure limit, spring and damper
            m_joint.linearLimit = new SoftJointLimit()
            {
                limit = linearLimit
            };
            m_joint.linearLimitSpring = new SoftJointLimitSpring()
            {
                spring = limitSpring,
                damper = limitDamper
            };
            
            // stabilize spring connection 
            m_rigidbody.drag = drag;
        }
    }

    #endregion

    // HAPTICS
    #region Haptics
    
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
        additionalData.isTouching = collisionDetection && touched.Count > 0;
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
        if ( !forceEnabled || (collisionDetection && !additionalData.isTouching) )
        {
            // Don't compute forces if there are no collisions which prevents feeling drag/friction while moving through air. 
            return Vector3.zero;
        }
        var force = additionalData.physicsCursorPosition - position;
        force *= stiffness;
        force -= velocity * damping;
        return force;
    }
    
    #endregion

    // COLLISION DETECTION
    #region Collision Detection

    private void SetupCollisionDetection()
    {
        // Add collider if not exists
        var col = gameObject.GetComponent<Collider>();
        if ( !col )
        {
            col = gameObject.AddComponent<SphereCollider>();
        }

        // Neutral PhysicMaterial to interact with others 
        if ( !col.material )
        {
            col.material = new PhysicMaterial {dynamicFriction = 0, staticFriction = 0};
        }

        collisionDetection = true;
    }

    /// <summary>
    /// Called when effector touch other game object
    /// </summary>
    /// <param name="collision">collision information</param>
    private void OnCollisionEnter ( Collision collision )
    {
        if ( forceEnabled && collisionDetection && !touched.Contains( collision.collider ) )
        {
            // store touched object
            touched.Add( collision.collider );
        }
    }

    /// <summary>
    /// Called when effector move away from another game object 
    /// </summary>
    /// <param name="collision">collision information</param>
    private void OnCollisionExit ( Collision collision )
    {
        if ( forceEnabled && collisionDetection && touched.Contains( collision.collider ) )
        {
            touched.Remove( collision.collider );
        }
    }
    
    #endregion
}
