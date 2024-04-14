using Haply.HardwareAPI.Unity;
using UnityEngine;

namespace Haply.Samples.BasicForceFeedbackAndWorkspaceControl
{
    /// <summary>
    /// This component is used to control the position of the associated GameObject based on
    /// HapticThread's cursor movement. When the <see cref="active"/> property is true, the cursor
    /// movement is applied to
    /// the current workspace to create an offset that allows for moving its reference frame.<br/>
    ///
    /// <b>Suggestion:</b><br/>
    /// Bind <see cref="active"/> properties to HandleThread Unity Events <b>OnButtonDown</b> and
    /// <b>OnButtonUp</b> by the inspector or by script as following:
    /// <code>
    /// handleThread.onButtonDown.AddListener(() => active = true);
    /// handleThread.onButtonUp.AddListener(() => active = false);
    /// </code>
    /// </summary>
    [RequireComponent(typeof(HapticThread))]
    public class WorkspaceOffsetController : MonoBehaviour
    {
        // Movable cursor with position controlled by Haptic Thread (must be a child of current
        // GameObject)
        private Transform m_cursor;

        // Saved workspace and cursor positions prior to any modification
        private Vector3 m_basePosition;
        private Vector3 m_cursorBasePosition;

        /// <summary>
        /// Enables the workspace offsetting relatively to the cursor position on each update
        /// </summary>
        public bool active
        {
            get => m_active;
            set
            {
                if (value)
                {
                    m_basePosition = transform.localPosition;
                    m_cursorBasePosition = m_cursor.localPosition;
                }

                m_active = value;
            }
        }

        private bool m_active;

        private void Awake()
        {
            // Get the moving cursor from the HapticThread
            m_cursor = GetComponent<HapticThread>().avatar;

            // Check if cursor is a child of the cursor offset
            if (!m_cursor.IsChildOf(transform))
                Debug.LogError($"Cursor '{m_cursor.name}' must be a child of '{name}'", gameObject);
        }

        private void Update()
        {
            if (active)
            {
                // Move cursor offset relative to cursor position 
                transform.position = m_basePosition - Vector3.Scale(m_cursor.localPosition -
                    m_cursorBasePosition, transform.lossyScale);
            }
        }
    }
}