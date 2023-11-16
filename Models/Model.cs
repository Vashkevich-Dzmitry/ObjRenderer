using System.Numerics;
using SixLabors.ImageSharp;

namespace ObjRenderer.Models
{
    public class Model
    {
        private const int InitialListSize = 10000;

        public readonly List<Face> Faces = new(InitialListSize);

        public List<Vector4> Vertices = new(InitialListSize);
        public List<Vector3> VertexNormals = new(InitialListSize);
        public List<Vector3> VertexTextures = new(InitialListSize);

        public NormalMap? NormalMap;
        public DiffuseMap? DiffuseMap;
        public SpecularMap? SpecularMap;

        public Vector3 Center
        {
            get
            {
                return new()
                {
                    X = (Vertices.Max(v => v.X) + Vertices.Min(v => v.X)) / 2,
                    Y = (Vertices.Max(v => v.Y) + Vertices.Min(v => v.Y)) / 2,
                    Z = (Vertices.Max(v => v.Z) + Vertices.Min(v => v.Z)) / 2
                };
            }
        }

        public Vector3 Size
        {
            get
            {
                return new()
                {
                    X = Vertices.Max(v => v.X) - Vertices.Min(v => v.X),
                    Y = Vertices.Max(v => v.Y) - Vertices.Min(v => v.Y),
                    Z = Vertices.Max(v => v.Z) - Vertices.Min(v => v.Z)
                };
            }
        }
    }
}
