using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Haply.HardwareAPI.Internal.Detection;
using Haply.HardwareAPI.Transport;

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
using System.IO.Ports;
#endif

namespace Haply.HardwareAPI.Unity.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor( typeof( HandleThread ), editorForChildClasses: true )]
    public class HandleThreadEditor : Editor
    {
        private SerializedProperty
            m_CachedDeviceAddress,
            m_CachedDeviceId,
            m_CachedDeviceName,
#pragma warning disable IDE1006 // Naming Styles
            onInitialized,
            onButtonUp,
            onButtonDown;
#pragma warning restore IDE1006 // Naming Styles

        private void OnEnable ()
        {
            m_CachedDeviceAddress = serializedObject.FindProperty( nameof( m_CachedDeviceAddress ) );
            m_CachedDeviceId = serializedObject.FindProperty( nameof( m_CachedDeviceId ) );
            m_CachedDeviceName = serializedObject.FindProperty( nameof( m_CachedDeviceName ) );
            onInitialized = serializedObject.FindProperty( nameof( onInitialized ) );
            onButtonUp = serializedObject.FindProperty( nameof( onButtonUp ) );
            onButtonDown = serializedObject.FindProperty( nameof( onButtonDown ) );
        }

        public override bool RequiresConstantRepaint () => true;

        private void AccessStatus ( out ConnectionStatus connectionStatus, out IOException ioException, out Exception exception )
        {
            var handleThread = target as HandleThread;

            if ( Application.isPlaying && handleThread && handleThread.isInitialized )
            {
                connectionStatus = handleThread.connectionStatus;
#pragma warning disable CS0618 // Type or member is obsolete
                ioException = handleThread.ioException;
#pragma warning restore CS0618 // Type or member is obsolete
                exception = handleThread.exception;
            }
            else
            {
                connectionStatus = default;
                ioException = default;
                exception = default;
            }
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            var options = new List<string>();
            var deviceIds = new List<int>();
            var dictionary = new Dictionary<int, (string deviceAddress, int deviceId, string deviceName)>();

            string FormatDeviceInfo ( int deviceId, string deviceName, string deviceAddress )
            {
                return $"{deviceAddress} (Handle {deviceName ?? $"{deviceId:X4}"})";
            }

            if ( !Application.isPlaying )
            {
                var autoDetect = AutoDetectUtility.GetInstance();

                var endpointStatus = autoDetect.storedValues.endpointStatus.GetValues();
                var deviceResponses = autoDetect.storedValues.deviceResponses.GetValues();
                var deviceNames = autoDetect.storedValues.deviceNames.GetValues();

                string GetDeviceName<T> ( (IEndpoint endpoint, T value) x )
                {
                    return autoDetect.storedValues.deviceNames.TryGetValue( x.endpoint, out var deviceName ) ? deviceName : default;
                }

                var devices = from x in endpointStatus
                              where x.value == ScanTaskStatus.Success_Handle
                              join r in deviceResponses on x.endpoint equals r.endpoint
                              select (deviceAddress: x.endpoint.address, deviceId: r.value.deviceId, deviceName: GetDeviceName( x ) );

                foreach ( var device in devices )
                {
                    dictionary.Add( device.deviceId, device );
                }

                deviceIds.AddRange( from d in devices select (int) d.deviceId );
                options.AddRange( from d in devices select FormatDeviceInfo( d.deviceId, d.deviceName, d.deviceAddress ) );
            }

            var selectedIndex = -1;

            if ( m_CachedDeviceId.intValue != 0 )
            {
                var deviceId = m_CachedDeviceId.intValue;

                if ( dictionary.ContainsKey( m_CachedDeviceId.intValue ) )
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

                    dictionary.Add( deviceId, (deviceAddress, deviceId, deviceName) );

                    deviceIds.Add( deviceId );
                    options.Add( FormatDeviceInfo( deviceId, deviceName, deviceAddress ) );
                }
            }
            else if ( options.Count > 0 )
            {
                selectedIndex = 0;

                var device = dictionary[deviceIds[0]];

                m_CachedDeviceAddress.stringValue = device.deviceAddress;
                m_CachedDeviceName.stringValue = device.deviceName;
                m_CachedDeviceId.intValue = device.deviceId;
            }
            else
            {
                dictionary.Add( 0, (default, 0, default) );

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
                    m_CachedDeviceName.stringValue = device.deviceName;
                    m_CachedDeviceId.intValue = device.deviceId;
                }
            }

            EditorGUILayout.Space();

            using ( new EditorGUI.DisabledScope( true ) )
            {
                var q = ((HandleThread)target).orientation;

                EditorGUILayout.Vector4Field( new GUIContent( "Orientation" ), new Vector4( q.x, q.y, q.z, q.w ) );
            }

            EditorGUILayout.Space();

            DrawPropertiesExcluding( serializedObject, "m_Script", "m_DeviceAddress", "onInitialized", "onButtonUp", "onButtonDown" );

            EditorGUILayout.Space();

            using ( new EditorGUI.DisabledScope( !Application.isPlaying ) )
            {
                AccessStatus( out var connectionStatus, out var ioException, out var exception );

                void ExceptionGUI ( Exception e )
                {
                    if ( e != null )
                    {
                        EditorGUILayout.HelpBox( e.Message, MessageType.Error );

                        if ( GUILayout.Button( "Log Stack Trace", EditorStyles.miniButton ) )
                        {
                            Debug.LogException( e, target );
                        }
                    }
                }

                EditorGUILayout.LabelField( "Connection Status", $"{connectionStatus}" );

                ExceptionGUI( ioException );

                ExceptionGUI( exception );
            }

            EditorGUILayout.Space();

            var ht = target as HandleThread;

            if ( ht != null && ht.isInitialized )
            {
                EditorGUILayout.LabelField( "I/O Controls", EditorStyles.boldLabel );
                EditorGUILayout.BeginHorizontal();
                
                var mb = ht.GetDevice().messageBus as Transport.HandleMessageBus;
                var exceptions = mb?.GetExceptions();

                if ( exceptions != null )
                {
                    foreach ( var exception in exceptions )
                    {
                        Debug.LogException( exception );
                    }
                }

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
            }
            
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField( onInitialized, new GUIContent( onInitialized.displayName, onInitialized.tooltip ) );
            EditorGUILayout.PropertyField( onButtonUp, new GUIContent( onButtonUp.displayName, onButtonUp.tooltip ), true );
            EditorGUILayout.PropertyField( onButtonDown, new GUIContent( onButtonDown.displayName, onButtonDown.tooltip), true );
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}