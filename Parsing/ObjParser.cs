using System.Numerics;
using System.IO;
using ObjRenderer.Models;

namespace ObjRenderer.Parsing
{
    public static class ObjParser
    {
        private static readonly Dictionary<string, Action<Model, string[]>> parsers = new()
        {
            {"v", ParseVertex},
            {"vt", ParseVertexTexture},
            {"vn", ParseVertexNormal},
            {"f", ParseFace}
        };

        public static Model ParseObj(string path)
        {
            var model = new Model();
            foreach (var line in File.ReadAllLines(path).Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var type = parts[0];
                if (parsers.TryGetValue(type, out var parse)) parse(model, parts);
            }

            return model;
        }

        private static void ParseVertex(Model model, string[] parts)
        {
            model.Vertices.Add(
                new Vector4(
                    parts[1].ToFloat(),
                    parts[2].ToFloat(),
                    parts[3].ToFloat(),
                    parts.ElementAtOrDefault(4)?.ToNullableFloat() ?? 1.0f));
        }

        private static void ParseVertexTexture(Model model, string[] parts)
        {
            model.VertexTextures.Add(
                new Vector3(
                    parts[1].ToFloat(),
                    parts.ElementAtOrDefault(2)?.ToNullableFloat() ?? 0.0f,
                    parts.ElementAtOrDefault(3)?.ToNullableFloat() ?? 0.0f));
        }

        private static void ParseVertexNormal(Model model, string[] parts)
        {

            model.VertexNormals.Add(
                new Vector3(
                    parts[1].ToFloat(),
                    parts[2].ToFloat(),
                    parts[3].ToFloat()));

        }

        private static void ParseFace(Model model, string[] parts)
        {
            model.Faces.Add(
                parts.Skip(1).Select(part =>
                    {
                        var indices = part.Split('/');
                        return new FaceDescription(
                            GetNormalizedVertexIndex(indices[0].ToInt(), model.Vertices.Count),
                            GetNormalizedVertexIndexOrDefault(indices.ElementAtOrDefault(1)?.ToNullableInt(), model.VertexTextures.Count),
                            GetNormalizedVertexIndexOrDefault(indices.ElementAtOrDefault(2)?.ToNullableInt(), model.VertexNormals.Count));
                    }
                    ).ToArray());
        }

        private static int GetNormalizedVertexIndex(int index, int count)
        {
            return index > 0 ? --index : count - index;
        }

        private static int? GetNormalizedVertexIndexOrDefault(int? index, int count)
        {
            return index.HasValue ? index!.Value > 0 ? --index : count - index : null;
        }
    }
}
