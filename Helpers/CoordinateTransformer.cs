using System.Numerics;

namespace ObjRenderer.Helpers
{
    public static class CoordinateTransformer
    {
        public static ParallelQuery<Vector4> ApplyTransform(this ParallelQuery<Vector4> vectors, Matrix4x4 transformationMatrix)
        {
            return vectors.Select(v => Vector4.Transform(v, transformationMatrix));
        }

        public static ParallelQuery<Vector4> DivideByW(this ParallelQuery<Vector4> vectors)
        {
            return vectors.Select(v => new Vector4(v.X / v.W, v.Y / v.W, v.Z / v.W, 1));
        }

        /// Viewport matrix
        /// |   width / 2   |        0       |          0          | 0 |
        /// |       0       |   -height / 2  |          0          | 0 |
        /// |       0       |        0       | maxDepth - minDepth | 0 |
        /// | x + width / 2 | y + height / 2 |       minDepth      | 1 |
        public static Matrix4x4 GetViewPortMatrix(float width, float height, float? minX = null, float? minY = null, float? minDepth = null, float? maxDepth = null)
        {
            return Matrix4x4.CreateViewport(minX ?? 0.0f, minY ?? 0.0f, width, height, minDepth ?? 0.0f, maxDepth ?? 1.0f);
        }


        /// World matrix
        public static Matrix4x4 GetWorldMatrix(Vector3 position, Vector3 forward, Vector3? up = null)
        {
            return Matrix4x4.CreateWorld(position, forward, up ?? Vector3.UnitY);
        }

        /// Scale matrix
        public static Matrix4x4 GetScaleMatrix(float scaleX, float scaleY, float scaleZ)
        {
            return Matrix4x4.CreateScale(scaleX, scaleY, scaleZ);
        }

        /// Scale matrix
        public static Matrix4x4 GetScaleMatrix(float scale)
        {
            return Matrix4x4.CreateScale(scale, scale, scale);
        }

        /// View matrix
        public static Matrix4x4 GetViewMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            var d = target - eye;
            var newUp = Vector3.Cross(Vector3.Cross(d, up), d);
            return Matrix4x4.CreateLookAt(eye, target, newUp);
        }

        /// Projection Matrix
        public static Matrix4x4 GetProjectionMatrix(float fieldOfView, float aspectRatio, float nearPlaneDistance,
            float farPlaneDistance)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView * (float)Math.PI / 180, aspectRatio,
                nearPlaneDistance, farPlaneDistance);
        }
    }
}
