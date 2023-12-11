using System.IO;
using Drawing = System.Drawing;
using System.Numerics;
using ObjRenderer.Maps;

namespace ObjRenderer.Parsing
{
    public static class MapFileParser
    {
        public static NormalMap? LoadNormalMap(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

                Image<Rgba32> mapImage = Image.Load<Rgba32>(stream);

                int width = mapImage.Width;
                int height = mapImage.Height;

                NormalMap map = new(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rgba32 pixel = mapImage[x, y];

                        Vector3 value = new()
                        {
                            X = pixel.R / 255f * 2 - 1,
                            Y = pixel.G / 255f * 2 - 1,
                            Z = pixel.B / 255f * 2 - 1
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

        public static DiffuseMap? LoadDiffuseMap(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

                Image<Rgba32> mapImage = Image.Load<Rgba32>(stream);

                int width = mapImage.Width;
                int height = mapImage.Height;

                DiffuseMap map = new(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rgba32 pixel = mapImage[x, y];

                        Drawing.Color value = Drawing.Color.FromArgb(pixel.R, pixel.G, pixel.B);

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

        public static SpecularMap? LoadSpecularMap(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

                Image<Rgba32> mapImage = Image.Load<Rgba32>(stream);

                int width = mapImage.Width;
                int height = mapImage.Height;

                SpecularMap map = new(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rgba32 pixel = mapImage[x, y];

                        float value = pixel.R / 255f;

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

        private const string NXEdgeName = "nx.png";
        private const string NYEdgeName = "ny.png";
        private const string NZEdgeName = "nz.png";
        private const string PXEdgeName = "px.png";
        private const string PYEdgeName = "py.png";
        private const string PZEdgeName = "pz.png";

        public static CubeMap? LoadCubeMap(string path)
        {
            try
            {
                CubeMap map = new(LoadDiffuseMap(path + PXEdgeName)!,
                    LoadDiffuseMap(path + PYEdgeName)!,
                    LoadDiffuseMap(path + PZEdgeName)!,
                    LoadDiffuseMap(path + NXEdgeName)!,
                    LoadDiffuseMap(path + NYEdgeName)!,
                    LoadDiffuseMap(path + NZEdgeName)!
                );

                return map;
            }
            catch
            {
                return null;
            }
        }
    }
}
