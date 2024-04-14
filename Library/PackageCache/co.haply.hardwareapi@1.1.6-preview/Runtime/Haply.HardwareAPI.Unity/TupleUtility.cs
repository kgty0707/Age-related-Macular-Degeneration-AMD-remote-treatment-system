
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Haply.HardwareAPI.Unity
{
    internal static class TupleUtility
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Vector3 TupleToVector3 ( (float x, float y, float z) t )
        {
            Vector3 v;

            v.x = t.x;
            v.y = t.y;
            v.z = t.z;

            return v;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static (float x, float y, float z) Vector3ToTuple ( Vector3 v )
        {
            (float x, float y, float z) t;

            t.x = v.x;
            t.y = v.y;
            t.z = v.z;

            return t;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void Swap<T> ( ref T a, ref T b ) => (a, b) = (b, a);
    }
}