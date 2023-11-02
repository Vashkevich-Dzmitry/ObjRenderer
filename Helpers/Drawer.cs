using ObjRenderer.Models;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static System.Math;

namespace ObjRenderer.Helpers
{
    public static class Drawer
    {
        public static Bitmap DrawBitmap(List<IList<FaceDescription>> faces, List<Vector4> vertices, List<Vector3> normals, int width, int height, Color baseColor, Vector3 lightingVector)
        {
            double[,] zBuffer = GetZBuffer(width, height);

            Bitmap bitmap = new(width, height);
            bitmap.Source.Lock();

            Parallel.ForEach(faces, (face) =>
            {
                (Vector3 color1, Vector3 color2, Vector3 color3) = CalculateColor(lightingVector, face, normals, baseColor);

                List<Vector4> points = face.Select(f => vertices.ElementAt(f.VertexIndex)).ToList();

                (Vector3 point1, Vector3 point2, Vector3 point3) = (
                    new(points[0].X, points[0].Y, points[0].Z),
                    new(points[1].X, points[1].Y, points[1].Z),
                    new(points[2].X, points[2].Y, points[2].Z)
                    );

                Rasterize(bitmap, point1, point2, point3, color1, color2, color3, zBuffer);
            });

            bitmap.Source.AddDirtyRect(new(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Source.Unlock();

            return bitmap;
        }

        private static void Rasterize(Bitmap bitmap, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 c1, Vector3 c2, Vector3 c3, double[,] zBuffer)
        {
            int xMin = (int)Max(0, Ceiling(Min(Min(p1.X, p2.X), p3.X)));
            int yMin = (int)Max(0, Ceiling(Min(Min(p1.Y, p2.Y), p3.Y)));
            int xMax = (int)Min(bitmap.PixelWidth, Ceiling(Max(Max(p1.X, p2.X), p3.X)));
            int yMax = (int)Min(bitmap.PixelHeight, Ceiling(Max(Max(p1.Y, p2.Y), p3.Y)));

            Vector2 p1p2 = new(p2.X - p1.X, p2.Y - p1.Y);
            Vector2 p2p3 = new(p3.X - p2.X, p3.Y - p2.Y);
            Vector2 p3p1 = new(p1.X - p3.X, p1.Y - p3.Y);

            float denom = 1 / PerpDot(-p3p1, p1p2);

            bool f1 = p2p3.Y > 0 || (p2p3.Y == 0 && p2p3.X < 0);
            bool f2 = p3p1.Y > 0 || (p3p1.Y == 0 && p3p1.X < 0);
            bool f3 = p1p2.Y > 0 || (p1p2.Y == 0 && p1p2.X < 0);

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
                    if (b.X > 0 && b.Y > 0 && b.Z > 0 || (b.X == 0 && f1) || (b.Y == 0 && f2) || (b.Z == 0 && f3))
                    {
                        Vector3 pixelColor = b.X * c1 + b.Y * c2 + b.Z * c3;
                        Vector3 pixelCoordinates = b.X * p1 + b.Y * p2 + b.Z * p3;
                        float pixelZValue = pixelCoordinates.Z;

                        if (zBuffer[x, y] > pixelZValue)
                        {
                            zBuffer[x, y] = pixelZValue;
                            bitmap.SetPixel(x, y, (byte)pixelColor.X, (byte)pixelColor.Y, (byte)pixelColor.Z);
                        }
                    }
                }
            }
        }

        private static float PerpDot(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private static double[,] GetZBuffer(int width, int height)
        {
            double[,] result = new double[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    result[i, j] = double.MaxValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Рассчитывает цвет полигона face, используя модель освещения Ламберта
        /// </summary>
        /// <param name="lightingVector"></param>
        /// <param name="face"></param>
        /// <param name="normals"></param>
        /// <param name="baseColor"></param>
        /// <returns>Цвет полигона</returns>
        private static (Vector3, Vector3, Vector3) CalculateColor(Vector3 lightingVector, IList<FaceDescription> face, List<Vector3> normals, Color baseColor)
        {

            (float k1, float k2, float k3) = CalculateColorCoefficient(lightingVector, face, normals);

            Vector3 color1 = new()
            {
                X = baseColor.R * k1,
                Y = baseColor.G * k1,
                Z = baseColor.B * k1,

            };

            Vector3 color2 = new()
            {
                X = baseColor.R * k2,
                Y = baseColor.G * k2,
                Z = baseColor.B * k2,

            };

            Vector3 color3 = new()
            {
                X = baseColor.R * k3,
                Y = baseColor.G * k3,
                Z = baseColor.B * k3,

            };

            return (color1, color2, color3);
        }

        /// <summary>
        /// Определяет долю базового цвета, необходимую для получения цвета вершины полигона
        /// </summary>
        /// <param name="lightingVector"></param>
        /// <param name="normalVector"></param>
        /// <returns>Доля базового цвета, необходимую для получения цвета вершины полигона</returns>
        private static float GetColorCoefficientUsingLambertLightingModel(Vector3 lightingVector, Vector3 normalVector)
        {
            var normal = Vector3.Normalize(normalVector);
            var lighting = Vector3.Normalize(lightingVector);

            return Math.Max(Vector3.Dot(normal, lighting), 0) * 0.5f + 0.5f;
        }

        /// <summary>
        /// Определяет долю базового цвета, необходимую для получения цвета полигона
        /// </summary>
        /// <param name="lightingVector"></param>
        /// <param name="face"></param>
        /// <param name="normals"></param>
        /// <returns>Цвет полигона</returns>
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
