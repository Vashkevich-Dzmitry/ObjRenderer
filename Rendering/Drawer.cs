using ObjRenderer.Models;
using System.Drawing;
using System.Numerics;
using System.Windows.Media.Media3D;
using static System.Math;

namespace ObjRenderer.Rendering
{
    public class Drawer
    {
        private const float _initialValue = float.MinValue;
        private readonly float[] _preinitializedBuffer;

        private readonly object[] _zBufferLocks;

        private readonly int _width, _height;

        private Bitmap _bitmap;
        public Drawer(int width, int height)
        {
            _width = width;
            _height = height;

            _zBufferLocks = new object[width * height];
            _preinitializedBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++)
            {
                _preinitializedBuffer[i] = _initialValue;
                _zBufferLocks[i] = new();
            }
        }

        public Bitmap DrawBitmap(List<Face> faces, List<Vector4> vertices, List<Vector3> normals, Color baseColor, Vector3 lightingVector)
        {
            float[] zBuffer = GetZBuffer();

            _bitmap = new Bitmap(_width, _height);
            _bitmap.Source.Lock();

            Parallel.ForEach(faces, (face) =>
            {
                Vector3 color = CalculateColor(lightingVector, face, normals, baseColor);

                Vector3 point1 = vertices.ElementAt(face.p0.index).ToVector3();
                Vector3 point2 = vertices.ElementAt(face.p1.index).ToVector3();
                Vector3 point3 = vertices.ElementAt(face.p2.index).ToVector3();

                Rasterize(_bitmap, point1, point2, point3, color, zBuffer);
            });

            _bitmap.Source.AddDirtyRect(new(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Source.Unlock();

            return _bitmap;
        }

        private void Rasterize(Bitmap bitmap, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 color, float[] zBuffer)
        {
            int xMin = (int)Max(0, Ceiling(Min(Min(p1.X, p2.X), p3.X)));
            int yMin = (int)Max(0, Ceiling(Min(Min(p1.Y, p2.Y), p3.Y)));
            int xMax = (int)Min(bitmap.PixelWidth, Ceiling(Max(Max(p1.X, p2.X), p3.X)));
            int yMax = (int)Min(bitmap.PixelHeight, Ceiling(Max(Max(p1.Y, p2.Y), p3.Y)));

            Vector2 p1p2 = new(p2.X - p1.X, p2.Y - p1.Y);
            Vector2 p2p3 = new(p3.X - p2.X, p3.Y - p2.Y);
            Vector2 p3p1 = new(p1.X - p3.X, p1.Y - p3.Y);

            float denom = 1 / PerpDot(-p3p1, p1p2);

            Vector2 pp1 = new(p1.X - xMin, p1.Y - yMin);
            Vector2 pp2 = new(p2.X - xMin, p2.Y - yMin);
            Vector2 pp3 = new(p3.X - xMin, p3.Y - yMin);

            Vector3 b0 = denom * new Vector3()
            {
                X = PerpDot(pp3, pp2),
                Y = PerpDot(pp1, pp3),
                Z = PerpDot(pp2, pp1)
            };

            Vector3 dbdx = denom * new Vector3()
            {
                X = pp3.Y - pp2.Y,
                Y = pp1.Y - pp3.Y,
                Z = pp2.Y - pp1.Y,
            };

            Vector3 dbdy = denom * new Vector3()
            {
                X = pp2.X - pp3.X,
                Y = pp3.X - pp1.X,
                Z = pp1.X - pp2.X,
            };

            for (int y = yMin; y < yMax; y++, b0 += dbdy)
            {
                Vector3 b = b0;
                for (int x = xMin; x < xMax; x++, b += dbdx)
                {
                    if (b.X > 0 && b.Y > 0 && b.Z > 0 
                        || b.X == 0 && (p2p3.Y > 0 || p2p3.Y == 0 && p2p3.X < 0) 
                        || b.Y == 0 && (p3p1.Y > 0 || p3p1.Y == 0 && p3p1.X < 0) 
                        || b.Z == 0 && (p1p2.Y > 0 || p1p2.Y == 0 && p1p2.X < 0))
                    {
                        float zValue = b.X * p1.Z + b.Y * p2.Z + b.Z * p3.Z;

                        int pixelIndex = y * bitmap.PixelWidth + x;

                        lock (_zBufferLocks[pixelIndex])
                        {
                            if (zBuffer[pixelIndex] < zValue)
                            {
                                zBuffer[pixelIndex] = zValue;

                                bitmap.SetPixel(x, y, (byte)color.X, (byte)color.Y, (byte)color.Z);
                            }
                        }

                    }
                }
            }

        }

        private static float PerpDot(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private float[] GetZBuffer()
        {
            return (float[])_preinitializedBuffer.Clone();
        }

        private static Vector3 CalculateColor(Vector3 lightingVector, Face face, List<Vector3> normals, Color baseColor)
        {

            (float k1, float k2, float k3) = CalculateColorCoefficient(lightingVector, face, normals);

            float k = (k1 + k2 + k3) / 3;

            Vector3 color = new()
            {
                X = baseColor.R * k,
                Y = baseColor.G * k,
                Z = baseColor.B * k,

            };

            return color;
        }

        private static float GetColorCoefficientUsingLambertLightingModel(Vector3 lightingVector, Vector3 normalVector)
        {
            var normal = Vector3.Normalize(normalVector);
            var lighting = Vector3.Normalize(lightingVector);

            return Max(Vector3.Dot(normal, lighting), 0);
        }

        private static (float, float, float) CalculateColorCoefficient(Vector3 lightingVector, Face face, List<Vector3> normals)
        {
            Vector3 normal1 = normals.ElementAt(face.p0.normalIndex ?? 0);
            Vector3 normal2 = normals.ElementAt(face.p1.normalIndex ?? 0);
            Vector3 normal3 = normals.ElementAt(face.p2.normalIndex ?? 0);

            float k1 = GetColorCoefficientUsingLambertLightingModel(lightingVector, normal1);
            float k2 = GetColorCoefficientUsingLambertLightingModel(lightingVector, normal2);
            float k3 = GetColorCoefficientUsingLambertLightingModel(lightingVector, normal3);
            return (k1, k2, k3);
        }
    }
}
