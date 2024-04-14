using Haply.HardwareAPI.Unity;
using UnityEngine;

public class ForceAndPosition : MonoBehaviour
{
    [Range(0, 800)]
    public float stiffness = 200f;
    
    private Vector3 m_initialPosition = Vector3.zero;
    
    private void Awake()
    {
        // find the HapticThread object (you can do it in Awake() or use a public field and set it in scene)
        var hapticThread = GetComponent<HapticThread>();
        // run the haptic loop with given function
        hapticThread.onInitialized.AddListener(() => hapticThread.Run(ForceCalculation) );
    }

    /// <summary>
    /// Calculate force to apply to keep cursor to initial position.
    /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
    /// </summary>
    /// <param name="position">cursor position</param>
    /// <returns>Force to apply</returns>
    private Vector3 ForceCalculation(in Vector3 position)
    {
        if (m_initialPosition == Vector3.zero)
        {
            // save the first device effector position 
            m_initialPosition = position;
        }
        // return opposite force to stay at initial position
        return (m_initialPosition - position) * stiffness;
    }
}