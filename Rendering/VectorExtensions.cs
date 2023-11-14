using System.Numerics;

namespace ObjRenderer.Rendering
{
    public static class VectorExtensions
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
    }
}