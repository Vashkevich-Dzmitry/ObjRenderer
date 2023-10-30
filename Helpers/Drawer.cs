using ObjRenderer.Models;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Shapes;

namespace ObjRenderer.Helpers
{
    public static class Drawer
    {
        public static Bitmap DrawBitmap(List<IList<FaceDescription>> faces, List<Vector4> vertices, List<Vector3> normals, int width, int height, Color color, Vector3 lightingVector)
        {
            double[,] zBuffer = GetZBuffer(width, height);

            Bitmap bitmap = new(width, height);
            bitmap.Source.Lock();

            Parallel.ForEach(faces, (face) =>
            {
                Color faceColor = CalculateColor(lightingVector, face, normals, color);

                List<Vector4> points = new(face.Select(f => vertices.ElementAt(f.VertexIndex)));

                Rasterize(bitmap, points, faceColor, zBuffer);
            });

            bitmap.Source.AddDirtyRect(new(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Source.Unlock();

            return bitmap;
        }

        private static void Rasterize(Bitmap bitmap, List<Vector4> points, Color color, double[,] zBuffer)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            List<Point> facePixels = CalculateFacePixels(points, zBuffer, width, height);

            foreach(var pixel in facePixels)
            {
                bitmap.SetPixel(pixel.X, pixel.Y, color.R, color.G, color.B);
            }
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

        private static List<Point> CalculateFacePixels(List<Vector4> points, double[,] zBuffer, int width, int height)
        {
            List<Point> resultPixels = new();

            points = points.OrderBy(p => p.Y).ToList();

            float x0 = points[0].X;
            float y0 = points[0].Y;
            float z0 = points[0].Z;
            float x1 = points[1].X;
            float y1 = points[1].Y;
            float z1 = points[1].Z;
            float x2 = points[2].X;
            float y2 = points[2].Y;
            float z2 = points[2].Z;

            float currentY = y0;

            while (currentY <= y1)
            {
                float targetX1 = Interpolate(currentY, x0, y0, x2, y2);
                float targetX2 = Interpolate(currentY, x0, y0, x1, y1);
                float targetZ1 = Interpolate(currentY, z0, y0, z2, y2);
                float targetZ2 = Interpolate(currentY, z0, y0, z1, y1);

                if (targetX1 > targetX2)
                {
                    (targetX1, targetX2) = (targetX2, targetX1);
                }

                float currentX = targetX1;

                while (currentX <= targetX2)
                {
                    int x = (int)Math.Floor(currentX);
                    int y = (int)Math.Ceiling(currentY);

                    double zValue = Interpolate(currentX, targetZ1, targetX1, targetZ2, targetX2);

                    if (x > 0 && x < width && y > 0 && y < height && zBuffer[x, y] > zValue)
                    {
                        resultPixels.Add(new(x, y));
                        zBuffer[x, y] = zValue;
                    }

                    currentX += 1.0f;
                }

                currentY += 1.0f;
            }

            currentY = y1;

            while (currentY <= y2) ///ToDo cycle in function
            {
                float targetX1 = Interpolate(currentY, x0, y0, x2, y2);
                float targetX2 = Interpolate(currentY, x1, y1, x2, y2);
                float targetZ1 = Interpolate(currentY, z0, y0, z2, y2);
                float targetZ2 = Interpolate(currentY, z1, y1, z2, y2);

                if (targetX1 > targetX2)
                {
                    (targetX1, targetX2) = (targetX2, targetX1);
                }

                float currentX = targetX1;

                while (currentX <= targetX2)
                {
                    int x = (int)Math.Floor(currentX);
                    int y = (int)Math.Ceiling(currentY);

                    double zValue = Interpolate(currentX, targetZ1, targetX1, targetZ2, targetX2);

                    if (x > 0 && x < width && y > 0 && y < height && zBuffer[x, y] > zValue)
                    {
                        resultPixels.Add(new(x, y));
                        zBuffer[x, y] = zValue;
                    }

                    currentX += 1.0f;
                }

                currentY += 1.0f;
            }

            return resultPixels;
        }

        private static bool ZeroCheck(float v1, float v2)
        {
            return Math.Abs(v1 - v2) < 0.0000001f;
        }

        public static float Interpolate(float targetY, float x1, float y1, float x2, float y2)
        {
            if (ZeroCheck(y1, y2))
                return x1;
            else
                return (x1 - x2) / (y1 - y2) * (targetY - y1) + x1;
        }

        /// <summary>
        /// Рассчитывает цвет полигона face, используя модель освещения Ламберта
        /// </summary>
        /// <param name="lightingVector"></param>
        /// <param name="face"></param>
        /// <param name="normals"></param>
        /// <param name="baseColor"></param>
        /// <returns>Цвет полигона</returns>
        private static Color CalculateColor(Vector3 lightingVector, IList<FaceDescription> face, List<Vector3> normals, Color baseColor)
        {
            double coefficient = CalculateColorCoefficient(lightingVector, face, normals);

            int red = (int)Math.Round(baseColor.R * coefficient);
            int green = (int)Math.Round(baseColor.G * coefficient);
            int blue = (int)Math.Round(baseColor.B * coefficient);

            return Color.FromArgb(red, green, blue);
        }

        /// <summary>
        /// Определяет долю базового цвета, необходимую для получения цвета вершины полигона
        /// </summary>
        /// <param name="lightingVector"></param>
        /// <param name="normalVector"></param>
        /// <returns>Доля базового цвета, необходимую для получения цвета вершины полигона</returns>
        private static double GetColorCoefficientUsingLambertLightingModel(Vector3 lightingVector, Vector3 normalVector)
        {
            var normal = Vector3.Normalize(normalVector);
            var lighting = Vector3.Normalize(lightingVector);

            return Math.Max(Vector3.Dot(normal, lighting), 0);
        }

        /// <summary>
        /// Определяет долю базового цвета, необходимую для получения цвета полигона
        /// </summary>
        /// <param name="lightingVector"></param>
        /// <param name="face"></param>
        /// <param name="normals"></param>
        /// <returns>Цвет полигона</returns>
        private static double CalculateColorCoefficient(Vector3 lightingVector, IList<FaceDescription> face, List<Vector3> normals)
        {
            double coefficient = 0;

            var faceNormals = face.Select(f => normals.ElementAt(f.VertexNormalIndex ?? 0));
            foreach (var normal in faceNormals)
            {
                coefficient += GetColorCoefficientUsingLambertLightingModel(lightingVector, normal);
            }

            coefficient /= face.Count;

            return coefficient;
        }

        private static void DrawLineIfFits(Bitmap bitmap, Point previousPoint, Point currentPoint, System.Drawing.Color color)
        {
            if ((previousPoint.X >= 0 && previousPoint.Y >= 0 && previousPoint.X < bitmap.PixelWidth && previousPoint.Y < bitmap.PixelHeight)
                || (currentPoint.X >= 0 && currentPoint.Y >= 0 && currentPoint.X < bitmap.PixelWidth && currentPoint.Y < bitmap.PixelHeight))
            {
                DrawLine(bitmap, previousPoint, currentPoint, color);
            }
        }

        private static void DrawLine(Bitmap bitmap, Point previousPoint, Point currentPoint, System.Drawing.Color color)
        {
            int x0 = previousPoint.X;
            int y0 = previousPoint.Y;
            int x1 = currentPoint.X;
            int y1 = currentPoint.Y;

            int deltaX = Math.Abs(x1 - x0);
            int deltaY = Math.Abs(y1 - y0);
            int x = x0;
            int y = y0;
            int xIncrement = x0 < x1 ? 1 : -1;
            int yIncrement = y0 < y1 ? 1 : -1;
            int error = deltaX - deltaY;

            bitmap.SetPixel(x, y, color.R, color.G, color.B);

            while (x != x1 || y != y1)
            {
                int error2 = error * 2;

                if (error2 > -deltaY)
                {
                    error -= deltaY;
                    x += xIncrement;
                }

                if (error2 < deltaX)
                {
                    error += deltaX;
                    y += yIncrement;
                }

                bitmap.SetPixel(x, y, color.R, color.G, color.B);
            }
        }
    }
}
