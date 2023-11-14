using ObjRenderer.Models;
using System.Drawing;
using System.Numerics;
using static System.Math;

namespace ObjRenderer.Rendering
{
    public class Drawer
    {
        private const float InitialZBufferValue = float.MinValue;

        private readonly float[] _preinitializedBuffer;
        private readonly object[] _zBufferLocks;

        private float[] _zBuffer;

        private readonly int _width, _height;

        private readonly Vector3 _diffuseColor;
        private readonly Vector3 _ambientColor;
        private readonly Vector3 _specularColor;

        private Bitmap _bitmap;
        public Drawer(int width, int height, Vector3 modelColor, Vector3 backgroundColor, Vector3 glitterColor)
        {
            _width = width;
            _height = height;
            _diffuseColor = modelColor;
            _ambientColor = backgroundColor;
            _specularColor = glitterColor;

            _zBufferLocks = new object[width * height];
            _preinitializedBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++)
            {
                _preinitializedBuffer[i] = InitialZBufferValue;
                _zBufferLocks[i] = new();
            }
        }

        public Bitmap DrawBitmap(List<Face> faces, List<Vector4> transformedVertices, List<Vector4> vectices, List<Vector3> normals, Vector3 lightingVector, Vector3 eyeVector)
        {
            _zBuffer = GetZBuffer();

            lightingVector = Vector3.Normalize(lightingVector);

            _bitmap = new Bitmap(_width, _height);
            _bitmap.Source.Lock();

            Parallel.ForEach(faces, (face) =>
            {
                Vector3 transformedPoint1 = transformedVertices[face.p0.index].ToVector3();
                Vector3 transformedPoint2 = transformedVertices[face.p1.index].ToVector3();
                Vector3 transformedPoint3 = transformedVertices[face.p2.index].ToVector3();

                Vector3 p1 = vectices[face.p0.index].ToVector3();
                Vector3 p2 = vectices[face.p1.index].ToVector3();
                Vector3 p3 = vectices[face.p2.index].ToVector3();

                Vector3 normal1 = face.p0.normalIndex.HasValue ? normals[face.p0.normalIndex.Value] : CalculateNormal(p1, p2, p3);
                Vector3 normal2 = face.p1.normalIndex.HasValue ? normals[face.p1.normalIndex.Value] : CalculateNormal(p1, p2, p3);
                Vector3 normal3 = face.p2.normalIndex.HasValue ? normals[face.p2.normalIndex.Value] : CalculateNormal(p1, p2, p3);

                Vector2 p1p2 = new(transformedPoint2.X - transformedPoint1.X, transformedPoint2.Y - transformedPoint1.Y);
                Vector2 p1p3 = new(transformedPoint3.X - transformedPoint1.X, transformedPoint3.Y - transformedPoint1.Y);

                if (PerpDot(p1p3, p1p2) > 0)
                {
                    Rasterize(_bitmap, transformedPoint1, transformedPoint2, transformedPoint3, p1, p2, p3, normal1, normal2, normal3, lightingVector, eyeVector);
                }
            });

            _bitmap.Source.AddDirtyRect(new(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Source.Unlock();

            return _bitmap;
        }

        private void Rasterize(Bitmap bitmap, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p01, Vector3 p02, Vector3 p03, Vector3 n1, Vector3 n2, Vector3 n3, Vector3 lighting, Vector3 eye)
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
                            if (_zBuffer[pixelIndex] < zValue)
                            {
                                _zBuffer[pixelIndex] = zValue;

                                Vector3 pixelPoint = b.X * p01 + b.Y * p02 + b.Z * p03;
                                Vector3 pixelNormal = b.X * n1 + b.Y * n2 + b.Z * n3;

                                (byte red, byte green, byte blue) = ComputePixelColor(pixelPoint, pixelNormal, lighting, eye);

                                bitmap.SetPixel(x, y, red, green, blue);
                            }
                        }

                    }
                }
            }

        }

        private (byte red, byte green, byte blue) ComputePixelColor(Vector3 pixelPoint, Vector3 pixelNormal, Vector3 lighting, Vector3 eye)
        {
            const float AmbientIntensity = 0.25f;
            const float GlossFactor = 8f;

            byte sRed, sGreen, sBlue;
            byte dRed, dGreen, dBlue;
            byte aRed, aGreen, aBlue;

            Vector3 eyeToPoint = Vector3.Normalize(pixelPoint - eye);
            Vector3 pointNormal = Vector3.Normalize(pixelNormal);

            float diffuseK = Max(Vector3.Dot(pointNormal, lighting), 0.0f);
            float specularK = (float)Pow(Max(Vector3.Dot(Vector3.Normalize(Vector3.Reflect(lighting, pointNormal)), eyeToPoint), 0.0f), GlossFactor);
            float ambientK = AmbientIntensity;

            sRed = (byte)Min(_specularColor.X * specularK, 255);
            sGreen = (byte)Min(_specularColor.Y * specularK, 255);
            sBlue = (byte)Min(_specularColor.Z * specularK, 255);

            dRed = (byte)Min(_diffuseColor.X * diffuseK, 255);
            dGreen = (byte)Min(_diffuseColor.Y * diffuseK, 255);
            dBlue = (byte)Min(_diffuseColor.Z * diffuseK, 255);

            aRed = (byte)Min(_ambientColor.X * ambientK, 255);
            aGreen = (byte)Min(_ambientColor.Y * ambientK, 255);
            aBlue = (byte)Min(_ambientColor.Z * ambientK, 255);

            byte red = (byte)Min(sRed + dRed + aRed, 255);
            byte green = (byte)Min(sGreen + dGreen + aGreen, 255);
            byte blue = (byte)Min(sBlue + dBlue + aBlue, 255);

            return (red, green, blue);
        }

        private static float PerpDot(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private float[] GetZBuffer()
        {
            return (float[])_preinitializedBuffer.Clone();
        }

        public static Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 side1 = p2 - p1;
            Vector3 side2 = p3 - p1;

            Vector3 normal = Vector3.Cross(side1, side2);
            normal = Vector3.Normalize(normal);

            return normal;
        }
    }
}
