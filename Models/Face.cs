namespace ObjRenderer.Models
{
    public readonly struct Face
    {
        public readonly FaceVertex p1;
        public readonly FaceVertex p2;
        public readonly FaceVertex p3;

        public Face(FaceVertex p1, FaceVertex p2, FaceVertex p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
}
