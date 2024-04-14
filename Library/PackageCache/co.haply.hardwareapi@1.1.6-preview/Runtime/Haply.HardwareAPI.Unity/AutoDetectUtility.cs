using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haply.HardwareAPI.Internal.Detection;
using System;
using System.Collections.Concurrent;

namespace Haply.HardwareAPI.Unity
{
    /// <summary>
    /// Static wrapper for the <see cref="AutoDetect"/> singleton.
    /// </summary>
    public static class AutoDetectUtility
    {
        private static AutoDetect s_Instance;

        /// <summary>
        /// Event raised whenever an Inverse3 device is detected.
        /// </summary>
        public static event Action<(IEndpoint endpoint, ushort id, string name, Handedness handedness)> OnDetectInverse3;

        /// <summary>
        /// Event raised whenever a Handle device is detected.
        /// </summary>
        public static event Action<(IEndpoint endpoint, ushort id)> OnDetectHandle;

        private static ConcurrentQueue<(IEndpoint endpoint, ScanTaskStatus status)> s_ScanEventQueue;

        static AutoDetectUtility ()
        {
            s_ScanEventQueue = new ConcurrentQueue<(IEndpoint endpoint, ScanTaskStatus status)>();
        }

        /// <summary>
        /// Consume the event queue and raise <see cref="OnDetectInverse3"/> and <see cref="OnDetectHandle"/> events.
        /// Calls <see cref="Initialize(bool, bool)"/> if the instance has not yet been initialized.
        /// </summary>
        public static void Poll ()
        {
            if ( s_Instance == null )
            {
                Initialize();
            }

            while ( s_ScanEventQueue.TryDequeue( out var e ) )
            {
                RaiseDetectEvents( e.endpoint, e.status );
            }
        }

        /// <summary>
        /// Access the internal <see cref="AutoDetect"/> instance.
        /// Calls <see cref="Initialize(bool, bool)"/> if the instance has not yet been initialized.
        /// </summary>
        /// 
        /// <returns>The internal <see cref="AutoDetect"/> instance.</returns>
        public static AutoDetect GetInstance ()
        {
            if ( s_Instance == null )
            {
                Initialize();
            }

            return s_Instance;
        }

        /// <summary>
        /// Check the active task count of the internal <see cref="AutoDetect"/> instance.
        /// </summary>
        /// 
        /// <returns>True, if one or more device scanning tasks are running.</returns>
        public static bool IsBusyPolling () => s_Instance.activeTaskCount > 0;

        public static IEnumerable<(IEndpoint endpoint, ushort id, string name, Handedness handedness)> ListDevicesInverse3 ()
        {
            var endpointStatus = s_Instance.storedValues.endpointStatus.GetValues();
            var deviceResponses = s_Instance.storedValues.deviceResponses.GetValues();
            var deviceHandedness = s_Instance.storedValues.deviceHandedness.GetValues();

            string GetDeviceName<T> ( (IEndpoint endpoint, T value) x )
            {
                return s_Instance.storedValues.deviceNames.TryGetValue( x.endpoint, out var deviceName ) ? deviceName : default;
            }

            var devices = from x in endpointStatus
                          where x.value == ScanTaskStatus.Success_Inverse3
                          join r in deviceResponses on x.endpoint equals r.endpoint
                          join h in deviceHandedness on x.endpoint equals h.endpoint
                          select (endpoint: x.endpoint, id: r.value.deviceId, name: GetDeviceName( x ), handedness: h.value);

            return devices;
        }

        public static IEnumerable<(IEndpoint endpoint, ushort id)> ListDevicesHandle ()
        {
            var endpointStatus = s_Instance.storedValues.endpointStatus.GetValues();
            var deviceResponses = s_Instance.storedValues.deviceResponses.GetValues();

            var devices = from x in endpointStatus
                          where x.value == ScanTaskStatus.Success_Inverse3
                          join r in deviceResponses on x.endpoint equals r.endpoint
                          select (endpoint: x.endpoint, id: r.value.deviceId);

            return devices;
        }

        private static void HandleScanEvent ( IEndpoint endpoint, ScanTaskStatus status )
        {
            s_ScanEventQueue.Enqueue( (endpoint, status) );
        }

        private static void RaiseDetectEvents ( IEndpoint endpoint, ScanTaskStatus status )
        {
            switch ( status )
            {
                case ScanTaskStatus.Success_Inverse3:
                {
                    s_Instance.storedValues.deviceResponses.TryGetValue( endpoint, out var info );
                    s_Instance.storedValues.deviceNames.TryGetValue( endpoint, out var name );
                    s_Instance.storedValues.deviceHandedness.TryGetValue( endpoint, out var handedness );

                    OnDetectInverse3?.Invoke( (endpoint, info.deviceId, name, handedness) );

                    break;
                }

                case ScanTaskStatus.Success_Handle:
                {
                    s_Instance.storedValues.deviceResponses.TryGetValue( endpoint, out var info );

                    OnDetectHandle?.Invoke( (endpoint, info.deviceId) );

                    break;
                }
            }
        }

        /// <summary>
        /// Initialize the internal <see cref="AutoDetect"/> instance.
        /// </summary>
        /// 
        /// <param name="filterPortNames">
        /// Whether or not to filter connected hardware against a list of known device names.
        /// Set this to <see cref="false"/> for new/experimental devices.
        /// </param>
        /// <param name="queryDeviceNames">
        /// Whether or not to query Haply's remote REST API for friendly device name data.
        /// Remove this to speed-up detection by 100 to 300 ms.
        /// Network failures and query misses will not prevent a device from being detected.
        /// </param>
        public static void Initialize ( bool filterPortNames = true, bool queryDeviceNames = true )
        {
            Dispose();

            var flags = AutoDetectFlags.GetHandedness | AutoDetectFlags.GetNames | AutoDetectFlags.KeepLogs;

            if ( filterPortNames )
            {
                flags |= AutoDetectFlags.FilterPorts;
            }

            if ( queryDeviceNames )
            {
                flags |= AutoDetectFlags.GetNames;
            }

            s_Instance = new AutoDetect( flags: flags );

            s_Instance.events.endpointScanStatus.Subscribe( HandleScanEvent );
            s_Instance.Start();
        }

        /// <summary>
        /// Dispose the internal <see cref="AutoDetect"/> instance.
        /// </summary>
        public static void Dispose ()
        {
            if ( s_Instance != null )
            {
                s_Instance.events.endpointScanStatus.Unsubscribe( HandleScanEvent );
                s_Instance.Dispose();
                s_Instance = default;

                s_ScanEventQueue.Clear();
            }
        }
    }
}