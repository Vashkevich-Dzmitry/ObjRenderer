using ObjRenderer.Models;
using System.Drawing;
using System.Numerics;
using static System.Math;

namespace ObjRenderer.Rendering
{
    public static class Drawer
    {
        public static Bitmap DrawBitmap(List<IList<FaceDescription>> faces, List<Vector4> vertices, List<Vector3> normals, int width, int height, Color baseColor, Vector3 lightingVector)
        {
            float[,] zBuffer = GetZBuffer(width, height);

            Bitmap bitmap = new(width, height);
            bitmap.Source.Lock();

            Parallel.ForEach(faces, (face) =>
            {
                Vector3 color = CalculateColor(lightingVector, face, normals, baseColor);

                List<Vector4> points = face.Select(f => vertices.ElementAt(f.VertexIndex)).ToList();

                (Vector3 point1, Vector3 point2, Vector3 point3) = (
                    new(points[0].X, points[0].Y, points[0].Z),
                    new(points[1].X, points[1].Y, points[1].Z),
                    new(points[2].X, points[2].Y, points[2].Z)
                    );

                Rasterize(bitmap, point1, point2, point3, color, zBuffer);
            });

            bitmap.Source.AddDirtyRect(new(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Source.Unlock();

            return bitmap;
        }

        private static void Rasterize(Bitmap bitmap, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 color, float[,] zBuffer)
        {
            int xMin = (int)Max(0, Ceiling(Min(Min(p1.X, p2.X), p3.X)));
            int yMin = (int)Max(0, Ceiling(Min(Min(p1.Y, p2.Y), p3.Y)));
            int xMax = (int)Min(bitmap.PixelWidth, Ceiling(Max(Max(p1.X, p2.X), p3.X)));
            int yMax = (int)Min(bitmap.PixelHeight, Ceiling(Max(Max(p1.Y, p2.Y), p3.Y)));

            Vector2 p1p2 = new(p2.X - p1.X, p2.Y - p1.Y);
            Vector2 p2p3 = new(p3.X - p2.X, p3.Y - p2.Y);
            Vector2 p3p1 = new(p1.X - p3.X, p1.Y - p3.Y);

            float denom = 1 / PerpDot(-p3p1, p1p2);

            bool f1 = p2p3.Y > 0 || p2p3.Y == 0 && p2p3.X < 0;
            bool f2 = p3p1.Y > 0 || p3p1.Y == 0 && p3p1.X < 0;
            bool f3 = p1p2.Y > 0 || p1p2.Y == 0 && p1p2.X < 0;

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
                    if (b.X > 0 && b.Y > 0 && b.Z > 0 || b.X == 0 && f1 || b.Y == 0 && f2 || b.Z == 0 && f3)
                    {
                        float zValue = (b.X * p1 + b.Y * p2 + b.Z * p3).Z;

                        if (zBuffer[x, y] < zValue)
                        {
                            zBuffer[x, y] = zValue;

                            bitmap.SetPixel(x, y, (byte)color.X, (byte)color.Y, (byte)color.Z);
                        }
                    }
                }
            }
        }

        private static float PerpDot(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private static float[,] GetZBuffer(int width, int height)
        {
            float[,] result = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = float.MinValue;
                }
            }

            return result;
        }

        private static Vector3 CalculateColor(Vector3 lightingVector, IList<FaceDescription> face, List<Vector3> normals, Color baseColor)
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

        private static (float, float, float) CalculateColorCoefficient(Vector3 lightingVector, IList<FaceDescription> face, List<Vector3> normals)
        {
            Vector3 normal1 = normals.ElementAt(face.ElementAtOrDefault(0).VertexNormalIndex ?? 0);
            Vector3 normal2 = normals.ElementAt(face.ElementAtOrDefault(1).VertexNormalIndex ?? 0);
            Vector3 normal3 = normals.ElementAt(face.ElementAtOrDefault(2).VertexNormalIndex ?? 0);

            float k1 = GetColorCoefficientUsingLambertLightingModel(lightingVector, normal1);
            float k2 = GetColorCoefficientUsingLambertLightingModel(lightingVector, normal2);
            float k3 = GetColorCoefficientUsingLambertLightingModel(lightingVector, normal3);
            return (k1, k2, k3);
        }
    }
}
