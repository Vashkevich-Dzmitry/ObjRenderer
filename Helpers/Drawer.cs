using ObjRenderer.Models;
using System.Drawing;
using System.Numerics;

namespace ObjRenderer.Helpers
{
    public static class Drawer
    {
        public static Bitmap DrawBitmap(List<IList<FaceDescription>> faces, List<Vector4> vertices, int width, int height, System.Drawing.Color color)
        {
            Bitmap bitmap = new(width, height);
            bitmap.Source.Lock();

            Parallel.ForEach(faces, (face, state, index) =>
            {
                var vertex = vertices.ElementAt(face[0].VertexIndex);
                var previousPoint = to2DPoint(vertex);
                for (var i = 1; i < face.Count; ++i)
                {
                    int index1 = face[i].VertexIndex;
                    var vertex1 = vertices.ElementAt(index1);
                    var currentPoint = to2DPoint(vertex1);
                    DrawLineIfFits(bitmap, previousPoint, currentPoint, color);
                    previousPoint = currentPoint;
                }
            });

            bitmap.Source.AddDirtyRect(new(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Source.Unlock();

            return bitmap;
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

        private static Point to2DPoint(Vector4 vertex)
        {
            return new((int)vertex.X, (int)vertex.Y);
        }
    }
}
