using System.Numerics;

namespace ObjRenderer.Rendering
{
    public static class Vector4Extensions
    {
        public static Vector3 ToVector3(this Vector4 vector4)
        {
            return new Vector3()
            {
                X = vector4.X,
                Y = vector4.Y,
                Z = vector4.Z
            };
        }

        public static Vector4 ToVector4(this Vector3 vector3)
        {
            return new Vector4()
            {
                X = vector3.X,
                Y = vector3.Y,
                Z = vector3.Z,
                W = 1
            };
        }
    }
}