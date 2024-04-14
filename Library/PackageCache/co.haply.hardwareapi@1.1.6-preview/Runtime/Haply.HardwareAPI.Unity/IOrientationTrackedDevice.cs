using UnityEngine;

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
using System.IO.Ports;
#else
#endif

namespace Haply.HardwareAPI.Unity
{
    public interface IOrientationTrackedDevice
    {
        Quaternion orientation { get; }

        void CalibrateOrientation ();
    }
}