using ObjRenderer.Models;
using System.IO;
using System.Numerics;

namespace ObjRenderer.Parsing
{
    public static class MapFileParser
    {
        public static Map? LoadMap(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

                Image<Rgba32> mapImage = Image.Load<Rgba32>(stream);

                int width = mapImage.Width;
                int height = mapImage.Height;

                Map map = new(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rgba32 pixel = mapImage[x, y];

                        Vector3 value = new()
                        {
                            X = pixel.R,
                            Y = pixel.G,
                            Z = pixel.B
                        };

                        map.SetValue(x, height - y - 1, value);
                    }
                }

                return map;
            }
            catch
            {
                return null;
            }
        }
    }
}
