using System.Numerics;

namespace ObjRenderer.Models
{
    public class Model
    {
        private const int InitialListSize = 10000;

        public List<Vector4> Vertices = new(InitialListSize);
        public readonly List<IList<FaceDescription>> Faces = new(InitialListSize);
        
        public readonly List<Vector3> VertexTextures = new(InitialListSize);
        public readonly List<Vector3> VertexNormals = new(InitialListSize);

        public Dimensions Size
        {
            get
            {
                var size = new Dimensions();

                if (Vertices.Count > 0)
                {
                    size.XMax = Vertices.Max(v => v.X);
                    size.XMin = Vertices.Min(v => v.X);
                    size.YMax = Vertices.Max(v => v.Y);
                    size.YMin = Vertices.Min(v => v.Y);
                    size.ZMax = Vertices.Max(v => v.Z);
                    size.ZMin = Vertices.Min(v => v.Z);
                }
                
                return size;
            }
        }
    }
}
