using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Haply.HardwareAPI.Internal.Detection;

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
using System.IO.Ports;
#endif

namespace Haply.HardwareAPI.Unity.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor( typeof( HapticThread ), editorForChildClasses: true )]
    public class HapticThreadEditor : Editor
    {
        private static List<Handedness> s_HandednessEnumValues = new List<Handedness>( Enum.GetValues( typeof( Handedness ) ) as Handedness[] ?? Array.Empty<Handedness>() );

        private SerializedProperty
            m_CachedDeviceAddress,
            m_CachedDeviceId,
            m_CachedDeviceName,
            m_CachedHandedness,
            m_TargetFrequency,
#pragma warning disable IDE1006 // Naming Styles
            avatar,
            onInitialized;
#pragma warning restore IDE1006 // Naming Styles

        private float[] m_EncoderAngles = new float[3];
        private float[] m_EncoderVelocities = new float[3];

        private void OnEnable ()
        {
            m_CachedDeviceAddress = serializedObject.FindProperty( nameof( m_CachedDeviceAddress ) );
            m_CachedDeviceId = serializedObject.FindProperty( nameof( m_CachedDeviceId ) );
            m_CachedDeviceName = serializedObject.FindProperty( nameof( m_CachedDeviceName ) );
            m_CachedHandedness = serializedObject.FindProperty( nameof( m_CachedHandedness ) );
            m_TargetFrequency = serializedObject.FindProperty( nameof( m_TargetFrequency ) );
            avatar = serializedObject.FindProperty( nameof( avatar ) );
            onInitialized = serializedObject.FindProperty( nameof( onInitialized ) );
        }

        public override bool RequiresConstantRepaint () => true;

        private void AccessHapticData ( float[] encoderAngles, float[] encoderVelocities, out Vector3 position, out Vector3 velocity )
        {
            var hapticThread = target as HapticThread;
            if ( Application.isPlaying && hapticThread && hapticThread.isInitialized)
            {
                for ( var i = 0; i < 3; i++ )
                {
                    (encoderAngles[i], encoderVelocities[i]) = hapticThread[i];
                }

                position = hapticThread.position;
                velocity = hapticThread.velocity;
            }
            else
            {
                for ( var i = 0; i < 3; i++ )
                {
                    encoderAngles[i] = 0f;
                    encoderVelocities[i] = 0f;
                }

                position = Vector3.zero;
                velocity = Vector3.zero;
            }
        }

        private void AccessStatus (
            out ConnectionStatus connectionStatus, out IOException ioException,
#pragma warning disable CS0618 // Type or member is obsolete
            out HapticThread.SimulationStatus simulationStatus, out Exception exception,
#pragma warning restore CS0618 // Type or member is obsolete
            out int fps )
        {
            var hapticThread = target as HapticThread;
            if ( Application.isPlaying && hapticThread && hapticThread.isInitialized )
            {
                connectionStatus = hapticThread.connectionStatus;
#pragma warning disable CS0618 // Type or member is obsolete
                ioException = hapticThread.ioException;
                simulationStatus = hapticThread.simulationStatus;
#pragma warning restore CS0618 // Type or member is obsolete
                exception = hapticThread.exception;
                fps = hapticThread.actualFrequency;
            }
            else
            {
                connectionStatus = default;
                ioException = default;
#pragma warning disable CS0618 // Type or member is obsolete
                ioException = hapticThread.ioException;
                simulationStatus = HapticThread.SimulationStatus.None;
#pragma warning restore CS0618 // Type or member is obsolete
                exception = default;
                fps = 0;
            }
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            var options = new List<string>();
            var deviceIds = new List<int>();
            var dictionary = new Dictionary<int, (string deviceAddress, int deviceId, string deviceName, Handedness handedndess)>();

            string FormatDeviceInfo ( int deviceId, string deviceName, string deviceAddress, Handedness handedness )
            {
                return $"{deviceAddress} (Inverse3 {deviceName ?? $"{deviceId:X4}"}, {(deviceId == 0 ? "" : handedness.ToString().Substring( 0, 1 ))})";
            }

            if ( !Application.isPlaying )
            {
                var autoDetect = AutoDetectUtility.GetInstance();

                var endpointStatus = autoDetect.storedValues.endpointStatus.GetValues();
                var deviceResponses = autoDetect.storedValues.deviceResponses.GetValues();
                var deviceHandedness = autoDetect.storedValues.deviceHandedness.GetValues();
                var deviceNames = autoDetect.storedValues.deviceNames.GetValues();

                string GetDeviceName<T> ( (IEndpoint endpoint, T value) x )
                {
                    return autoDetect.storedValues.deviceNames.TryGetValue( x.endpoint, out var deviceName ) ? deviceName : default;
                }

                var devices = from x in endpointStatus
                              where x.value == ScanTaskStatus.Success_Inverse3
                              join r in deviceResponses on x.endpoint equals r.endpoint
                              join h in deviceHandedness on x.endpoint equals h.endpoint
                              select (deviceAddress: x.endpoint.address, deviceId: r.value.deviceId, deviceName: GetDeviceName( x ), handedness: h.value);

                foreach ( var device in devices )
                {
                    dictionary.Add( device.deviceId, device );
                }

                deviceIds.AddRange( from d in devices select (int) d.deviceId );
                options.AddRange( from d in devices select FormatDeviceInfo( d.deviceId, d.deviceName, d.deviceAddress, d.handedness ) );
            }

            var selectedIndex = -1;

            if ( m_CachedDeviceId.intValue != 0 )
            {
                var deviceId = m_CachedDeviceId.intValue;

                if ( dictionary.ContainsKey( m_CachedDeviceId.intValue) )
                {
                    selectedIndex = deviceIds.IndexOf( deviceId );

                    // Make sure the COM port is up-to-date,
                    // in case the same device ID is now associated
                    // to a new COM port since the last save
                    m_CachedDeviceAddress.stringValue = dictionary[deviceId].deviceAddress;
                }
                else
                {
                    selectedIndex = options.Count;

                    var deviceAddress = m_CachedDeviceAddress.stringValue;
                    var deviceName = m_CachedDeviceName.stringValue;
                    var handedness = s_HandednessEnumValues[m_CachedHandedness.enumValueIndex];

                    dictionary.Add( deviceId, (deviceAddress, deviceId, deviceName, handedness) );

                    deviceIds.Add( deviceId );
                    options.Add( FormatDeviceInfo( deviceId, deviceName, deviceAddress, handedness ) );
                }
            }
            else if ( options.Count > 0 )
            {
                selectedIndex = 0;
                
                var device = dictionary[deviceIds[0]];

                m_CachedDeviceAddress.stringValue = device.deviceAddress;
                m_CachedHandedness.enumValueIndex = s_HandednessEnumValues.IndexOf( device.handedndess );
                m_CachedDeviceName.stringValue = device.deviceName;
                m_CachedDeviceId.intValue = device.deviceId;
            }
            else
            {
                dictionary.Add( 0, (default, 0, default, 0) );

                deviceIds.Insert( 0, 0 );
                options.Insert( 0, "" );

                selectedIndex = 0;
            }

            using ( new EditorGUI.DisabledScope( Application.isPlaying ) )
            {
                EditorGUI.BeginChangeCheck();

                var newIndex = EditorGUILayout.Popup( "Device Address", selectedIndex, options.ToArray() );

                if ( EditorGUI.EndChangeCheck() )
                {
                    var device = dictionary[deviceIds[newIndex]];

                    m_CachedDeviceAddress.stringValue = device.deviceAddress;
                    m_CachedHandedness.enumValueIndex = s_HandednessEnumValues.IndexOf( device.handedndess );
                    m_CachedDeviceName.stringValue = device.deviceName;
                    m_CachedDeviceId.intValue = device.deviceId;
                }
            }

            EditorGUILayout.Space();

            using ( new EditorGUI.DisabledScope( true ) )
            {
                var q = ((HapticThread)target).orientation;

                EditorGUILayout.Vector4Field( new GUIContent( "Orientation" ), new Vector4( q.x, q.y, q.z, q.w ) );
            }

            using ( new EditorGUI.DisabledScope( true ) )
            {
                AccessHapticData( m_EncoderAngles, m_EncoderVelocities, out var position, out var velocity );

                m_CachedDeviceAddress.isExpanded = EditorGUILayout.Foldout( m_CachedDeviceAddress.isExpanded, "Encoders (3)" );

                if ( m_CachedDeviceAddress.isExpanded )
                {
                    EditorGUI.indentLevel++;

                    for ( var i = 0; i < 3; i++ )
                    {
                        EditorGUILayout.LabelField( $"Encoder {i}" );

                        EditorGUI.indentLevel++;
                        EditorGUILayout.FloatField( $"Angle", m_EncoderAngles[i] );
                        EditorGUILayout.FloatField( $"Velocity", m_EncoderVelocities[i] );
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Vector3Field( new GUIContent( "Cursor Position" ), position );
                EditorGUILayout.Vector3Field( new GUIContent( "Cursor Velocity" ), velocity );
            }

            EditorGUILayout.PropertyField( avatar, new GUIContent( "Cursor Avatar", avatar.tooltip ) );

            EditorGUILayout.Space();

            DrawPropertiesExcluding( serializedObject, "m_Script", "m_DeviceAddress", "avatar", "onInitialized" );

            EditorGUILayout.Space();

            using ( new EditorGUI.DisabledScope( !Application.isPlaying ) )
            {
                AccessStatus( out var connectionStatus, out var ioException, out var simulationStatus, out var exception, out var fps );

                void ExceptionGUI ( Exception exception )
                {
                    if ( exception != null )
                    {
                        EditorGUILayout.HelpBox( exception.Message, MessageType.Error );

                        if ( GUILayout.Button( "Log Stack Trace", EditorStyles.miniButton ) )
                        {
                            Debug.LogException( exception, target );
                        }
                    }
                }

                EditorGUILayout.LabelField( "Connection Status", $"{connectionStatus}" );

                ExceptionGUI( ioException );

                EditorGUILayout.LabelField( "Simulation Status", $"{simulationStatus}" );

                ExceptionGUI( exception );

                EditorGUILayout.LabelField( "Simulation FPS", $"{fps}" );
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField( "Orientation Controls", EditorStyles.boldLabel );

            EditorGUILayout.BeginHorizontal();

            var ht = target as HapticThread;

#pragma warning disable CS0618 // Type or member is obsolete
            using ( new EditorGUI.DisabledScope( ht.simulationStatus != HapticThread.SimulationStatus.Running ) )
            {
#pragma warning restore CS0618 // Type or member is obsolete
                if ( GUILayout.Button( "Query Orientation", EditorStyles.miniButtonLeft ) )
                {
                    ht.QueryOrientation();
                }

                if ( GUILayout.Button( "Calibrate Orientation", EditorStyles.miniButtonRight ) )
                {
                    ht.CalibrateOrientation();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField( "I/O Controls", EditorStyles.boldLabel );

            EditorGUILayout.BeginHorizontal();

            using ( new EditorGUI.DisabledScope( ht ) )
            {
                if ( GUILayout.Button( "Pause", EditorStyles.miniButtonLeft ) )
                {
                    ht.Pause();
                }

                if ( GUILayout.Button( "Resume", EditorStyles.miniButtonRight ) )
                {
                    ht.Resume();
                }
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField( onInitialized, new GUIContent( onInitialized.displayName, onInitialized.tooltip ) );
        }
    }
}