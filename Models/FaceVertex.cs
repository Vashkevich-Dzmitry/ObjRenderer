namespace ObjRenderer.Models
{
    public readonly struct FaceVertex
    {
        public readonly int index;
        public readonly int? textureIndex;
        public readonly int? normalIndex;

        public FaceVertex(int vertexIndex, int? vertexTextureIndex = null, int? vertexNormalIndex = null)
        {
            index = vertexIndex;
            textureIndex = vertexTextureIndex;
            normalIndex = vertexNormalIndex;
        }
    }
}