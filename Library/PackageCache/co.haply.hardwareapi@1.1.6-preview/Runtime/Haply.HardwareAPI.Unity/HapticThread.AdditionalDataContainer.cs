using System.Reflection;
using UnityEngine;

namespace Haply.HardwareAPI.Unity
{
    public partial class HapticThread
    {
        private class AdditionalDataContainer<T> where T : struct
        {
            public static bool ValidateStructMembers ()
            {
                var type = typeof( T );
                var valid = true;

                foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
                {
                    if ( field.FieldType.IsClass )
                    {
                        Debug.LogError( $"Invalid field type in scene data struct. {type.Namespace}.{field.Name}'s declared type ({field.FieldType}) is a reference type. Only value types are supported at this time." );
                        valid = false;
                    }
                }

                foreach ( var property in type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
                {
                    if ( property.PropertyType.IsClass )
                    {
                        Debug.LogError( $"Invalid property type in scene data struct. {type.Namespace}.{property.Name}'s declared type ({property.PropertyType}) is a reference type. Only value types are supported at this time." );
                        valid = false;
                    }
                }

                return valid;
            }

            static AdditionalDataContainer ()
            {
                ValidateStructMembers();
            }


            private T m_Data = default( T );
            private object m_Mutex = new object();

            public AdditionalDataContainer () { }

            public AdditionalDataContainer ( in T data )
            {
                m_Data = data;
            }

            public void Set ( in T additionalData )
            {
                lock ( m_Mutex )
                {
                    m_Data = additionalData;
                }
            }

            internal T Get ()
            {
                lock ( m_Mutex )
                {
                    return m_Data;
                }
            }
        }
    }
}