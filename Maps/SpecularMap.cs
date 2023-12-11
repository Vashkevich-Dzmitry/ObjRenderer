namespace ObjRenderer.Maps
{
    public class SpecularMap : Map<float>
    {
        public SpecularMap(int width, int height) : base(width, height) { }

        public override float GetValue(float x, float y)
        {
            return _values[GetMapYCoordinate(y) * Width + GetMapXCoordinate(x)];
        }

        public override void SetValue(int x, int y, float value)
        {
            _values[y * Width + x] = value;
        }
    }
}
