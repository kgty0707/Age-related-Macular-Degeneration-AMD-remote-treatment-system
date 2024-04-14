using Haply.HardwareAPI.Unity;
using UnityEngine;

public class HapticController : MonoBehaviour
{
    [Range(-2, 2)]
    public float forceX;
    [Range(-2, 2)]
    public float forceY;
    [Range(-2, 2)]
    public float forceZ;

    private void Awake()
    {
        var hapticThread = GetComponent<HapticThread>();
        hapticThread.onInitialized.AddListener(() => hapticThread.Run(ForceCalculation));
    }

    private Vector3 ForceCalculation(in Vector3 position)
    {
        return new Vector3(forceX, forceY, forceZ);
    }
}