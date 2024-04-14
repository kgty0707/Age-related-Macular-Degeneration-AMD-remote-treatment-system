using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
using System.IO.Ports;
#endif

using static Haply.HardwareAPI.Unity.TupleUtility;

namespace Haply.HardwareAPI.Unity
{
    public class HandleThread : MonoBehaviour, IOrientationTrackedDevice
    {
        private static readonly Vector3 s_Frame = new Vector3( 0f, 0f, 0f );

        [SerializeField]
        [HideInInspector]
        private int m_CachedDeviceId;

        [SerializeField]
        [HideInInspector]
        private string m_CachedDeviceName;

        [SerializeField]
        [HideInInspector]
        private string m_CachedDeviceAddress;

        public string deviceAddress
        {
            get
            {
                return m_CachedDeviceAddress;
            }

            [Obsolete( "Setting address directly is no longer supported. Please use the drop down in the inspector (or the TryBindTo method) instead." )]
            set
            {
                Debug.LogError( "Setting address directly is no longer supported. Please use the drop down in the inspector (or the TryBindTo method) instead." );
            }
        }

        private bool m_ButtonPressed;
        
        public UnityEvent onButtonDown;
        public UnityEvent onButtonUp;

        public bool buttonPressed
        {
            get => m_ButtonPressed;
            private set
            {
                if (m_ButtonPressed == value)
                {
                    return;
                }

                m_ButtonPressed = value;
                if (m_ButtonPressed)
                {
                    onButtonDown?.Invoke();
                }
                else
                {
                    onButtonUp?.Invoke();
                }
            }
        }

        public bool isInitialized => m_Handle != null;

        public ConnectionStatus connectionStatus => m_Handle.connection?.status ?? ConnectionStatus.Disposed;

        [Obsolete( "This property may not be available in a subsequent release." )]
        public IOException ioException => default;

        private Exception m_Exception;

        public Exception exception => m_Exception;

        private object m_BindingMutex = new object();
        private Handle m_Handle;
        
        public UnityEvent onInitialized;

        /// <summary>
        /// The Handle's calibrated orientation, in a Unity-friendly reference frame.
        /// This will be different from <see cref="Inverse3.orientation"/>.
        /// </summary>
        public Quaternion orientation
        {
            get
            {
                if ( CheckInitialized( false ) )
                {
                    var frameRHS = Quaternion.Euler( s_Frame );
                    var frameLHS = Quaternion.Inverse( frameRHS );

                    var orientation = m_Handle.orientation;

                    var q = new Quaternion( orientation.x, orientation.y, orientation.z, orientation.w );

                    Swap( ref q.y, ref q.z );

                    q.x = -q.x;
                    q.y = -q.y;
                    q.z = -q.z;

                    return frameLHS * q * frameRHS;
                }
                else
                {
                    return Quaternion.identity;
                }
            }
        }

        public Transform avatar;

        public string deviceId => m_Handle?.deviceInfo.deviceId.ToString( "0000" ) ?? default;

        [Obsolete( "This property may not be available in a subsequent release." )]
        public string deviceModelName => "Handle";

        [Obsolete( "This property may not be available in a subsequent release." )]
        public string deviceCompanyName => "Haply";

        public Handle GetDevice ()
        {
            if ( CheckInitialized() )
            {
                return m_Handle;
            }
            else
            {
                return default;
            }
        }

        public void CalibrateOrientation ()
        {
            Debug.LogWarning( "Handle orientation calibration is performed in hardware. Please consult the documentation for additional details." );
        }

        private bool CheckInitialized ( bool logError = true )
        {
            if ( m_Handle == null )
            {
                if ( logError )
                {
                    Debug.LogError( "This device has not been initialized yet.", this );
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        public void Pause ()
        {
            if ( CheckInitialized() )
            {
                m_Handle.PauseControlTasks();
            }
        }

        public void Resume ()
        {
            if ( CheckInitialized() )
            {
                m_Handle.ResumeControlTasks();
            }
        }

        private void TryInitialize ()
        {
            if ( string.IsNullOrWhiteSpace( m_CachedDeviceAddress ) )
            {
                throw new Exception( "Handle address not set" );
            }
            try
            {
                m_Handle = new Handle( m_CachedDeviceAddress );
                onInitialized.Invoke();
            }
            catch ( Exception e )
            {
                if ( m_Handle != null )
                {
                    try
                    {
                        m_Handle.Dispose();
                    }
                    catch ( Exception )
                    {
                        // ignored
                    }
                }

                m_Exception = e;
                m_Handle = null;
            }
        }

        public bool TryBindTo ( IEndpoint endpoint, out Exception exception )
        {
            lock ( m_BindingMutex )
            {
                Handle handle = default;

                try
                {
                    handle = new Handle( endpoint, default );
                    exception = default;

                    m_Handle = handle;
                    m_CachedDeviceAddress = endpoint.address;
                    
                    return true;
                }
                catch ( Exception e )
                {
                    exception = e;

                    if ( handle != null )
                    {
                        try
                        {
                            handle.Dispose();
                        }
                        catch ( Exception )
                        {
                            // ignored
                        }
                    }

                    return false;
                }
            }
        }

        private void Update ()
        {
            lock ( m_BindingMutex )
            {
                if ( m_Handle == null )
                {
                    TryInitialize();
                }
                else if ( m_Handle.endpoint.address != m_CachedDeviceAddress )
                {
                    m_Handle.Dispose();
                    m_Handle = null;

                    TryInitialize();
                }

                if ( m_Handle != null )
                {
                    if ( avatar != null )
                    {
                        avatar.localRotation = orientation;
                    }

                    try
                    {
                        buttonPressed = m_Handle.GetStatusByte( 0 ) == 1;
                    }
                    catch ( ArgumentOutOfRangeException )
                    {
                        // ignored
                    }
                }
            }
        }

        private void OnDestroy ()
        {
            m_Handle?.Dispose();
            m_Handle = null;
        }
    }
}