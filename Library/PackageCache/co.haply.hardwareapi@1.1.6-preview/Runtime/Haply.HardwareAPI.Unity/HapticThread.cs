using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
using System.IO.Ports;
#endif

using static Haply.HardwareAPI.Unity.TupleUtility;

namespace Haply.HardwareAPI.Unity
{
    [Obsolete( "This type may be removed/renamed in a subsequent release." )]
    public delegate Vector3 ForceCalculation1 ( in Vector3 position );

    [Obsolete( "This type may be removed/renamed in a subsequent release." )]
    public delegate Vector3 ForceCalculation2 ( in Vector3 position, in Vector3 velocity );

    [Obsolete( "This type may be removed/renamed in a subsequent release." )]
    public delegate Vector3 ForceCalculation1<TAdditionalData> ( in Vector3 position, in TAdditionalData additionalData ) where TAdditionalData : struct;

    [Obsolete( "This type may be removed/renamed in a subsequent release." )]
    public delegate Vector3 ForceCalculation2<TAdditionalData> ( in Vector3 position, in Vector3 velocity, in TAdditionalData additionalData ) where TAdditionalData : struct;

    public partial class HapticThread : MonoBehaviour, IFrameRateProvider, IOrientationTrackedDevice
    {
        [Obsolete( "This type may be removed in a subsequent release." )]
        public enum ForceCalculationInputs
        {
            Position,
            PositionVelocity
        }

        [Obsolete( "This type may be removed in a subsequent release." )]
        public enum SimulationStatus
        {
            None,
            Running,
            TorqueCalculationFailure,
            Exception
        }

        private static readonly Vector3 s_Frame = new Vector3( 0f, 0f, 90f );

        [SerializeField]
        [HideInInspector]
        private Handedness m_CachedHandedness;

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

        public bool isInitialized => m_Inverse3 != null;

        public Transform avatar;

        [SerializeField]
        [Range( Inverse3.MIN_CONTROL_LOOP_RATE, Inverse3.MAX_CONTROL_LOOP_RATE ), Delayed]
        [FormerlySerializedAs( "m_Rate" )]
        [Tooltip( "Haptic thread update frequency." )]
        private int m_TargetFrequency = Inverse3.DEFAULT_CONTROL_LOOP_RATE;

        public int targetFrequency
        {
            get
            {
                return m_TargetFrequency;
            }

            set
            {
                m_TargetFrequency = Mathf.Clamp( value, Inverse3.MIN_CONTROL_LOOP_RATE, Inverse3.MAX_CONTROL_LOOP_RATE );
            }
        }

        public ConnectionStatus connectionStatus => m_Inverse3.connection?.status ?? ConnectionStatus.Disposed;

        [Obsolete( "This property may not be available in a subsequent release." )]
        public IOException ioException => default;

        [Obsolete( "This property may not be available in a subsequent release." )]
        public SimulationStatus simulationStatus
        {
            get
            {
                var status = m_Inverse3?.status ?? default;

                switch ( status )
                {
                    case DeviceStatus.Active:
                        return SimulationStatus.Running;
                    
                    default:
                        return SimulationStatus.None;
                }
            }
        }

        private Exception m_Exception;

        public Exception exception => m_Exception;

        private Dictionary<Type, object> m_AdditionalData;

        private object m_AdditionalDataMutex = new object();

        private object m_BindingMutex = new object();
        private Inverse3 m_Inverse3;

        public UnityEvent onInitialized;

        public bool hold
        {
            set => m_Inverse3.BeginHold();
            get => m_Inverse3.activeControlType == Inverse3.ControlType.IdleHold;
        }

        public int actualFrequency => m_Inverse3.GetIoLoopPerformanceData().observedRate;

        /// <summary>
        /// Get the angle and angular velocity of one of the Inverse3's joints.
        /// </summary>
        /// <param name="index">The joint index (0, 1, or 2).</param>
        /// <returns>The angle in degrees (°) and angular velocity in degrees / second (°/s) of the joint at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public (float angle, float velocity) this[int index]
        {
            get
            {
                var j = m_Inverse3.GetJointStates( out _ );

                switch ( index )
                {
                    case 0:
                        return (j.angles.joint1, j.angularVelocities.joint1);

                    case 1:
                        return (j.angles.joint2, j.angularVelocities.joint2);

                    case 2:
                        return (j.angles.joint3, j.angularVelocities.joint3);

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// The Inverse3's cursor position, in meters (m).
        /// Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        public Vector3 position
        {
            get
            {
                var p = TupleToVector3( m_Inverse3?.cursorPosition ?? default );

                Swap( ref p.y, ref p.z );

                return p;
            }
        }

        /// <summary>
        /// The Inverse3's cursor velocity, in meters / second (m/s).
        /// Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        public Vector3 velocity
        {
            get
            {
                var p = TupleToVector3( m_Inverse3?.cursorVelocity ?? default );

                Swap( ref p.y, ref p.z );

                return p;
            }
        }

        [Obsolete( "This property may not be available in a subsequent release." )]
        public bool isWaitingForOrientationResponse => false;
  
        /// <summary>
        /// The Inverse3's calibrated orientation, in a Unity-friendly reference frame.
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

                    var calibration = m_Inverse3.GetOrientationCalibration( out _ );

                    var orientation = m_Inverse3.orientation;
                    var q = new Quaternion( orientation.x, orientation.y, orientation.z, orientation.w );

                    return frameLHS * q * frameRHS;
                }
                else
                {
                    return Quaternion.identity;
                }
            }
        }

        public string deviceId => m_Inverse3?.deviceInfo.deviceId.ToString( "0000" ) ?? default;

        [Obsolete( "This property may not be available in a subsequent release." )]
        public string deviceModelName => "Inverse3";

        [Obsolete( "This property may not be available in a subsequent release." )]
        public string deviceCompanyName => "Haply";

        public Inverse3 GetDevice ()
        {
            if ( CheckInitialized() )
            {
                return m_Inverse3;
            }
            else
            {
                return default;
            }
        }

        public void QueryOrientation ()
        {
            m_Inverse3.QueryOrientation();
        }

        public void CalibrateOrientation ()
        {
            m_Inverse3.CalibrateOrientation();
        }

        /// <summary>
        /// The Inverse3's Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        public void SetForce ( Vector3 force )
        {
            if ( CheckInitialized() )
            {
                var (x, y, z) = Vector3ToTuple( force );

                Swap( ref y, ref z );

                m_Inverse3.SetCursorForce( x, y, z );
            }
        }

        public void SetAdditionalData<T> ( in T additionalData ) where T : struct
        {
            var key = typeof( T );

            lock ( m_AdditionalDataMutex )
            {
                if ( m_AdditionalData == null )
                {
                    m_AdditionalData = new Dictionary<Type, object>();
                }

                if ( m_AdditionalData.TryGetValue( key, out var value ) )
                {
                    var container = value as AdditionalDataContainer<T>;

                    container.Set( additionalData );
                }
                else
                {
                    m_AdditionalData[key] = new AdditionalDataContainer<T>( additionalData );
                }
            }
        }

        private T GetAdditionalData<T> () where T : struct
        {
            var key = typeof( T );

            lock ( m_AdditionalDataMutex )
            {
                if ( m_AdditionalData == null )
                {
                    m_AdditionalData = new Dictionary<Type, object>();
                }

                if ( m_AdditionalData.TryGetValue( key, out var value ) )
                {
                    var container = value as AdditionalDataContainer<T>;

                    return container.Get();
                }
                else
                {
                    return default;
                }
            }
        }

        private bool CheckInitialized ( bool logError = true )
        {
            if ( m_Inverse3 == null )
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

        /// <summary>
        /// The Inverse3's Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        /// <param name="forceCalculation">Force calculation function.</param>
#pragma warning disable CS0618 // Type or member is obsolete
        public void Run ( ForceCalculation1 forceCalculation )
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if ( CheckInitialized() )
            {
                m_Inverse3.BeginCursorForceControl( ( in (float x, float y, float z) position, in (float x, float y, float z) velocity ) =>
                {
                    var p = TupleToVector3( position );

                    Swap( ref p.y, ref p.z );

                    var f = Vector3ToTuple( forceCalculation( p ) );
                
                    Swap( ref f.y, ref f.z );
                    
                    return f;
                } );
            }
        }

        /// <summary>
        /// The Inverse3's Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        /// <param name="forceCalculation">Force calculation function.</param>
#pragma warning disable CS0618 // Type or member is obsolete
        public void Run ( ForceCalculation2 forceCalculation )
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if ( CheckInitialized() )
            {
                m_Inverse3.BeginCursorForceControl( ( in (float x, float y, float z) position, in (float x, float y, float z) velocity ) =>
                {
                    var p = TupleToVector3( position );
                    var v = TupleToVector3( velocity );

                    Swap( ref p.y, ref p.z );
                    Swap( ref v.y, ref v.z );

                    var f = Vector3ToTuple( forceCalculation( p, v ) );
                
                    Swap( ref f.y, ref f.z );
                    
                    return f;
                } );
            }
        }

        /// <summary>
        /// The Inverse3's Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        /// <typeparam name="TAdditionalData">Type of the additional data struct.</typeparam>
        /// <param name="forceCalculation">Force calculation function.</param>
        /// <param name="additionalData">Initial values for additional data.</param>
#pragma warning disable CS0618 // Type or member is obsolete
        public void Run<TAdditionalData> ( ForceCalculation1<TAdditionalData> forceCalculation, TAdditionalData additionalData ) where TAdditionalData : struct
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if ( CheckInitialized() )
            {
                SetAdditionalData( additionalData );

                m_Inverse3.BeginCursorForceControl( ( in (float x, float y, float z) position, in (float x, float y, float z) velocity ) =>
                {
                    var p = TupleToVector3( position );

                    Swap( ref p.y, ref p.z );

                    var f = Vector3ToTuple( forceCalculation( p, GetAdditionalData<TAdditionalData>() ) );
                
                    Swap( ref f.y, ref f.z );
                    
                    return f;
                } );
            }
        }

        /// <summary>
        /// The Inverse3's Y and Z components are swapped automatically in order to conform with Unity's axis convention.
        /// </summary>
        /// <typeparam name="TAdditionalData">Type of the additional data struct.</typeparam>
        /// <param name="forceCalculation">Force calculation function.</param>
        /// <param name="additionalData">Initial values for additional data.</param>
#pragma warning disable CS0618 // Type or member is obsolete
        public void Run<TAdditionalData> ( ForceCalculation2<TAdditionalData> forceCalculation, TAdditionalData additionalData ) where TAdditionalData : struct
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if ( CheckInitialized() )
            {
                SetAdditionalData( additionalData );

                m_Inverse3.BeginCursorForceControl( ( in (float x, float y, float z) position, in (float x, float y, float z) velocity ) =>
                {
                    var p = TupleToVector3( position );
                    var v = TupleToVector3( velocity );

                    Swap( ref p.y, ref p.z );
                    Swap( ref v.y, ref v.z );

                    var f = Vector3ToTuple( forceCalculation( p, v, GetAdditionalData<TAdditionalData>() ) );
                
                    Swap( ref f.y, ref f.z );
                    
                    return f;
                } );
            }
        }

        [Obsolete( "This method is no longer supported, please use Run( ... ) instead." )]
        public void SetForceCalculation ( ForceCalculation1 forceCalculation )
        {
            Debug.LogError( "SetForceCalculation is no longer supported. Please use Run( ... ) instead." );
        }

        [Obsolete( "This method is no longer supported, please use Run( ... ) instead." )]
        public void SetForceCalculation ( ForceCalculation2 forceCalculation )
        {
            Debug.LogError( "SetForceCalculation is no longer supported. Please use Run( ... ) instead." );
        }

        [Obsolete( "This method is no longer supported, please use Run( ... ) instead." )]
        public void SetForceCalculation<TAdditionalData> ( ForceCalculation1<TAdditionalData> forceCalculation, TAdditionalData additionalData ) where TAdditionalData : struct
        {
            Debug.LogError( "SetForceCalculation is no longer supported. Please use Run( ... ) instead." );
        }

        [Obsolete( "This method is no longer supported, please use Run( ... ) instead." )]
        public void SetForceCalculation<TAdditionalData> ( ForceCalculation2<TAdditionalData> forceCalculation, TAdditionalData additionalData ) where TAdditionalData : struct
        {
            Debug.LogError( "SetForceCalculation is no longer supported. Please use Run( ... ) instead." );
        }

        [Obsolete( "This method no longer has any effect." )]
        public void Cancel ()
        {
            Debug.LogError( "This method is obsolete and no longer has any effect." );
        }

        public void Pause ()
        {
            if ( CheckInitialized() )
            {
                m_Inverse3.PauseControlTasks();
            }
        }

        public void Resume ()
        {
            if ( CheckInitialized() )
            {
                m_Inverse3.ResumeControlTasks();
            }
        }

        /// <summary>
        /// This method has no effect.
        /// Please use the inkwell calibration procedure to calibrate your device.
        /// </summary>
        [Obsolete]
        public void CalibrateDevice ()
        {
            Debug.LogError( "This method is obsolete and no longer has any effect." );
        }

        private void TryInitialize ()
        {
            if ( string.IsNullOrWhiteSpace( m_CachedDeviceAddress ) )
            {
                throw new Exception( "Device address not set" );
            }
            try
            {
                m_Inverse3 = new Inverse3( m_CachedDeviceAddress );
                onInitialized.Invoke();
            }
            catch ( Exception e )
            {
                if ( m_Inverse3 != null )
                {
                    try
                    {
                        m_Inverse3.Dispose();
                    }
                    catch ( Exception )
                    {
                        // ignored
                    }
                }

                m_Exception = e;
                m_Inverse3 = null;
            }
        }

        public bool TryBindTo ( IEndpoint endpoint, out Exception exception )
        {
            lock ( m_BindingMutex )
            {
                Inverse3 inverse3 = default;

                try
                {
                    inverse3 = new Inverse3( endpoint );
                    exception = default;

                    m_Inverse3 = inverse3;
                    m_CachedDeviceAddress = endpoint.address;

                    return true;
                }
                catch ( Exception e )
                {
                    exception = e;

                    if ( inverse3 != null )
                    {
                        try
                        {
                            inverse3.Dispose();
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
                if ( m_Inverse3 == null )
                {
                    TryInitialize();
                }
                else if ( m_Inverse3.endpoint.address != m_CachedDeviceAddress )
                {
                    m_Inverse3.Dispose();
                    m_Inverse3 = null;

                    TryInitialize();
                }
            }
            
            if ( m_Inverse3 != null )
            {
                if ( m_Inverse3.targetIoRate != m_TargetFrequency )
                {
                    m_Inverse3.targetIoRate = m_TargetFrequency;
                }

                if ( avatar != null )
                {
                    avatar.localPosition = position;
                }
            }
        }

        private void OnDestroy ()
        {
            m_Inverse3?.Dispose();
            m_Inverse3 = null;
        }
    }
}