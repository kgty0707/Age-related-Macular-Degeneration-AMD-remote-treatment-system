using System;
using Haply.HardwareAPI.Unity;
using UnityEngine;

namespace Haply.Samples.BasicForceFeedbackAndWorkspaceControl
{
    /// <summary>
    /// This component is used to control the scale of the associated GameObject relative to the
    /// rotation of the HandleThread's avatar along the Z-axis. When the <see cref="active"/>
    /// property is true, the cursor's orientation is applied to the current workspace's scale using
    /// the specified <see cref="scalingFactor"/>. This allows for scaling down to increase cursor
    /// precision or scaling up to expand the workspace range.<br/>
    ///
    /// <b>Suggestion:</b><br/>
    /// Bind <see cref="active"/> properties to HandleThread Unity Events <b>OnButtonDown</b> and
    /// <b>OnButtonUp</b> by the inspector or by script as following:
    /// <code>
    /// handleThread.onButtonDown.AddListener(() => active = true);
    /// handleThread.onButtonUp.AddListener(() => active = false);
    /// </code>
    /// </summary>
    [RequireComponent(typeof(HandleThread))]
    public class WorkspaceScaleController : MonoBehaviour
    {
        // Movable cursor where the rotation is controlled by Handle Thread (must be a child of
        // current GameObject)
        private Transform m_cursor;

        [Tooltip("Sensitivity of handle's rotation scaling")]
        [Range(0, 5)]
        public float scalingFactor = 3f;
        [Range(0, 10)]
        public float minimumScale = 1f;
        [Range(1, 10)]
        public float maximumScale = 5f;
        
        // Saved workspace scale and cursor orientation prior to any modification
        private float m_baseScale;
        private float m_cursorBaseAngle;
        private float m_cursorPreviousAngle;
        private int m_rotationCount;
        
        /// <summary>
        /// If enabled the workspace will be uniformly scaled relatively to cursor roll (Z-axis
        /// rotation)
        /// </summary>
        public bool active
        {
            get => m_active;
            set
            {
                if (value)
                {
                    m_rotationCount = 0;
                    m_baseScale = transform.localScale.z;
                    m_cursorPreviousAngle = m_cursorBaseAngle = m_cursor.localEulerAngles.z;
                }
                m_active = value;
            }
        }
        private bool m_active;
        
        private void Awake()
        {
            // Get the rotating cursor from the HandleThread
            m_cursor = GetComponent<HandleThread>().avatar;
            
            // Check if cursor is a child of the cursor offset
            if (!m_cursor.IsChildOf(transform))
                Debug.LogError($"Cursor '{m_cursor.name}' must be a child of '{name}'", gameObject);
            if (minimumScale >= maximumScale)
                throw new ArgumentException($"maximumScale ({maximumScale}) must be greater than" +
                                            $"minimumScale ({minimumScale})");
        }

        private void Update()
        {
            if (active)
            {
                // Calculate scale relative to cursor roll on Z-axis rotation
                var totalDegrees = GetTotalDegrees(m_cursor.localEulerAngles.z, m_cursorBaseAngle);
                var scale = m_baseScale - totalDegrees * scalingFactor / 100f;
                
                // Limit between minimumScale and maximumScale
                scale = Mathf.Clamp(scale, minimumScale, maximumScale);

                // Set cursor offset scale (same on each axis)
                transform.localScale = Vector3.one * scale;
                // Invert cursor scale to keep its original size
                m_cursor.localScale = Vector3.one / scale;
            }
        }

        /// <summary>
        /// Get total degrees between <paramref name="baseAngle"/> and
        /// <paramref name="currentAngle"/> over the 360 degrees limitation.
        /// </summary>
        /// <param name="currentAngle">Moving angle to compare with <paramref name="baseAngle"/>
        /// </param>
        /// <param name="baseAngle">Static reference angle</param>
        /// <returns>Total degrees between <paramref name="baseAngle"/> and
        /// <paramref name="currentAngle"/> over the 360
        /// degrees limitation.</returns>
        /// <remarks><see cref="m_cursorPreviousAngle"/> is used to compare the
        /// <see cref="currentAngle"/> from previous call to detect when the angle goes under 0 or
        /// over 360 degrees to update <see cref="m_rotationCount"/>
        /// </remarks>
        private float GetTotalDegrees(float currentAngle, float baseAngle)
        {
            if (currentAngle - m_cursorPreviousAngle > 330)
                m_rotationCount--;
            else if (m_cursorPreviousAngle - currentAngle > 330)
                m_rotationCount++;
            
            m_cursorPreviousAngle = currentAngle;
            
            return 360f * m_rotationCount + (currentAngle - baseAngle);
        }
    }
}