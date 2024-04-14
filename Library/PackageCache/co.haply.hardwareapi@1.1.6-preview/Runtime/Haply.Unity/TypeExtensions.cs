using UnityEngine;

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
using System.IO.Ports;
#else
#endif

namespace Haply.Unity
{
    public static class TypeExtensions
    {
        public static Vector3 AsVector3 ( this (float x, float y, float z) tuple )
        {
            Vector3 vector3;
            vector3.x = tuple.x;
            vector3.y = tuple.y;
            vector3.z = tuple.z;
            return vector3;
        }

        public static (float x, float y, float z) AsTuple ( this Vector3 vector3 ) => (vector3.x, vector3.y, vector3.z);

        public static Quaternion AsQuaternion ( this (float x, float y, float z, float w) tuple )
        {
            Quaternion quaternion;
            quaternion.x = tuple.x;
            quaternion.y = tuple.y;
            quaternion.z = tuple.z;
            quaternion.w = tuple.w;
            return quaternion;
        }
    }
}