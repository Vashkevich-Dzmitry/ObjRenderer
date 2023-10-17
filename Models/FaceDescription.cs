namespace ObjRenderer.Models
{
    public readonly struct FaceDescription
    {
        public int VertexIndex { get; }
        public int? VertexTextureIndex { get; }
        public int? VertexNormalIndex { get; }

        public FaceDescription(int vertexIndex, int? vertexTextureIndex = null, int? vertexNormalIndex = null)
        {
            VertexIndex = vertexIndex;
            VertexTextureIndex = vertexTextureIndex;
            VertexNormalIndex = vertexNormalIndex;
        }
    }
}