using Haply.HardwareAPI.Unity;
using UnityEngine;

namespace Haply.Samples.BasicForceFeedbackAndWorkspaceControl
{
    public class GroundForce : MonoBehaviour
    {
        [Range(0, 800)]
        public float stiffness = 600f;
        [Range(0, 3)]
        public float damping = 1;
        
        public Transform ground;

        private HapticThread m_hapticThread;
        
        private float m_groundHeight;
        private float m_cursorRadius;

        // Workspace
        private float m_workspaceScale = 1;
        private float m_workspaceHeight;

        private void Start ()
        {
            // find the HapticThread object
            m_hapticThread = GetComponent<HapticThread>();
            
            StoreTransformInfos();

            // run the haptic loop with given function
            m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run( ForceCalculation ));
        }

        /// <summary>
        /// store all transform information which cannot be acceded in haptic tread loop
        ///
        /// <remarks>Do not use this method for dynamic objects in Update() or FixedUpdate() except for debug in editor
        /// (prefer <see cref="HapticThread.GetAdditionalData{T}"/>)</remarks>
        /// </summary>
        private void StoreTransformInfos ()
        {
            m_groundHeight = ground.transform.position.y;
            m_cursorRadius = m_hapticThread.avatar.lossyScale.y / 2;
            
            var workspace = m_hapticThread.avatar.parent;
            m_workspaceScale = workspace.lossyScale.y;
            m_workspaceHeight = workspace.position.y;
        }
        
    #if UNITY_EDITOR
        private void Update () => StoreTransformInfos();
    #endif

        /// <summary>
        /// Calculate force to apply when the cursor hit the ground.
        /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
        /// </summary>
        /// <param name="position">cursor position</param>
        /// <param name="velocity">cursor velocity (optional)</param>
        /// <returns>Force to apply</returns>
        private Vector3 ForceCalculation ( in Vector3 position, in Vector3 velocity )
        {
            var force = Vector3.zero;

            // Contact point scaled by parent offset
            var contactPoint = (position.y * m_workspaceScale) + m_workspaceHeight - m_cursorRadius;
            
            var penetration = m_groundHeight - contactPoint;
            if ( penetration > 0 )
            {
                force.y = penetration * stiffness - velocity.y * damping;
                
                // invert the offset scale to avoid stiffness relative to it
                force.y /= m_workspaceScale;
            }
            
            return force;
        }
    }
}