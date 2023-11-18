using ObjRenderer.Models;
using Drawing = System.Drawing;
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

        private readonly Drawing.Color _diffuseColor;
        private readonly Drawing.Color _ambientColor;
        private readonly Drawing.Color _specularColor;

        private Bitmap _bitmap;
        public Drawer(int width, int height, Drawing.Color modelColor, Drawing.Color backgroundColor, Drawing.Color specularColor)
        {
            _width = width;
            _height = height;
            _diffuseColor = modelColor;
            _ambientColor = backgroundColor;
            _specularColor = specularColor;

            _zBufferLocks = new object[width * height];
            _preinitializedBuffer = new float[width * height];
            for (int i = 0; i < width * height; i++)
            {
                _preinitializedBuffer[i] = InitialZBufferValue;
                _zBufferLocks[i] = new();
            }
        }

        public Bitmap DrawBitmap(List<Face> faces, List<Vector4> transformedVertices, Model model, Vector3 lightingVector, Vector3 eyeVector)
        {
            _zBuffer = GetZBuffer();

            lightingVector = Vector3.Normalize(lightingVector);

            _bitmap = new Bitmap(_width, _height);
            _bitmap.Source.Lock();

            Parallel.ForEach(faces, (face) =>
            {
                Vector3 transformedPoint1 = transformedVertices[face.p1.index].ToVector3();
                Vector3 transformedPoint2 = transformedVertices[face.p2.index].ToVector3();
                Vector3 transformedPoint3 = transformedVertices[face.p3.index].ToVector3();

                Vector3 p1 = model.Vertices[face.p1.index].ToVector3();
                Vector3 p2 = model.Vertices[face.p2.index].ToVector3();
                Vector3 p3 = model.Vertices[face.p3.index].ToVector3();

                Vector3 normal1 = face.p1.normalIndex.HasValue ? model.VertexNormals[face.p1.normalIndex.Value] : CalculateNormal(p1, p2, p3);
                Vector3 normal2 = face.p2.normalIndex.HasValue ? model.VertexNormals[face.p2.normalIndex.Value] : CalculateNormal(p1, p2, p3);
                Vector3 normal3 = face.p3.normalIndex.HasValue ? model.VertexNormals[face.p3.normalIndex.Value] : CalculateNormal(p1, p2, p3);

                Vector3 vt1 = face.p1.textureIndex.HasValue ? model.VertexTextures[face.p1.textureIndex.Value] : -Vector3.One;
                Vector3 vt2 = face.p2.textureIndex.HasValue ? model.VertexTextures[face.p2.textureIndex.Value] : -Vector3.One;
                Vector3 vt3 = face.p3.textureIndex.HasValue ? model.VertexTextures[face.p3.textureIndex.Value] : -Vector3.One;

                Vector2 p1p2 = new(transformedPoint2.X - transformedPoint1.X, transformedPoint2.Y - transformedPoint1.Y);
                Vector2 p1p3 = new(transformedPoint3.X - transformedPoint1.X, transformedPoint3.Y - transformedPoint1.Y);

                if (PerpDot(p1p3, p1p2) > 0)
                {
                    Rasterize(_bitmap, transformedPoint1, transformedPoint2, transformedPoint3, p1, p2, p3, normal1, normal2, normal3, vt1, vt2, vt3, lightingVector, eyeVector, model.NormalMap, model.DiffuseMap, model.SpecularMap);
                }
            });

            _bitmap.Source.AddDirtyRect(new(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Source.Unlock();

            return _bitmap;
        }

        private void Rasterize(Bitmap bitmap, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p01, Vector3 p02, Vector3 p03, Vector3 n1, Vector3 n2, Vector3 n3, Vector3 vt1, Vector3 vt2, Vector3 vt3, Vector3 lighting, Vector3 eye, NormalMap? normalMap, DiffuseMap? diffuseMap, SpecularMap? specularMap)
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
                                Vector3 pixelPoint = b.X * p01 + b.Y * p02 + b.Z * p03;
                                Vector3 pixelTexture = b.X * vt1 + b.Y * vt2 + b.Z * vt3;

                                Vector3 pixelNormal = normalMap != null ? normalMap.GetValue(pixelTexture.X, pixelTexture.Y) : b.X * n1 + b.Y * n2 + b.Z * n3;

                                float specularCoefficient = specularMap != null ? specularMap.GetValue(pixelTexture.X, pixelTexture.Y) : 1f;
                                float diffuseCoefficient = 0.6f;
                                float ambientCoefficient = 0.4f;

                                Drawing.Color diffuseColor = diffuseMap != null ? diffuseMap.GetValue(pixelTexture.X, pixelTexture.Y) : _diffuseColor;
                                Drawing.Color ambientColor = diffuseMap != null ? diffuseColor : _ambientColor;
                                Drawing.Color specularColor = _specularColor;

                                (byte red, byte green, byte blue) = ComputePixelColor(pixelPoint, pixelNormal, lighting, eye, diffuseColor, ambientColor, specularColor, diffuseCoefficient, ambientCoefficient, specularCoefficient);

                                _zBuffer[pixelIndex] = zValue;

                                bitmap.SetPixel(x, y, red, green, blue);
                            }
                        }

                    }
                }
            }

        }

        private static (byte red, byte green, byte blue) ComputePixelColor(Vector3 pixelPoint, Vector3 pixelNormal, Vector3 lighting, Vector3 eye, Drawing.Color diffuseColor, Drawing.Color ambientColor, Drawing.Color specularColor, float diffuseK, float ambientK, float specularK)
        {
            const int GlossFactor = 2;

            byte sRed, sGreen, sBlue;
            byte dRed, dGreen, dBlue;
            byte aRed, aGreen, aBlue;

            Vector3 eyeToPoint = Vector3.Normalize(pixelPoint - eye);
            Vector3 pointNormal = Vector3.Normalize(pixelNormal);

            float diffuseIntensity = Max(Vector3.Dot(pointNormal, lighting), 0.0f);

            Vector3 reflectedLighting = Vector3.Reflect(lighting, pointNormal);
            float dot = Vector3.Dot(reflectedLighting, eyeToPoint);
            float k = Max(dot, 0.0f);
            float specularIntensity = MyPow(k, GlossFactor);

            sRed = (byte)(specularColor.R * specularIntensity);
            sGreen = (byte)(specularColor.G * specularIntensity);
            sBlue = (byte)(specularColor.B * specularIntensity);

            dRed = (byte)(diffuseColor.R * diffuseIntensity);
            dGreen = (byte)(diffuseColor.G * diffuseIntensity);
            dBlue = (byte)(diffuseColor.B * diffuseIntensity);

            aRed = ambientColor.R;
            aGreen = ambientColor.G;
            aBlue = ambientColor.B;

            byte red = (byte)Min(sRed * specularK + dRed * diffuseK + aRed * ambientK, 255);
            byte green = (byte)Min(sGreen * specularK + dGreen * diffuseK + aGreen * ambientK, 255);
            byte blue = (byte)Min(sBlue * specularK + dBlue * diffuseK + aBlue * ambientK, 255);

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

        private static Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 side1 = p2 - p1;
            Vector3 side2 = p3 - p1;

            Vector3 normal = Vector3.Cross(side1, side2);
            normal = Vector3.Normalize(normal);

            return normal;
        }

        private static float MyPow(float number, int power)
        {
            if (number == 0.0f) return 0.0f;

            float result = 1.0f;
            while (power > 0)
            {
                if (power % 2 == 1)
                    result *= number;
                power >>= 1;
                number *= number;
            }

            return result;
        }
    }
}
