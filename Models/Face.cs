namespace ObjRenderer.Models
{
    public readonly struct Face
    {
        public readonly FaceVertex p0;
        public readonly FaceVertex p1;
        public readonly FaceVertex p2;

        public Face(FaceVertex p0, FaceVertex p1, FaceVertex p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
